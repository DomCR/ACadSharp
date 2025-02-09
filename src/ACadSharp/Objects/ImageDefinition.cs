using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="ImageDefinition"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectImageDefinition"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.RasterImageDef"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectImageDefinition)]
	[DxfSubClass(DxfSubclassMarker.RasterImageDef)]
	public class ImageDefinition : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectImageDefinition;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.RasterImageDef;

		/// <summary>
		/// Class version
		/// </summary>
		[DxfCodeValue(90)]
		public int ClassVersion { get; internal set; }

		/// <summary>
		/// File name of image
		/// </summary>
		[DxfCodeValue(1)]
		public string FileName { get; set; }

		/// <summary>
		/// Image size in pixels
		/// </summary>
		[DxfCodeValue(10, 20)]
		public XY Size { get; set; }

		/// <summary>
		/// Default size of one pixel in AutoCAD units
		/// </summary>
		[DxfCodeValue(11, 21)]
		public XY DefaultSize { get; set; } = new XY(1, 1);

		/// <summary>
		/// Image-is-loaded flag.
		/// </summary>
		[DxfCodeValue(280)]
		public bool IsLoaded { get; set; } = true;

		/// <summary>
		/// Resolution units.
		/// </summary>
		[DxfCodeValue(281)]
		public ResolutionUnit Units { get; set; }
	}
}
