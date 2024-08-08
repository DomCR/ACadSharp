using CSUtilities.Converters;
using System.Runtime.CompilerServices;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamReaders
{
	internal class DwgAppInfoReader : DwgSectionIO
	{
		public override string SectionName => DwgSectionDefinition.AppInfo;

		private readonly IDwgStreamReader _reader;

		public DwgAppInfoReader(ACadVersion version, IDwgStreamReader reader) : base(version)
		{
			this._reader = reader;
		}

		public void Read()
		{
			if (!this.R2007Plus)
			{
				this.readR18();
			}

			//UInt32	4	Unknown(ODA writes 2)
			int unknown1 = this._reader.ReadInt();
			//String	2 + 2 * n + 2	App info name, ODA writes “AppInfoDataList”
			string infoname = this._reader.ReadTextUnicode();
			//UInt32	4	Unknown(ODA writes 3)
			int unknown2 = this._reader.ReadInt();
			//Byte[]	16	Version data(checksum, ODA writes zeroes)
			byte[] bytes = this._reader.ReadBytes(16);
			//String	2 + 2 * n + 2	Version
			string version = this._reader.ReadTextUnicode();
			//Byte[]	16	Comment data(checksum, ODA writes zeroes)
			byte[] comm = this._reader.ReadBytes(16);

			if (!this.R2010Plus)
			{
				return;
			}

			//String	2 + 2 * n + 2	Comment
			string comment = this._reader.ReadTextUnicode();
			//Byte[]	16	Product data(checksum, ODA writes zeroes)
			byte[] product = this._reader.ReadBytes(16);
			//String	2 + 2 * n + 2	Product
			string xml = this._reader.ReadTextUnicode();
		}

		private void readR18()
		{
			//For this version the values don't match with the documentaiton

			//String	2 + 2 * n + 2	App info name, ODA writes “AppInfoDataList”
			string infoname = this._reader.ReadVariableText();
			//UInt32	4	Unknown(ODA writes 2)
			int unknown2 = this._reader.ReadInt();
			//Unknown, ODA writes "4001"
			string version = this._reader.ReadVariableText();
			//String	2 + n App info product XML element, e.g. ODA writes “< ProductInformation name = "Teigha" build_version = "0.0" registry_version = "3.3" install_id_string = "ODA" registry_localeID = "1033" /> "
			string xml = this._reader.ReadVariableText();
			//String	2 + n	App info version, e.g. ODA writes "2.7.2.0"
			string comment = this._reader.ReadVariableText();
		}
	}
}
