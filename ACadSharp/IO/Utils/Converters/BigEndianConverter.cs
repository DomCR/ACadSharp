using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO.Utils.Converters
{
	internal class BigEndianConverter : EndianConverter
	{
		public static BigEndianConverter Instance = new BigEndianConverter();
		static IEndianConverter init()
		{
			if (BitConverter.IsLittleEndian)
				return new InverseConverter();
			else
				return new DefaultEndianConverter();
		}
		public BigEndianConverter() : base(init()) { }
	}
}
