using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public class HatchPattern
	{
		public static HatchPattern Solid { get { return new HatchPattern("SOLID"); } }

		public class Line
		{
			/// <summary>
			/// Pattern line angle
			/// </summary>
			[DxfCodeValue(DxfReferenceType.IsAngle, 53)]
			public double Angle { get; internal set; }

			/// <summary>
			/// Pattern line base point
			/// </summary>
			[DxfCodeValue(43, 44)]
			public XY BasePoint { get; internal set; }

			/// <summary>
			/// Pattern line offset
			/// </summary>
			[DxfCodeValue(45, 46)]
			public XY Offset { get; internal set; }

			public List<double> DashLengths { get; set; } = new List<double>();

			public Line Clone()
			{
				Line clone = (Line)this.MemberwiseClone();
				clone.DashLengths = new List<double>(this.DashLengths);
				return clone;
			}
		}

		[DxfCodeValue(2)]
		public string Name { get; set; }

		[DxfCodeValue(DxfReferenceType.IsAngle, 53)]
		public double Angle { get; set; }

		public double Scale { get; set; }

		public List<Line> Lines { get; set; } = new List<Line>();

		public HatchPattern(string name)
		{
			this.Name = name;
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
