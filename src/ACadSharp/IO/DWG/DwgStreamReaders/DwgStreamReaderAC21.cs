using CSUtilities.Converters;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgStreamReaderAC21 : DwgStreamReaderAC18
	{
		public DwgStreamReaderAC21(Stream stream, bool resetPosition) : base(stream, resetPosition) { }
		
		public override string ReadTextUnicode()
		{
			short textLength = this.ReadShort<LittleEndianConverter>();
			string value;
			if (textLength == 0)
			{
				value = string.Empty;
			}
			else
			{
				//Correct the text length by shifting 1 bit
				short length = (short)(textLength << 1);
				//Read the string and get rid of the empty bytes
				value = this.ReadString(length, Encoding.Unicode).Replace("\0", "");
			}
			return value;
		}
	
		public override string ReadVariableText()
		{
			int textLength = this.ReadBitShort();
			string value;
			if (textLength == 0)
			{
				value = string.Empty;
			}
			else
			{
				//Correct the text length by shifting 1 bit
				short length = (short)(textLength << 1);
				//Read the string and get rid of the empty bytes
				value = this.ReadString(length, Encoding.Unicode).Replace("\0", "");
			}
			return value;
		}
	}
}
