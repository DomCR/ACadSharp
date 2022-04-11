using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.IO.DXF
{
	public class DxfWriter : IDisposable
	{
		private CadDocument _document;
		private IDxfStreamWriter _writer;

		public DxfWriter(string filename, CadDocument document, bool binary)
			: this(File.Create(filename), document, binary)
		{
		}

		public DxfWriter(Stream stream, CadDocument document, bool binary)
		{
			var encoding = Encoding.UTF8;

			if (binary)
			{
				throw new NotImplementedException();
			}
			else
			{
				this._writer = new DxfAsciiWriter(new StreamWriter(stream, encoding));
			}

			this._document = document;
		}

		public void Write()
		{
			this.writeHeader();

			this._writer.Write(DxfCode.Start, DxfFileToken.EndOfFile);
		}


		/// <inheritdoc/>
		public void Dispose()
		{
			this._writer.Dispose();
		}

		public static void Write(string filename, CadDocument document, bool binary)
		{

		}

		public static void Write(Stream stream, CadDocument document, bool binary)
		{

		}

		private void writeHeader()
		{
			new DwgHeaderWriter(this._writer, this._document).Write();
		}
	}
}
