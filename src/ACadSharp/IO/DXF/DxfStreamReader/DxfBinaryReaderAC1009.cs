using System.IO;
using System.Text;

namespace ACadSharp.IO.DXF
{
	internal class DxfBinaryReaderAC1009 : DxfBinaryReader
	{
		public DxfBinaryReaderAC1009(Stream stream, Encoding encoding) : base(stream, encoding)
		{
		}

		protected override DxfCode readCode()
		{
			int code = this._stream.ReadByte();
			if (code == byte.MaxValue)
			{
				code = this._stream.ReadInt16();
			}

			return (DxfCode)code;
		}
	}
}
