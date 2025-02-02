using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects
{

	public partial class GeoData
	{
		public class GeoMeshPoint
		{
			/// <summary>
			/// Coordinate of source mesh point.
			/// </summary>
			[DxfCodeValue(13, 23)]
			public XY Source { get; set; }

			/// <summary>
			/// Coordinate of destination mesh point
			/// </summary>
			[DxfCodeValue(14, 24)]
			public XY Destination { get; set; }

			/// <inheritdoc/>
			public override string ToString()
			{
				return $"src:{this.Source.ToString(System.Globalization.CultureInfo.InvariantCulture)} dest:{this.Destination.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
			}
		}
	}
}
