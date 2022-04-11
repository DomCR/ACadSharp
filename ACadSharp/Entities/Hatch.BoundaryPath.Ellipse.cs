using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	public partial class Hatch
	{
		public partial class BoundaryPath
		{
			public class Ellipse : Arc
			{
				/// <inheritdoc/>
				public override EdgeType Type => EdgeType.EllipticArc;

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
			}
		}
	}
}
