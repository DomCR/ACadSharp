using CSUtilities.Text;
using System;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters
{
	internal abstract class DwgFileHeaderWriterBase
	{
		public DwgFileHeader _fileHeader { get; }

		public ACadVersion _version;

		protected Encoding _encoding;

		protected Stream _stream;

		protected CadDocument _document;

		public DwgFileHeaderWriterBase(Stream stream, CadDocument model)
		{
			if (!stream.CanSeek || !stream.CanWrite)
			{
				throw new ArgumentException();
			}

			this._document = model;
			this._stream = stream;
			this._version = model.Header.Version;
			this._encoding = TextEncoding.Windows1252();
		}
	}
}
