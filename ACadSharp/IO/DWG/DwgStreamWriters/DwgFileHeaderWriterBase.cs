using CSUtilities.Converters;
using CSUtilities.IO;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters
{
	internal abstract class DwgFileHeaderWriterBase: IDwgFileHeaderWriter
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

		public abstract void Init();

		public abstract void CreateSection(string name, MemoryStream stream, bool isCompressed, int decompsize = 0x7400);

		public abstract void WriteFile();
	}

	internal class DwgFileHeaderWriterAC15 : DwgFileHeaderWriterBase
	{
		public DwgFileHeaderWriterAC15(Stream stream, CadDocument model) : base(stream, model)
		{
		}

		public override void CreateSection(string name, MemoryStream stream, bool isCompressed, int decompsize = 29696)
		{
			throw new NotImplementedException();
		}

		public override void Init()
		{
			throw new NotImplementedException();
		}

		public override void WriteFile()
		{
			throw new NotImplementedException();
		}
	}
}
