using ACadSharp.Attributes;
using ACadSharp.Types.Units;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="RasterVariables"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectRasterVariables"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.RasterVariables"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectRasterVariables)]
	[DxfSubClass(DxfSubclassMarker.RasterVariables)]
	public class RasterVariables : NonGraphicalObject
	{
		/// <summary>
		/// Class version 0.
		/// </summary>
		[DxfCodeValue(90)]
		public int ClassVersion { get; internal set; }

		/// <summary>
		/// Gets or sets the image display quality (screen only).
		/// </summary>
		[DxfCodeValue(71)]
		public ImageDisplayQuality DisplayQuality { get; set; } = ImageDisplayQuality.High;

		/// <summary>
		/// Gets or sets if the image frame is shown.
		/// </summary>
		[DxfCodeValue(70)]
		public bool IsDisplayFrameShown { get; set; }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectRasterVariables;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.RasterVariables;

		/// <summary>
		/// AutoCAD units for inserting images. <br/>
		/// This is what one AutoCAD unit is equal to for the purpose of inserting and scaling images with an associated resolution.
		/// </summary>
		[DxfCodeValue(92)]
		public ImageUnits Units { get; set; }

		/// <inheritdoc/>
		public RasterVariables() : base()
		{
		}
	}
}