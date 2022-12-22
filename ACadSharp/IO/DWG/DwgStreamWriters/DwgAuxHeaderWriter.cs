using ACadSharp.Header;
using CSUtilities.Text;
using System.IO;

namespace ACadSharp.IO.DWG.DwgStreamWriters
{
	internal class DwgAuxHeaderWriter : DwgSectionIO
	{
		public override string SectionName => DwgSectionDefinition.AuxHeader;

		private CadHeader _header;

		private MemoryStream _stream;

		public DwgAuxHeaderWriter(MemoryStream stream, CadHeader header)
			: base(header.Version)
		{
			this._stream = stream;
			this._header = header;
		}

		public void Write()
		{
			IDwgStreamWriter writer = DwgStreamWriterBase.GetStreamHandler(this._version, this._stream, TextEncoding.Windows1252());

			//RC: 0xff 0x77 0x01
			writer.WriteByte(0xFF);
			writer.WriteByte(0x77);
			writer.WriteByte(0x01);

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
			writer.WriteRawShort((short)this._version);

			//RS: Maintenance version
			writer.WriteRawShort(this._header.MaintenanceVersion);

			//RL: Number of saves (starts at 1)
			writer.WriteRawLong(1);
			//RL: -1
			writer.WriteRawLong(-1);

			//RS: Number of saves part 1( = Number of saves – number of saves part 2)
			writer.WriteRawShort(1);
			//RS: Number of saves part 2( = Number of saves – 0x7fff if Number of saves > 0x7fff, otherwise 0)
			writer.WriteRawShort(0);

			//RL: 0
			writer.WriteRawLong(0);
			//RS: DWG version string
			writer.WriteRawShort((short)_version);
			//RS : Maintenance version
			writer.WriteRawShort((short)this._header.MaintenanceVersion);
			//RS: DWG version string
			writer.WriteRawShort((short)_version);
			//RS : Maintenance version
			writer.WriteRawShort((short)this._header.MaintenanceVersion);

			//RS: 0x0005
			writer.WriteRawShort(0x5);
			//RS: 0x0893
			writer.WriteRawShort(2195);
			//RS: 0x0005
			writer.WriteRawShort(5);
			//RS: 0x0893
			writer.WriteRawShort(2195);
			//RS: 0x0000
			writer.WriteRawShort(0);
			//RS: 0x0001
			writer.WriteRawShort(1);
			//RL: 0x0000
			writer.WriteRawLong(0);
			//RL: 0x0000
			writer.WriteRawLong(0);
			//RL: 0x0000
			writer.WriteRawLong(0);
			//RL: 0x0000
			writer.WriteRawLong(0);
			//RL: 0x0000
			writer.WriteRawLong(0);

			//TD: TDCREATE(creation datetime)
			writer.Write8BitJulianDate(this._header.CreateDateTime);

			//TD: TDUPDATE(update datetime)
			writer.Write8BitJulianDate(this._header.UpdateDateTime);

			int handseed = -1;
			if (this._header.HandleSeed <= 0x7FFFFFFF)
			{
				handseed = (int)this._header.HandleSeed;
			}

			//RL: HANDSEED(Handle seed) if < 0x7fffffff, otherwise - 1.
			writer.WriteRawLong(handseed);
			//RL : Educational plot stamp(default value is 0)
			writer.WriteRawLong(1);
			//RS: 0
			writer.WriteRawShort(0);
			//RS: Number of saves part 1 – number of saves part 2
			writer.WriteRawShort(1);
			//RL: 0
			writer.WriteRawLong(0);
			//RL: 0
			writer.WriteRawLong(0);
			//RL: 0
			writer.WriteRawLong(0);
			//RL: Number of saves
			writer.WriteRawLong(1);
			//RL : 0
			writer.WriteRawLong(0);
			//RL: 0
			writer.WriteRawLong(0);
			//RL: 0
			writer.WriteRawLong(0);
			//RL: 0
			writer.WriteRawLong(0);

			//R2018 +
			if (this.R2018Plus)
			{
				//RS : 0
				writer.WriteRawShort(0);
				//RS : 0
				writer.WriteRawShort(0);
				//RS : 0
				writer.WriteRawShort(0);
			}
		}
	}
}
