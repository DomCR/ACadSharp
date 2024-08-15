using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	public partial class LwPolyline
	{
		public class Vertex : IVertex
		{
			/// <summary>
			/// Vertex coordinates (in OCS)
			/// </summary>
			[DxfCodeValue(10, 20)]
			public XY Location { get; set; } = XY.Zero;

			/// <summary>
			/// Starting width
			/// </summary>
			[DxfCodeValue(DxfReferenceType.Optional, 40)]
			public double StartWidth { get; set; } = 0.0;

			/// <summary>
			/// Ending width
			/// </summary>
			[DxfCodeValue(DxfReferenceType.Optional, 41)]
			public double EndWidth { get; set; } = 0.0;

			/// <summary>
			/// The bulge is the tangent of one fourth the included angle for an arc segment, made negative if the arc goes clockwise from the start point to the endpoint.A bulge of 0 indicates a straight segment, and a bulge of 1 is a semicircle
			/// </summary>
			[DxfCodeValue(DxfReferenceType.Optional, 42)]
			public double Bulge { get; set; } = 0.0;

			/// <summary>
			/// Vertex flags
			/// </summary>
			[DxfCodeValue(70)]
			public VertexFlags Flags { get; set; } = VertexFlags.Default;

			/// <summary>
			/// Curve fit tangent direction
			/// </summary>
			[DxfCodeValue(50)]
			public double CurveTangent { get; set; } = 0;

			/// <summary>
			/// Vertex identifier
			/// </summary>
			[DxfCodeValue(91)]
			public int Id { get; set; } = 0;

			IVector IVertex.Location { get { return this.Location; } }

			public Vertex() { }

			public Vertex(XY location)
			{
				Location = location;
			}
		}
	}
}
