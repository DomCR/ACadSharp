using ACadSharp.Attributes;
using ACadSharp.Geometry;
using ACadSharp.IO.Templates;

namespace ACadSharp.Entities
{
	public class Vertex : Entity
	{
		public override ObjectType ObjectType => ObjectType.VERTEX_2D;  //Shit there is a 3d too...
		public override string ObjectName => DxfFileToken.EntityVertex;

		//100	Subclass marker(AcDbVertex)
		//100	Subclass marker(AcDb2dVertex or AcDb3dPolylineVertex)

		[DxfCodeValue(DxfCode.XCoordinate, DxfCode.YCoordinate, DxfCode.ZCoordinate)]
		public XYZ Location { get; set; } = XYZ.Zero;
		[DxfCodeValue(DxfCode.StartWith)]
		public double StartWith { get; set; } = 0.0;
		[DxfCodeValue(DxfCode.EndWith)]
		public double EndWith { get; set; } = 0.0;
		[DxfCodeValue(DxfCode.Bulge)]
		public double Bulge { get; set; } = 0.0;
		[DxfCodeValue(DxfCode.Int16)]
		public VertexFlags Flags { get; set; }
		[DxfCodeValue(DxfCode.Angle)]
		public double CurveTangent { get; set; }

		//71 Polyface mesh vertex index(optional; present only if nonzero)
		//72 Polyface mesh vertex index(optional; present only if nonzero)
		//73 Polyface mesh vertex index(optional; present only if nonzero)
		//74 Polyface mesh vertex index(optional; present only if nonzero)
		//91 Vertex identifier

		public Vertex() : base() { }

		internal Vertex(DxfEntityTemplate template) : base(template) { }
	}
}
