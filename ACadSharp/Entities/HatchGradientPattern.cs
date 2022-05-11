using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public class HatchGradientPattern : HatchPattern
	{
		/// <summary>
		/// Zero is reserved for future use
		/// </summary>
		[DxfCodeValue(451)]
		internal int Reserved { get; set; }

		/// <summary>
		/// Gradient definition; corresponds to the Centered option on the Gradient Tab of the Boundary Hatch and Fill dialog box.
		/// Each gradient has two definitions, shifted and non-shifted. 
		/// A Shift value describes the blend of the two definitions that should be used. A value of 0.0 means only the non-shifted version should be used, and a value of 1.0 means that only the shifted version should be used.
		/// </summary>
		[DxfCodeValue(461)]
		public double Shift { get; set; }

		/// <summary>
		/// Records how colors were defined and is used only by dialog code
		/// </summary>
		/// <remarks>
		/// 0 = Two-color gradient <br/>
		/// 1 = Single-color gradient
		/// </remarks>
		[DxfCodeValue(452)]
		public bool IsSingleColorGradient { get; set; }

		/// <summary>
		/// Color tint value used by dialog code (default = 0, 0; range is 0.0 to 1.0). The color tint value is a gradient color and controls the degree of tint in the dialog when the Hatch group code 452 is set to 1.
		/// </summary>
		[DxfCodeValue(462)]
		public double ColorTint { get; set; }

		/// <summary>
		/// Number of colors
		/// </summary>
		/// <remarks>
		/// 0 = Solid hatch <br/>
		/// 2 = Gradient
		/// </remarks>
		[DxfCodeValue(453)]
		public List<Color> Colors { get; set; } = new List<Color>();

		public HatchGradientPattern(string name) : base(name) { }
	}
}
