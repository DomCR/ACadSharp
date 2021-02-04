using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp
{
	public struct Color
	{
		public static Color ByLayer { get { return new Color(256); } }
		public static Color ByBlock { get { return new Color(0); } }

		public short Index { get; set; }

		public Color(short cnumber)
		{
			Index = -1;
		}

	}
}
