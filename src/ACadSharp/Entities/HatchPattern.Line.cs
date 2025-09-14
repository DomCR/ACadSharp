using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public partial class HatchPattern
	{
		public class Line
		{
			/// <summary>
			/// Pattern line angle.
			/// </summary>
			[DxfCodeValue(DxfReferenceType.IsAngle, 53)]
			public double Angle { get; set; }

			/// <summary>
			/// Pattern line base point.
			/// </summary>
			[DxfCodeValue(43, 44)]
			public XY BasePoint { get; set; }

			/// <summary>
			/// Line dashes.
			/// </summary>
			[DxfCodeValue(DxfReferenceType.Count, 79)]
			[DxfCollectionCodeValue(49)]
			public List<double> DashLengths { get; set; } = new List<double>();

			/// <summary>
			/// Pattern line offset.
			/// </summary>
			[DxfCodeValue(45, 46)]
			public XY Offset { get; set; }

			/// <summary>
			/// Clones this line.
			/// </summary>
			/// <returns></returns>
			public Line Clone()
			{
				Line clone = (Line)this.MemberwiseClone();
				clone.DashLengths = new List<double>(this.DashLengths);
				return clone;
			}
		}
	}
}