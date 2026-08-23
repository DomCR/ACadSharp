using ACadSharp.Entities;

namespace ACadSharp.Tests.Entities;

public class VertexFaceRecordTests : CommonVertexTests<VertexFaceRecord>
{
	public override VertexFlags[] ValidFlags => new VertexFlags[] { VertexFlags.PolyFaceMeshVertex };
}
