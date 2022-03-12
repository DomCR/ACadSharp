using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public partial class Hatch
	{
		public partial class BoundaryPath
		{
			public class Polyline : Edge
			{
				/// <inheritdoc/>
				public override EdgeType Type => EdgeType.Polyline;

				/// <summary>
				/// Has bulge flag
				/// </summary>
				[DxfCodeValue(72)]
				public bool HasBulge { get { return Bulge != 0; } }

				/// <summary>
				/// Is closed flag
				/// </summary>
				[DxfCodeValue(73)]
				public bool IsClosed { get; set; }

				/// <summary>
				/// Bulge
				/// </summary>
				/// <remarks>
				/// optional, default = 0
				/// </remarks>
				[DxfCodeValue(42)]
				public double Bulge { get; set; } = 0.0;

				/// <remarks>
				/// Position values are only X and Y
				/// </remarks>
				[DxfCodeValue(93)]
				public List<XY> Vertices { get; set; } = new List<XY>();
			}
		}
	}
}
