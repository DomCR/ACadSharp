using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;
using System.Linq;

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
				public bool HasBulge => this.Bulges.Any();

				/// <summary>
				/// Is closed flag
				/// </summary>
				[DxfCodeValue(73)]
				public bool IsClosed { get; set; }

				/// <summary>
				/// Bulge
				/// </summary>
				/// <remarks>
				/// optional, default empty
				/// </remarks>
				[DxfCodeValue(42)]
				public List<double> Bulges { get; set; } = new List<double>();

				/// <remarks>
				/// Position values are only X and Y
				/// </remarks>
				[DxfCodeValue(93)]
				public List<XY> Vertices { get; set; } = new List<XY>();
			}
		}
	}
}
