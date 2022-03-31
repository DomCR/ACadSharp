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
		public override string ObjectName => DxfFileToken.EntityVertex;

		//100	Subclass marker(AcDbVertex)
		//100	Subclass marker(AcDb2dVertex or AcDb3dPolylineVertex)

		[DxfCodeValue(10, 20, 30)]
		public XYZ Location { get; set; } = XYZ.Zero;

		[DxfCodeValue(40)]
		public double StartWidth { get; set; } = 0.0;

		[DxfCodeValue(41)]
		public double EndWidth { get; set; } = 0.0;

		[DxfCodeValue(42)]
		public double Bulge { get; set; } = 0.0;

		[DxfCodeValue(70)]
		public VertexFlags Flags { get; set; }

		[DxfCodeValue(50)]
		public double CurveTangent { get; set; }

		//71 Polyface mesh vertex index(optional; present only if nonzero)
		//72 Polyface mesh vertex index(optional; present only if nonzero)
		//73 Polyface mesh vertex index(optional; present only if nonzero)
		//74 Polyface mesh vertex index(optional; present only if nonzero)

		[DxfCodeValue(91)]
		public int Id { get; set; }

		public Vertex() : base() { }
	}
}
