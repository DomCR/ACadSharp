using ACadSharp.Entities;

namespace ACadSharp.Tests.Entities;

public class Vertex3DTests : CommonVertexTests<Vertex3D>
{
	public override VertexFlags[] ValidFlags => new VertexFlags[] { VertexFlags.PolylineVertex3D };
}
