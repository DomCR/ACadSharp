using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp
{
	public struct Color
	{
		public static Color ByLayer { get { return new Color(256); } }
		public static Color ByBlock { get { return new Color(0); } }

		/// <summary>
		/// Defines if the color is defined by layer.
		/// </summary>
		public bool IsByLayer
		{
			get { return Index == 256; }
		}
		/// <summary>
		/// Defines if the color is defined by block.
		/// </summary>
		public bool IsByBlock
		{
			get { return Index == 0; }
		}
		public short Index { get; set; }

		public Color(short cnumber)
		{
			Index = -1;
		}
	}
}
