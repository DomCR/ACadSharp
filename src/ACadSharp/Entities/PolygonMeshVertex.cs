using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="PolygonMeshVertex"/> entity
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityVertex"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.PolygonMeshVertex"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityVertex)]
	[DxfSubClass(DxfSubclassMarker.PolygonMeshVertex)]
	public class PolygonMeshVertex : Vertex
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.VERTEX_MESH;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.PolygonMeshVertex;

		/// <inheritdoc/>
		public PolygonMeshVertex() : base()
		{
		}

		/// <inheritdoc/>
		public PolygonMeshVertex(XYZ location) : base(location)
		{
		}
	}
}