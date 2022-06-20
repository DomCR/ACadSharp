using System.IO;

namespace ACadSharp.IO.DWG
{
	internal class DwgStreamReaderAC12 : DwgStreamReaderBase
	{
		public DwgStreamReaderAC12(Stream stream, bool resetPosition) : base(stream, resetPosition) { }
	
		public override string ReadVariableText()
		{
			short length = this.ReadBitShort();
			string str;
			if (length > 0)
			{
				str = this.ReadString(length, this.Encoding).Replace("\0", "");
			}
			else
				str = string.Empty;
			return str;
		}
	}
}
