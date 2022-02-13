using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="PolyLine3D"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityPolyline"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Polyline"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityPolyline)]
	[DxfSubClass(DxfSubclassMarker.Polyline3d)]
	public class PolyLine3D : PolyLine
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.POLYLINE_3D;
	}
}
