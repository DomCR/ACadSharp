using CSUtilities.IO;
using CSUtilities.Text;
using System.IO;
using System.Linq;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgClassesWriter : DwgSectionIO
	{
		private CadDocument _model;

		private MemoryStream _sectionStream;

		private IDwgStreamWriter _startWriter;

		private IDwgStreamWriter _writer;

		private readonly byte[] _startSentinel = new byte[16]
		{
			0x8D, 0xA1, 0xC4, 0xB8, 0xC4, 0xA9, 0xF8, 0xC5, 0xC0, 0xDC, 0xF4, 0x5F, 0xE7, 0xCF, 0xB6, 0x8A
		};

		private readonly byte[] _endSentinel = new byte[16]
		{
			0x72, 0x5E, 0x3B, 0x47, 0x3B, 0x56, 0x07, 0x3A, 0x3F, 0x23, 0x0B, 0xA0, 0x18, 0x30, 0x49, 0x75
		};

		public DwgClassesWriter(CadDocument document, ACadVersion version, Stream stream) : base(version)
		{
			this._model = document;
			this._startWriter = DwgStreamWriterBase.GetStreamHandler(version, stream, TextEncoding.Windows1252());

			this._sectionStream = new MemoryStream();
			this._writer = DwgStreamWriterBase.GetStreamHandler(version, _sectionStream, TextEncoding.Windows1252());
		}

		public void Write()
		{
			if (R2007Plus)
			{
				this._writer.SavePositonForSize();
			}

			short maxClassNumber = 0;
			if (this._model.Classes.Any())
			{
				maxClassNumber = this._model.Classes.Max(c => c.ClassNumber);
			}

			if (this.R2004Plus)
			{
				//BS : Maxiumum class number
				this._writer.WriteBitShort(maxClassNumber);
				//RC: 0x00
				this._writer.WriteByte(0);
				//RC: 0x00
				this._writer.WriteByte(0);
				//B : true
				this._writer.WriteBit(true);
			}

			foreach (var c in this._model.Classes)
			{
				this._writer.WriteBitShort(c.ClassNumber);
				this._writer.WriteBitShort((short)c.ProxyFlags);
				this._writer.WriteVariableText(c.ApplicationName);
				this._writer.WriteVariableText(c.CppClassName);
				this._writer.WriteVariableText(c.DxfName);
				this._writer.WriteBit(c.WasZombie);
				this._writer.WriteBitShort(c.ItemClassId);

				if (this.R2004Plus)
				{
					//BL : Number of objects created of this type in the current DB(DXF 91).
					this._writer.WriteBitLong(1);
					this._writer.WriteBitShort((short)this._model.Header.Version);
					this._writer.WriteBitShort(this._model.Header.MaintenanceVersion);
					this._writer.WriteBitLong(0);
					this._writer.WriteBitLong(0);
				}
			}

			this._writer.WriteSpearShift();

			this.writeSizeAndCrc();
		}

		private void writeSizeAndCrc()
		{
			this._startWriter.WriteBytes(_startSentinel);

			CRC8StreamHandler crc = new CRC8StreamHandler(this._startWriter.Stream, 0xC0C1);
			StreamIO swriter = new StreamIO(crc);

			swriter.Write((int)this._sectionStream.Length);

			if (this._model.Header.Version >= ACadVersion.AC1024
				&& this._model.Header.MaintenanceVersion > 3
				|| this._model.Header.Version > ACadVersion.AC1027)
			{
				//RL : unknown, possibly the high 32 bits of a 64-bit size?
				swriter.Write((int)0);  //TODO: Define endian order!!!
			}

			//Write the section
			swriter.Stream.Write(this._sectionStream.GetBuffer(), 0, (int)this._sectionStream.Length);

			//RS: CRC
			this._startWriter.WriteRawShort(crc.Seed);

			this._startWriter.WriteBytes(this._endSentinel);

			if (this.R2004Plus)
			{
				//For R18 and later 8 unknown bytes follow. The ODA writes 0 bytes.
				_startWriter.WriteRawLong(0);
				_startWriter.WriteRawLong(0);
			}
		}
	}
}
