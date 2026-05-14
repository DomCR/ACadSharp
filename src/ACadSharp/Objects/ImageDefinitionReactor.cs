using ACadSharp.Attributes;
using ACadSharp.Entities;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="ImageDefinitionReactor"/> object. <br/>
	/// This object is for internal use only.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectImageDefinitionReactor"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.RasterImageDefReactor"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectImageDefinitionReactor)]
	[DxfSubClass(DxfSubclassMarker.RasterImageDefReactor)]
	public class ImageDefinitionReactor : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectImageDefinitionReactor;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.RasterImageDefReactor;

		/// <summary>
		/// Class version 2.
		/// </summary>
		[DxfCodeValue(90)]
		public int ClassVersion { get; set; } = 2;

		/// <summary>
		/// Object ID for associated image object.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 330)]
		public RasterImage Image { get; set; }

		internal ImageDefinitionReactor() { }

		internal ImageDefinitionReactor(RasterImage image)
		{
			this.Owner = image;
			this.Image = image;
		}
	}
}
