using ACadSharp.Attributes;
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
				/// Center point (in OCS)
				/// </summary>
				[DxfCodeValue(10, 20)]
				public XY Center { get; set; }

				/// <summary>
				/// Is counterclockwise flag
				/// </summary>
				[DxfCodeValue(73)]
				public bool CounterClockWise { get; set; }

				/// <summary>
				/// End angle
				/// </summary>
				[DxfCodeValue(51)]
				public double EndAngle { get; set; }

				/// <summary>
				/// Endpoint of major axis relative to center point (in OCS)
				/// </summary>
				[DxfCodeValue(11, 21)]
				public XY MajorAxisEndPoint { get; set; }

				/// <summary>
				/// Length of minor axis (percentage of major axis length)
				/// </summary>
				[DxfCodeValue(40)]
				public double MinorToMajorRatio { get; set; }

				/// <summary>
				/// Start angle
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
					throw new System.NotImplementedException();
				}
			}
		}
	}
}