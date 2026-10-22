using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgStreamWriterAC21 : DwgStreamWriterAC18
	{
		public DwgStreamWriterAC21(Stream stream, Encoding encoding) : base(stream, encoding)
		{
		}

		public override void WriteVariableText(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				base.WriteBitShort(0);
				return;
			}
			//[FIX] Consistent with WriteTextUnicode: TU length includes the trailing null wide character; the string is terminated with a 2-byte 0
			base.WriteBitShort((short)(value.Length + 1));
			base.WriteBytes(Encoding.Unicode.GetBytes(value));
			//[FIX] Same as WriteVariableText: the null is written via the bit writer; never write directly to the raw Stream
			base.WriteByte(0);
			base.WriteByte(0);
		}

		public override void WriteTextUnicode(string value)
		{
			this.WriteRawShort((short)(value.Length + 1));
			byte[] bytes = Encoding.Unicode.GetBytes(value);

			this.WriteBytes(bytes);
			base.Stream.WriteByte(0);
			base.Stream.WriteByte(0);
		}
	}
}
