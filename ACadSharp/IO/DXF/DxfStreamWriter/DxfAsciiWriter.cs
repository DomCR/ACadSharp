using System.IO;

namespace ACadSharp.IO.DXF
{
	internal class DxfAsciiWriter : IDxfStreamWriter
	{
		private TextWriter _stream;

		public DxfAsciiWriter(StreamWriter stream)
		{
			this._stream = stream;
		}

		public void Write(DxfCode code, string value)
		{
			this._stream.WriteLine((int)code);
			this._stream.WriteLine(value);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this._stream.Dispose();
		}
	}
}
