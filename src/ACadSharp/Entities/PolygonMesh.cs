using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="PolygonMesh"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityPolyline"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.PolygonMesh"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityPolyline)]
	[DxfSubClass(DxfSubclassMarker.PolygonMesh)]
	public class PolygonMesh : Polyline<PolygonMeshVertex>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.POLYLINE_MESH;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.PolygonMesh;
	}
}