using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	//TODO: FIX the hatch mess!!!!
	public class HatchPattern
	{
		public readonly static HatchPattern Solid = new HatchPattern("SOLID");

		public class Line
		{
			public double Angle { get; internal set; }
			public XY BasePoint { get; internal set; }
			public XY Offset { get; internal set; }
			public List<double> DashLengths { get; set; } = new List<double>();
		}

		public string Name { get; set; }
		public double Angle { get; set; }
		public double Scale { get; set; }

		public List<Line> Lines { get; set; } = new List<Line>();

		public HatchPattern(string name)
		{
			Name = name;
		}

		public HatchPattern Clone()
		{
			throw new System.NotImplementedException();
		}
	}
}
