using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO.Utils.Converters
{
	internal class LittleEndianConverter : EndianConverter
	{
		public static LittleEndianConverter Instance = new LittleEndianConverter();
		static IEndianConverter init()
		{
			if (BitConverter.IsLittleEndian)
				return new DefaultEndianConverter();
			else
				return new InverseConverter();
		}
		public LittleEndianConverter() : base(init()) { }
	}
}
