using System;

using ACadSharp.Attributes;
using ACadSharp.IO.DXF;
using CSMath;

namespace ACadSharp.Entities
{
	public partial class Hatch
	{
		public partial class BoundaryPath
		{
			public class Ellipse : Edge
			{
				/// <summary>
				/// Center point (in OCS).
				/// </summary>
				[DxfCodeValue(10, 20)]
				public XY Center { get; set; }

				/// <summary>
				/// Counterclockwise flag.
				/// </summary>
				[DxfCodeValue(73)]
				public bool CounterClockWise { get; set; }

				/// <summary>
				/// End angle.
				/// </summary>
				[DxfCodeValue(51)]
				public double EndAngle { get; set; }

				/// <summary>
				/// Endpoint of major axis relative to center point (in OCS).
				/// </summary>
				[DxfCodeValue(11, 21)]
				public XY MajorAxisEndPoint { get; set; }

				/// <summary>
				/// Length of minor axis (percentage of major axis length).
				/// </summary>
				[DxfCodeValue(40)]
				public double MinorToMajorRatio { get; set; }

				/// <summary>
				/// Start angle.
				/// </summary>
				[DxfCodeValue(50)]
				public double StartAngle { get; set; }

				/// <inheritdoc/>
				public override EdgeType Type => EdgeType.EllipticArc;

				/// <inheritdoc/>
				public override void ApplyTransform(Transform transform)
				{
					throw new System.NotImplementedException();
				}

				/// <inheritdoc/>
				public override BoundingBox GetBoundingBox()
				{
					return this.ToEntity().GetBoundingBox();
				}

				/// <inheritdoc/>
				public override Entity ToEntity()
				{
					XYZ center = new XYZ(this.Center.X, this.Center.Y, 0.0);
					XYZ axisPoint = new XYZ(this.MajorAxisEndPoint.X, this.MajorAxisEndPoint.Y, 0.0);

					double rotation = axisPoint.Convert<XY>().GetAngle();
					double majorAxis = 2 * axisPoint.GetLength();

					Entities.Ellipse ellipse = new();
					ellipse.Center = center;
					ellipse.StartParameter = this.CounterClockWise ? this.StartAngle : 2 * Math.PI - this.EndAngle;
					ellipse.EndParameter = this.CounterClockWise ? this.EndAngle : 2 * Math.PI - this.StartAngle;

					return ellipse;
				}
			}
		}
	}
}