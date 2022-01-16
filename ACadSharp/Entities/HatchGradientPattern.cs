using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public class HatchGradientPattern : HatchPattern
	{
		public int Reserved { get; set; }
		public double Shift { get; set; }
		public bool IsSingleColorGradient { get; set; }
		public double ColorTint { get; set; }
		public List<Color> Colors { get; set; } = new List<Color>();
		public HatchGradientPattern(string name) : base(name) { }
	}
}
