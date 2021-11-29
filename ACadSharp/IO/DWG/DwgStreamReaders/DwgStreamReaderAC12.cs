using ACadSharp.Types;
using CSUtilities.Converters;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgStreamReaderAC12 : DwgStreamReader
	{
		public DwgStreamReaderAC12(Stream stream, bool resetPosition) : base(stream, resetPosition) { }
		public override string ReadVariableText()
		{
			short length = ReadBitShort();
			string str;
			if (length > 0)
			{
				str = ReadString(length, Encoding).Replace("\0", "");
			}
			else
				str = string.Empty;
			return str;
		}
	}
}
