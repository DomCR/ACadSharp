using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public partial class HatchPattern
	{
		public static HatchPattern Solid { get { return new HatchPattern("SOLID"); } }

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

			clone.Lines = new List<Line>();
			foreach (var item in this.Lines)
			{
				clone.Lines.Add(item.Clone());
			}

			return clone;
		}
	}
}
