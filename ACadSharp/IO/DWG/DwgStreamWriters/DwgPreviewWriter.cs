using CSUtilities.IO;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgPreviewWriter : DwgSectionIO
	{
		private IDwgStreamWriter _swriter;

		private readonly byte[] _startSentinel = new byte[16]
		{
			0x1F,0x25,0x6D,0x07,0xD4,0x36,0x28,0x28,0x9D,0x57,0xCA,0x3F,0x9D,0x44,0x10,0x2B
		};

		private readonly byte[] _endSentinel = new byte[16]
		{
			0xE0, 0xDA, 0x92, 0xF8, 0x2B, 0xC9, 0xD7, 0xD7, 0x62, 0xA8, 0x35, 0xC0, 0x62, 0xBB, 0xEF, 0xD4
		};

		public DwgPreviewWriter(ACadVersion version, Stream stream) : base(version)
		{
			this._swriter = DwgStreamWriterBase.GetStreamHandler(version, stream, Encoding.Default);
		}

		public void Write()
		{
			this._swriter.WriteBytes(_startSentinel);
			this._swriter.WriteRawLong(1);
			this._swriter.WriteByte(0);
			this._swriter.WriteBytes(_endSentinel);
		}
	}

	internal class DwgAppInfodWriter : DwgSectionIO
	{
		private IDwgStreamWriter writer;

		private byte[] _emptyArr = new byte[16];

		public DwgAppInfodWriter(ACadVersion version, Stream stream) : base(version)
		{
			this.writer = DwgStreamWriterBase.GetStreamHandler(version, stream, Encoding.Default);
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
			writer.WriteTextUnicode("<ProductInformation name =\"ACadSharp\" build_version=\"_version_\" registry_version=\"2.0.39.12\" install_id_string=\"ACadSharp\" registry_localeID=\"1033\"/>");
		}
	}

}
