using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	public partial class Hatch
	{
		public partial class BoundaryPath
		{
			public class Arc : Edge
			{
				/// <inheritdoc/>
				public override EdgeType Type => EdgeType.CircularArc;

				/// <summary>
				/// Center point (in OCS)
				/// </summary>
				[DxfCodeValue(10, 20)]
				public XY Center { get; set; }

				/// <summary>
				/// Radius
				/// </summary>
				[DxfCodeValue(40)]
				public double Radius { get; set; }

				/// <summary>
				/// Start angle
				/// </summary>
				[DxfCodeValue(50)]
				public double StartAngle { get; set; }

				/// <summary>
				/// End angle
				/// </summary>
				[DxfCodeValue(51)]
				public double EndAngle { get; set; }

				/// <summary>
				/// Is counterclockwise flag
				/// </summary>
				[DxfCodeValue(73)]
				public bool CounterClockWise { get; set; }
			}
		}
	}
}
