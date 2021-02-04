using System;
using System.Collections.Generic;
using System.Text;

namespace CSUtilities.Converters
{
	internal class BigEndianConverter : EndianConverter
	{
		public static BigEndianConverter Instance = new BigEndianConverter();
		static IEndianConverter init()
		{
			if (BitConverter.IsLittleEndian)
				return (IEndianConverter)new InverseConverter();
			else
				return (IEndianConverter)new DefaultEndianConverter();
		}
		public BigEndianConverter() : base(init()) { }
	}
}
