using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgAppInfoWriter : DwgSectionIO
	{
		public override string SectionName => DwgSectionDefinition.AppInfo;

		private IDwgStreamWriter writer;

		private byte[] _emptyArr = new byte[16];

		public DwgAppInfoWriter(ACadVersion version, Stream stream) : base(version)
		{
			this.writer = DwgStreamWriterBase.GetStreamHandler(version, stream, Encoding.Unicode);
		}

		public void Write()
		{
			//UInt32	4	Unknown(ODA writes 2)
			writer.WriteInt(2);
			//String	2 + 2 * n + 2	App info name, ODA writes “AppInfoDataList”
			writer.WriteTextUnicode("AppInfoDataList");
			//UInt32	4	Unknown(ODA writes 3)
			writer.WriteInt(3);
			//Byte[]	16	Version data(checksum, ODA writes zeroes)
			writer.WriteBytes(_emptyArr);
			//String	2 + 2 * n + 2	Version
			writer.WriteTextUnicode("_version_");
			//Byte[]	16	Comment data(checksum, ODA writes zeroes)
			writer.WriteBytes(_emptyArr);
			//String	2 + 2 * n + 2	Comment
			writer.WriteTextUnicode("This is a comment from ACadSharp");
			//Byte[]	16	Product data(checksum, ODA writes zeroes)
			writer.WriteBytes(_emptyArr);
			//String	2 + 2 * n + 2	Product
			writer.WriteTextUnicode("<ProductInformation name =\"ACadSharp\" build_version=\"_version_\" registry_version=\"_version_\" install_id_string=\"ACadSharp\" registry_localeID=\"1033\"/>");
		}
	}
}
