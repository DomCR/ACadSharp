using ACadSharp.Attributes;
using System;

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

		//350	Soft-owner ID/handle to history object

	}
}
