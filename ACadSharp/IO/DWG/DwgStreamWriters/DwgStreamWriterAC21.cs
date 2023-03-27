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
			base.WriteBitShort((short)value.Length);
			base.WriteBytes(Encoding.Unicode.GetBytes(value));
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
