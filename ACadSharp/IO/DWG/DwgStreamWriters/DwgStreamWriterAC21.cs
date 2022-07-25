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
			base.WriteBytes(System.Text.Encoding.Unicode.GetBytes(value));
		}
	}
}
