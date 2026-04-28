using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities;

/// <summary>
/// Represents a <see cref="VertexFaceMesh"/> entity
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.EntityVertex"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.PolyfaceMeshVertex"/>
/// </remarks>
[DxfName(DxfFileToken.EntityVertex)]
[DxfSubClass(DxfSubclassMarker.PolyfaceMeshVertex)]
public class VertexFaceMesh : Vertex
{
	/// <inheritdoc/>
	public override VertexFlags Flags { get => base.Flags | VertexFlags.PolygonMesh3D | VertexFlags.PolyFaceMeshVertex; set => base.Flags = value; }

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.VERTEX_PFACE;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.PolyfaceMeshVertex;

	/// <inheritdoc/>
	public VertexFaceMesh() : base()
	{
	}

	/// <inheritdoc/>
	public VertexFaceMesh(IVector location)
		: base(location)
	{
	}
}