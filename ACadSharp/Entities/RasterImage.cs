using ACadSharp.Attributes;
using ACadSharp.Objects;

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
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityImage;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.RasterImage;

		internal RasterImage() : base() { }

		public RasterImage(ImageDefinition definition)
		{
			this.Definition = definition;
		}

		/// <summary>
		/// Set the image definition
		/// </summary>
		/// <param name="definition"></param>
		public void SetImageDefinition(ImageDefinition definition)
		{
			this.Definition = definition;
		}
	}
}
