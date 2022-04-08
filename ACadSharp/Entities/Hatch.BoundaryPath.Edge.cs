namespace ACadSharp.Entities
{
	public partial class Hatch
	{
		public partial class BoundaryPath
		{
			public enum EdgeType
			{
				/// <remarks>
				/// Not included in the documentation
				/// </remarks>
				Polyline = 0,
				Line = 1,
				CircularArc = 2,
				EllipticArc = 3,
				Spline = 4,
			}

			public abstract class Edge
			{
				/// <summary>
				/// Edge type
				/// </summary>
				public abstract EdgeType Type { get; }
			}
		}
	}
}
