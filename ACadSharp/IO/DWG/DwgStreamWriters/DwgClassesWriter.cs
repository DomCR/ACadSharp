using CSUtilities.IO;
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

		private IDwgStreamWriter _endWriter;

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
			this._startWriter = DwgStreamWriterBase.GetStreamHandler(version, stream, Encoding.Default);

			this._sectionStream = new MemoryStream();
			this._endWriter = DwgStreamWriterBase.GetStreamHandler(version, _sectionStream, Encoding.Default);
		}

		public void Write()
		{
			if (R2007Plus)
			{
				this._endWriter.SavePositonForSize();
			}

			short maxClassNumber = 0;
			if (this._model.Classes.Any())
			{
				maxClassNumber = this._model.Classes.Max(c => c.ClassNumber);
			}

			if (this.R2004Plus)
			{
				//BS : Maxiumum class number
				this._endWriter.WriteBitShort(maxClassNumber);
				//RC: 0x00
				this._endWriter.WriteByte(0);
				//RC: 0x00
				this._endWriter.WriteByte(0);
				//B : true
				this._endWriter.WriteBit(true);
			}

			foreach (var c in this._model.Classes)
			{
				this._endWriter.WriteBitShort(c.ClassNumber);
				this._endWriter.WriteBitShort((short)c.ProxyFlags);
				this._endWriter.WriteVariableText(c.ApplicationName);
				this._endWriter.WriteVariableText(c.CppClassName);
				this._endWriter.WriteVariableText(c.DxfName);
				this._endWriter.WriteBit(c.WasZombie);
				this._endWriter.WriteBitShort(c.ItemClassId);

				if (this.R2004Plus)
				{
					//BL : Number of objects created of this type in the current DB(DXF 91).
					this._endWriter.WriteBitLong(1);
					this._endWriter.WriteBitShort((short)this._model.Header.Version);
					this._endWriter.WriteBitShort(this._model.Header.MaintenanceVersion);
					this._endWriter.WriteBitLong(0);
					this._endWriter.WriteBitLong(0);
				}
			}

			this._endWriter.WriteSpearShift();

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
				swriter.Write((int)0);
			}

			//Write the section
			this._startWriter.WriteBytes(this._sectionStream.GetBuffer());

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
