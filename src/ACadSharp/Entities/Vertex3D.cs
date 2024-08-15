using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Vertex3D"/> entity
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityVertex"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Polyline3dVertex"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityVertex)]
	[DxfSubClass(DxfSubclassMarker.Polyline3dVertex)]
	public class Vertex3D : Vertex
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.VERTEX_3D;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Polyline3dVertex;
	}
}
