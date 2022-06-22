using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a base for <see cref="Vertex2D"/> and <see cref="Vertex3D"/>
	/// </summary>
	[DxfSubClass(DxfSubclassMarker.Vertex, true)]
	public abstract class Vertex : Entity
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityVertex;

		/// <summary>
		/// Location point (in OCS when 2D, and WCS when 3D)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ Location { get; set; } = XYZ.Zero;

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
		public VertexFlags Flags { get; set; }

		/// <summary>
		/// Curve fit tangent direction
		/// </summary>
		[DxfCodeValue(50)]
		public double CurveTangent { get; set; }

		//71 Polyface mesh vertex index(optional; present only if nonzero)
		//72 Polyface mesh vertex index(optional; present only if nonzero)
		//73 Polyface mesh vertex index(optional; present only if nonzero)
		//74 Polyface mesh vertex index(optional; present only if nonzero)

		/// <summary>
		/// Vertex identifier
		/// </summary>
		[DxfCodeValue(91)]
		public int Id { get; set; }

		public Vertex() : base() { }
	}
}
