using ACadSharp.Header;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.IO.DWG
{
	internal class DwgHeaderWriter : DwgSectionIO
	{
		private MemoryStream _msbegin;
		private MemoryStream _msmain;

		private IDwgStreamWriter _swbegin;
		private IDwgStreamWriter _swmain;

		private Stream _stream;

		private CadDocument _document;
		private CadHeader _header;

		public DwgHeaderWriter(Stream stream, CadDocument document) : base(document.Header.Version)
		{
			this._stream = stream;
			this._document = document;
			this._header = document.Header;

			this._msbegin = new MemoryStream();
			this._msmain = new MemoryStream();

			this._swbegin = DwgStreamWriter.GetStreamHandler(document.Header.Version, this._msbegin, Encoding.Default);
			this._swmain = DwgStreamWriter.GetStreamHandler(document.Header.Version, _msmain, Encoding.Default);
		}

		public void Write()
		{

			this._stream.Write(this._msbegin.GetBuffer(), 0, (int)_msbegin.Length);
			this._stream.Write(this._msmain.GetBuffer(), 0, (int)_msmain.Length);
		}

		public void writeSectionBegin()
		{
			//0xCF,0x7B,0x1F,0x23,0xFD,0xDE,0x38,0xA9,0x5F,0x7C,0x68,0xB8,0x4E,0x6D,0x33,0x5F
			_swbegin.WriteBytes(DwgSectionDefinition.StartSentinels[DwgSectionDefinition.Header]);

			//RL : Size of the section.
			_swbegin.WriteRawLong(this._msmain.Length);
		}
	}
}
