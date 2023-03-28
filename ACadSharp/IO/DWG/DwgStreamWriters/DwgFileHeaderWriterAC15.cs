using CSUtilities.Converters;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ACadSharp.IO.DWG
{
	/*
	HEADER
		FILE HEADER
		DWG HEADER VARIABLES
		CRC
	CLASS DEFINITIONS
	TEMPLATE (R13 only, optional)
	PADDING (R13C3 AND LATER, 200 bytes, minutes the template section above if present)
	IMAGE DATA (PRE-R13C3)
	OBJECT DATA
		All entities, table entries, dictionary entries, etc. go in this
		section.
	OBJECT MAP
	OBJECT FREE SPACE (optional)
	TEMPLATE (R14-R15, optional)
	SECOND HEADER
	IMAGE DATA (R13C3 AND LATER)
	 */
	internal class DwgFileHeaderWriterAC15 : DwgFileHeaderWriterBase
	{
		public override int HandleSectionOffset
		{
			get
			{
				long offset = _fileHeaderSize;

				foreach (var item in this._records)
				{
					if (item.Key == DwgSectionDefinition.AcDbObjects)
						break;

					offset += item.Value.Item2.Length;
				}

				return (int)offset;
			}
		}

		protected override int _fileHeaderSize { get { return 0x61; } }

		private readonly Dictionary<string, (DwgSectionLocatorRecord, MemoryStream)> _records;

		private byte[] _endSentinel = new byte[16]
		{
			0x95,0xA0,0x4E,0x28,0x99,0x82,0x1A,0xE5,0x5E,0x41,0xE0,0x5F,0x9D,0x3A,0x4D,0x00
		};

		public DwgFileHeaderWriterAC15(Stream stream, CadDocument model) : base(stream, model)
		{
			_records = new Dictionary<string, (DwgSectionLocatorRecord, MemoryStream)>
			{
				{ DwgSectionDefinition.Header      , (new DwgSectionLocatorRecord(0), null) },
				{ DwgSectionDefinition.Classes     , (new DwgSectionLocatorRecord(1), null) },
				{ DwgSectionDefinition.ObjFreeSpace, (new DwgSectionLocatorRecord(3), null) },
				{ DwgSectionDefinition.Template    , (new DwgSectionLocatorRecord(4), null) },
				{ DwgSectionDefinition.AuxHeader   , (new DwgSectionLocatorRecord(5), null) },
				{ DwgSectionDefinition.AcDbObjects , (new DwgSectionLocatorRecord(null), null) },
				{ DwgSectionDefinition.Handles     , (new DwgSectionLocatorRecord(2), null) },
				{ DwgSectionDefinition.Preview     , (new DwgSectionLocatorRecord(null), null) },
			};
		}

		public override void AddSection(string name, MemoryStream stream, bool isCompressed, int decompsize = 0x7400)
		{
			this._records[name].Item1.Size = stream.Length;
			this._records[name] = (this._records[name].Item1, stream);
		}

		public override void WriteFile()
		{
			setRecordSeekers();

			writeFileHeader();

			writeRecordStreams();
		}

		private void setRecordSeekers()
		{
			long currOffset = _fileHeaderSize;
			foreach (var item in this._records.Values)
			{
				item.Item1.Seeker = currOffset;
				currOffset += item.Item2.Length;
			}
		}

		private void writeFileHeader()
		{
			MemoryStream memoryStream = new MemoryStream();

			//0x00	6	“ACXXXX” version string
			IDwgStreamWriter writer = DwgStreamWriterBase.GetStreamHandler(this._version, memoryStream, this._encoding);
			writer.WriteBytes(Encoding.ASCII.GetBytes(this._document.Header.VersionString));
			//The next 7 starting at offset 0x06 are to be six bytes of 0 
			//(in R14, 5 0’s and the ACADMAINTVER variable) and a byte of 1.
			writer.WriteBytes(new byte[7] { 0, 0, 0, 0, 0, 15, 1 });
			//At 0x0D is a seeker (4 byte long absolute address) for the beginning sentinel of the image data.
			writer.WriteRawLong(this._records[DwgSectionDefinition.Preview].Item1.Seeker);

			writer.WriteByte(0x1B);
			writer.WriteByte(0x19);

			//Bytes at 0x13 and 0x14 are a raw short indicating the value of the code page for this drawing file.
			writer.WriteBytes(LittleEndianConverter.Instance.GetBytes((short)30));
			writer.WriteBytes(LittleEndianConverter.Instance.GetBytes(6));

			foreach (var item in this._records.Values.Select(r => r.Item1))
			{
				if (!item.Number.HasValue)
					continue;

				this.writeRecord(writer, item);
			}

			//CRC
			writer.WriteSpearShift();
			writer.WriteRawShort((short)CRC8StreamHandler.GetCRCValue(0xC0C1, memoryStream.GetBuffer(), 0L, memoryStream.Length));

			//0x95,0xA0,0x4E,0x28,0x99,0x82,0x1A,0xE5,0x5E,0x41,0xE0,0x5F,0x9D,0x3A,0x4D,0x00
			writer.WriteBytes(_endSentinel);

			this._stream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
		}

		private void writeRecord(IDwgStreamWriter writer, DwgSectionLocatorRecord record)
		{
			//Record number (raw byte) | Seeker (raw long) | Size (raw long)
			writer.WriteByte((byte)record.Number.Value);
			writer.WriteRawLong(record.Seeker);
			writer.WriteRawLong(record.Size);
		}

		private void writeRecordStreams()
		{
			foreach (var item in this._records.Values.Select(r => r.Item2))
			{
				this._stream.Write(item.GetBuffer(), 0, (int)item.Length);
			}
		}
	}
}
