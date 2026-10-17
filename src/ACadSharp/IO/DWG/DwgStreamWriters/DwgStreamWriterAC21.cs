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
			//[FIX] 与 WriteTextUnicode 一致：TU 长度含结尾 null 宽字符，字符串以 2 字节 0 终止
			base.WriteBitShort((short)(value.Length + 1));
			base.WriteBytes(Encoding.Unicode.GetBytes(value));
			//[FIX] 同 WriteVariableText：null 走位写入器，禁止直写原始 Stream
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
