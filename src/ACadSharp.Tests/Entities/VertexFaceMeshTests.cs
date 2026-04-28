using ACadSharp.Entities;

namespace ACadSharp.Tests.Entities;

public class VertexFaceMeshTests : CommonVertexTests<VertexFaceMesh>
{
	public override VertexFlags[] ValidFlags => new VertexFlags[] { VertexFlags.PolygonMesh3D, VertexFlags.PolyFaceMeshVertex };
}
