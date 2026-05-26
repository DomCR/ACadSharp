using ACadSharp.IO.DWG.FileHeaders;
using CSUtilities.Converters;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal class DwgFileHeaderWriterAC15 : DwgFileHeaderWriterBase<DwgFileHeaderAC15>
{
	public override int FileHeaderSize { get { return 0x61; } }

	public override int HandleSectionOffset
	{
		get
		{
			return (int)(FileHeaderSize
				+ this._records[DwgSectionDefinition.AuxHeader].Stream.Length
				+ this._records[DwgSectionDefinition.Preview].Stream.Length
				+ this._records[DwgSectionDefinition.Header].Stream.Length
				+ this._records[DwgSectionDefinition.Classes].Stream.Length);
		}
	}

	private const int _nRecords = 6;

	private byte[] _endSentinel = new byte[16]
	{
		0x95,0xA0,0x4E,0x28,0x99,0x82,0x1A,0xE5,0x5E,0x41,0xE0,0x5F,0x9D,0x3A,0x4D,0x00
	};

	private Dictionary<string, DwgSectionLocatorRecord> _records = new();

	public DwgFileHeaderWriterAC15(
		System.IO.Stream stream,
		Encoding encoding,
		CadDocument document)
		: base(stream, encoding, document)
	{
		this._records = new Dictionary<string, DwgSectionLocatorRecord>
		{
			{ DwgSectionDefinition.AuxHeader   , new DwgSectionLocatorRecord(5) },
			{ DwgSectionDefinition.Preview     , new DwgSectionLocatorRecord(null) },
			{ DwgSectionDefinition.Header      , new DwgSectionLocatorRecord(0) },
			{ DwgSectionDefinition.Classes     , new DwgSectionLocatorRecord(1) },
			{ DwgSectionDefinition.AcDbObjects , new DwgSectionLocatorRecord(null) },
			{ DwgSectionDefinition.Handles     , new DwgSectionLocatorRecord(2) },
			{ DwgSectionDefinition.ObjFreeSpace, new DwgSectionLocatorRecord(3) },
			{ DwgSectionDefinition.Template    , new DwgSectionLocatorRecord(4) },
		};
	}

	public override void AddSection(string name, MemoryStream stream, bool isCompressed, int decompsize = 29696)
	{
		this._records[name].Stream = stream;
	}

	public override void WriteFile()
	{
		this.setSeekers();

		this.writeFileHeader();

		this.writeRecordStreams();

		this._stream.Flush();
	}

	private void setSeekers()
	{
		var currOffset = this.FileHeaderSize;
		foreach (var kv in this._records)
		{
			var record = kv.Value;
			record.Seeker = currOffset;
			currOffset += (int)record.Stream.Length;
		}
	}

	private void writeFileHeader()
	{
		System.IO.MemoryStream ms = new System.IO.MemoryStream();

		//0x00	6	“ACXXXX” version string
		IDwgStreamWriter writer = DwgStreamWriterBase.GetStreamWriter(this._version, ms, this._encoding);
		writer.WriteBytes(Encoding.ASCII.GetBytes(this._document.Header.VersionString), 0, 6);
		//The next 7 starting at offset 0x06 are to be six bytes of 0
		//(in R14, 5 0’s and the ACADMAINTVER variable) and a byte of 1.
		writer.WriteBytes(new byte[7] { 0, 0, 0, 0, 0, 15, 1 });
		//At 0x0D is a seeker (4 byte long absolute address) for the beginning sentinel of the image data.
		writer.WriteRawLong(this._records[DwgSectionDefinition.Preview].Seeker);

		writer.WriteByte(0x1B);
		writer.WriteByte(0x19);

		//Bytes at 0x13 and 0x14 are a raw short indicating the value of the code page for this drawing file.
		writer.WriteBytes(LittleEndianConverter.Instance.GetBytes(this.getFileCodePage()));
		writer.WriteBytes(LittleEndianConverter.Instance.GetBytes(_nRecords), 0, 4);

		this.writeRecord(writer, this._records[DwgSectionDefinition.Header]);
		this.writeRecord(writer, this._records[DwgSectionDefinition.Classes]);
		this.writeRecord(writer, this._records[DwgSectionDefinition.Handles]);
		this.writeRecord(writer, this._records[DwgSectionDefinition.ObjFreeSpace]);
		this.writeRecord(writer, this._records[DwgSectionDefinition.Template]);
		this.writeRecord(writer, this._records[DwgSectionDefinition.AuxHeader]);

		//CRC
		writer.WriteSpearShift();
		writer.WriteRawShort((short)CRC8StreamHandler.GetCRCValue(0xC0C1, ms.GetBuffer(), 0L, ms.Length));

		//0x95,0xA0,0x4E,0x28,0x99,0x82,0x1A,0xE5,0x5E,0x41,0xE0,0x5F,0x9D,0x3A,0x4D,0x00
		writer.WriteBytes(_endSentinel, 0, _endSentinel.Length);

		this._stream.Write(ms.GetBuffer(), 0, (int)ms.Length);
	}

	private void writeRecord(IDwgStreamWriter writer, DwgSectionLocatorRecord record)
	{
		//Record number (raw byte) | Seeker (raw long) | Size (raw long)
		writer.WriteByte((byte)record.Number.Value);
		writer.WriteRawLong(record.Seeker);
		writer.WriteRawLong(record.Stream.Length);
	}

	private void writeRecordStreams()
	{
		foreach (DwgSectionLocatorRecord item in this._records.Values)
		{
			if (item.Stream == null)
				continue;

			this._stream.Write(item.Stream.GetBuffer(), 0, (int)item.Stream.Length);
		}
	}
}