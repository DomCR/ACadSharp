using ACadSharp.Attributes;
using CSMath;
using System;
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
			/// Perpendicular distance for the next line to be located.
			/// </summary>
			public double LineOffset
			{
				get
				{
					double cos = Math.Cos(0.0 - this.Angle);
					double sin = Math.Sin(0.0 - this.Angle);

					return this.Offset.X * sin + this.Offset.Y * cos;
				}
			}

			/// <summary>
			/// Gets or sets the local displacements between lines of the same family.
			/// </summary>
			[DxfCodeValue(45, 46)]
			public XY Offset { get; set; }

			/// <summary>
			/// Gets the horizontal shift of the offset after applying the specified angle transformation.
			/// </summary>
			public double Shift
			{
				get
				{
					double cos = Math.Cos(0.0 - this.Angle);
					double sin = Math.Sin(0.0 - this.Angle);

					return this.Offset.X * cos - this.Offset.Y * sin;
				}
			}

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