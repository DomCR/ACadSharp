using CSMath;
using System;
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

			public Line Clone()
			{
				Line clone = (Line)this.MemberwiseClone();
				clone.DashLengths = new List<double>(this.DashLengths);
				return clone;
			}
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
			HatchPattern clone = (HatchPattern)this.MemberwiseClone();

			clone.Lines.Clear();
			foreach (var item in this.Lines)
			{
				clone.Lines.Add(item.Clone());
			}

			return clone;
		}
	}
}
