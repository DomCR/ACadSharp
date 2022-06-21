using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgObjectWriter : DwgSectionIO
	{

		private MemoryStream _msbegin;
		private MemoryStream _msmain;

		private IDwgStreamWriter _swbegin;
		private IDwgStreamWriter _writer;

		private Stream _stream;

		private CadDocument _document;

		public DwgObjectWriter(Stream stream, CadDocument document) : base(document.Header.Version)
		{
			this._stream = stream;
			this._document = document;

			this._msbegin = new MemoryStream();
			this._msmain = new MemoryStream();

			this._swbegin = DwgStreamWriter.GetStreamHandler(document.Header.Version, this._msbegin, Encoding.Default);
			this._writer = DwgStreamWriter.GetStreamHandler(document.Header.Version, _msmain, Encoding.Default);
		}

		public void Write()
		{

		}
	}
}
