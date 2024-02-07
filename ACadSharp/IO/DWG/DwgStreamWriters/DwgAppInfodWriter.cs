using System.IO;
using System.Reflection;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgAppInfoWriter : DwgSectionIO
	{
		public override string SectionName => DwgSectionDefinition.AppInfo;

		private IDwgStreamWriter _writer;

		private byte[] _emptyArr = new byte[16];

		public DwgAppInfoWriter(ACadVersion version, Stream stream) : base(version)
		{
			this._writer = DwgStreamWriterBase.GetStreamWriter(version, stream, Encoding.Unicode);
		}

		public void Write()
		{
			string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			//UInt32	4	class_version (default: 3)
			_writer.WriteInt(3);
			//String	2 + 2 * n + 2	App info name, ODA writes “AppInfoDataList”
			_writer.WriteTextUnicode("AppInfoDataList");
			//UInt32	4	num strings (default: 3)
			_writer.WriteInt(3);
			//Byte[]	16	Version data(checksum, ODA writes zeroes)
			_writer.WriteBytes(_emptyArr);
			//String	2 + 2 * n + 2	Version
			_writer.WriteTextUnicode(version);
			//Byte[]	16	Comment data(checksum, ODA writes zeroes)
			_writer.WriteBytes(_emptyArr);
			//String	2 + 2 * n + 2	Comment
			_writer.WriteTextUnicode("This is a comment from ACadSharp");
			//Byte[]	16	Product data(checksum, ODA writes zeroes)
			_writer.WriteBytes(_emptyArr);
			//String	2 + 2 * n + 2	Product
			_writer.WriteTextUnicode($"<ProductInformation name =\"ACadSharp\" build_version=\"{version}\" registry_version=\"{version}\" install_id_string=\"ACadSharp\" registry_localeID=\"1033\"/>");
		}
	}
}
