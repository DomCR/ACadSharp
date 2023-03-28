using CSUtilities.IO;
using CSUtilities.Text;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgPreviewWriter : DwgSectionIO
	{
		public override string SectionName => DwgSectionDefinition.Preview;

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
			this._swriter = DwgStreamWriterBase.GetStreamHandler(version, stream, TextEncoding.Windows1252());
		}

		public void Write()
		{
			this._swriter.WriteBytes(_startSentinel);
			this._swriter.WriteRawLong(1);
			this._swriter.WriteByte(0);
			this._swriter.WriteBytes(_endSentinel);
		}
	}
}
