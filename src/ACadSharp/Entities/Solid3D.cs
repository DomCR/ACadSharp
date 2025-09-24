using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Solid3D"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.Entity3DSolid"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Solid3D"/>
	/// </remarks>
	[DxfName(DxfFileToken.Entity3DSolid)]
	[DxfSubClass(DxfSubclassMarker.Solid3D)]
	public class Solid3D : ModelerGeometry
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.SOLID3D;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.Entity3DSolid;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Solid3D;

		//70	Modeler format version number(currently = 1)
		//1	Proprietary data(multiple lines < 255 characters each)
		//3	Additional lines of proprietary data(if previous group 1 string is greater than 255 characters) (optional)
	}
}
