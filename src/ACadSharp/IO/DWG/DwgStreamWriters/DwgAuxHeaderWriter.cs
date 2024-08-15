using ACadSharp.Header;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters
{
	internal class DwgAuxHeaderWriter : DwgSectionIO
	{
		public override string SectionName => DwgSectionDefinition.AuxHeader;

		private MemoryStream _stream;
		private Encoding _encoding;
		private CadHeader _header;
		private IDwgStreamWriter _writer;

		public DwgAuxHeaderWriter(MemoryStream stream, Encoding encoding, CadHeader header)
			: base(header.Version)
		{
			this._stream = stream;
			this._encoding = encoding;
			this._header = header;

			this._writer = DwgStreamWriterBase.GetStreamWriter(this._version, this._stream, encoding);
		}

		public void Write()
		{
			//RC: 0xff 0x77 0x01
			this._writer.WriteByte(0xFF);
			this._writer.WriteByte(0x77);
			this._writer.WriteByte(0x01);

			//RS: DWG version:
			//AC1010 = 17,
			//AC1011 = 18,
			//AC1012 = 19,
			//AC1013 = 20,
			//AC1014 = 21,
			//AC1015(beta) = 22,
			//AC1015 = 23,
			//AC1018(beta) = 24,
			//AC1018 = 25,
			//AC1021(beta) = 26,
			//AC1021 = 27,
			//AC1024(beta) = 28,
			//AC1024 = 29
			//AC1027(beta) = 30,
			//AC1027 = 31,
			//AC1032(beta) = 32,
			//AC1032 = 33
			this._writer.WriteRawShort((short)this._version);

			//RS: Maintenance version
			this._writer.WriteRawShort(this._header.MaintenanceVersion);

			//RL: Number of saves (starts at 1)
			this._writer.WriteRawLong(1);
			//RL: -1
			this._writer.WriteRawLong(-1);

			//RS: Number of saves part 1( = Number of saves – number of saves part 2)
			this._writer.WriteRawShort(1);
			//RS: Number of saves part 2( = Number of saves – 0x7fff if Number of saves > 0x7fff, otherwise 0)
			this._writer.WriteRawShort(0);

			//RL: 0
			this._writer.WriteRawLong(0);
			//RS: DWG version string
			this._writer.WriteRawShort((short)this._version);
			//RS : Maintenance version
			this._writer.WriteRawShort((short)this._header.MaintenanceVersion);
			//RS: DWG version string
			this._writer.WriteRawShort((short)this._version);
			//RS : Maintenance version
			this._writer.WriteRawShort((short)this._header.MaintenanceVersion);

			//RS: 0x0005
			this._writer.WriteRawShort(0x5);
			//RS: 0x0893
			this._writer.WriteRawShort(2195);
			//RS: 0x0005
			this._writer.WriteRawShort(5);
			//RS: 0x0893
			this._writer.WriteRawShort(2195);
			//RS: 0x0000
			this._writer.WriteRawShort(0);
			//RS: 0x0001
			this._writer.WriteRawShort(1);
			//RL: 0x0000
			this._writer.WriteRawLong(0);
			//RL: 0x0000
			this._writer.WriteRawLong(0);
			//RL: 0x0000
			this._writer.WriteRawLong(0);
			//RL: 0x0000
			this._writer.WriteRawLong(0);
			//RL: 0x0000
			this._writer.WriteRawLong(0);

			//TD: TDCREATE(creation datetime)
			this._writer.Write8BitJulianDate(this._header.CreateDateTime);

			//TD: TDUPDATE(update datetime)
			this._writer.Write8BitJulianDate(this._header.UpdateDateTime);

			int handseed = -1;
			if (this._header.HandleSeed <= 0x7FFFFFFF)
			{
				handseed = (int)this._header.HandleSeed;
			}

			//RL: HANDSEED(Handle seed) if < 0x7fffffff, otherwise - 1.
			this._writer.WriteRawLong(handseed);
			//RL : Educational plot stamp(default value is 0)
			this._writer.WriteRawLong(0);
			//RS: 0
			this._writer.WriteRawShort(0);
			//RS: Number of saves part 1 – number of saves part 2
			this._writer.WriteRawShort(1);
			//RL: 0
			this._writer.WriteRawLong(0);
			//RL: 0
			this._writer.WriteRawLong(0);
			//RL: 0
			this._writer.WriteRawLong(0);
			//RL: Number of saves
			this._writer.WriteRawLong(1);
			//RL : 0
			this._writer.WriteRawLong(0);
			//RL: 0
			this._writer.WriteRawLong(0);
			//RL: 0
			this._writer.WriteRawLong(0);
			//RL: 0
			this._writer.WriteRawLong(0);

			//R2018 +
			if (this.R2018Plus)
			{
				//RS : 0
				this._writer.WriteRawShort(0);
				//RS : 0
				this._writer.WriteRawShort(0);
				//RS : 0
				this._writer.WriteRawShort(0);
			}
		}
	}
}
