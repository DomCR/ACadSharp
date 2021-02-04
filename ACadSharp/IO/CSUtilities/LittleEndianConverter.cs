using System;
using System.Collections.Generic;
using System.Text;

namespace CSUtilities.Converters
{
	internal class LittleEndianConverter : EndianConverter
	{
		public static LittleEndianConverter Instance = new LittleEndianConverter();
		static IEndianConverter init()
		{
			if (BitConverter.IsLittleEndian)
				return (IEndianConverter)new DefaultEndianConverter();
			else
				return (IEndianConverter)new InverseConverter();
		}
		public LittleEndianConverter() : base(init()) { }
	}
}
