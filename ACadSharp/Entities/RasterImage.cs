using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="RasterImage"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityImage"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.RasterImage"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityImage)]
	[DxfSubClass(DxfSubclassMarker.RasterImage)]
	public class RasterImage : CadImageBase
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNDEFINED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityImage;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.RasterImage;
	}
}
