using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="RevolvedSurface"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityRevolvedSurface"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.RevolvedSurface"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityRevolvedSurface)]
	[DxfSubClass(DxfSubclassMarker.RevolvedSurface)]
	public class RevolvedSurface : Surface
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityRevolvedSurface;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.RevolvedSurface;
	}
}
