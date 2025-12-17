using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="VisualStyle"/> object
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectVisualStyle"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.VisualStyle"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectVisualStyle)]
	[DxfSubClass(DxfSubclassMarker.VisualStyle)]
	public class VisualStyle : NonGraphicalObject
	{
		[DxfCodeValue(44)]
		public double Brightness { get; set; }

		/// <summary>
		/// Color
		/// </summary>
		[DxfCodeValue(62, 63)]
		public Color Color { get; set; }

		/// <summary>
		/// Description
		/// </summary>
		[DxfCodeValue(2)]
		public string Description { get; set; }

		[DxfCodeValue(93)]
		public int DisplaySettings { get; set; }

		[DxfCodeValue(174)]
		public short EdgeApplyStyleFlag { get; set; }

		[DxfCodeValue(66)]
		public int EdgeColor { get; set; }

		[DxfCodeValue(42)]
		public double EdgeCreaseAngle { get; set; }

		[DxfCodeValue(64)]
		public Color EdgeIntersectionColor { get; set; }

		[DxfCodeValue(175)]
		public int EdgeIntersectionLineType { get; set; }

		[DxfCodeValue(171)]
		public short EdgeIsolineCount { get; set; }

		[DxfCodeValue(78)]
		public int EdgeJitter { get; set; }

		[DxfCodeValue(92)]
		public int EdgeModifiers { get; set; }

		[DxfCodeValue(65)]
		public Color EdgeObscuredColor { get; set; }

		[DxfCodeValue(75)]
		public int EdgeObscuredLineType { get; set; }

		[DxfCodeValue(77)]
		public int EdgeOverhang { get; set; }

		[DxfCodeValue(67)]
		public Color EdgeSilhouetteColor { get; set; }

		[DxfCodeValue(79)]
		public int EdgeSilhouetteWidth { get; set; }

		[DxfCodeValue(91)]
		public int EdgeStyle { get; set; }

		[DxfCodeValue(74)]
		public EdgeStyleModel EdgeStyleModel { get; set; }

		[DxfCodeValue(76)]
		public int EdgeWidth { get; set; }

		[DxfCodeValue(73)]
		public FaceColorMode FaceColorMode { get; set; }

		/// <summary>
		/// Gets or sets the lighting model used to render the face.
		/// </summary>
		/// <remarks>The lighting model determines how light interacts with the face during rendering, affecting its
		/// appearance and shading. Set this property to control the visual style of the face in 3D scenes.</remarks>
		[DxfCodeValue(71)]
		public FaceLightingModelType FaceLightingModel { get; set; }

		[DxfCodeValue(72)]
		public FaceLightingQualityType FaceLightingQuality { get; set; }

		[DxfCodeValue(90)]
		public FaceModifierType FaceModifiers { get; set; }

		/// <summary>
		/// Face opacity level
		/// </summary>
		[DxfCodeValue(40)]
		public double FaceOpacityLevel { get; set; }

		/// <summary>
		/// Face specular level
		/// </summary>
		[DxfCodeValue(41)]
		public double FaceSpecularLevel { get; set; }

		/// <summary>
		/// Face style mono color
		/// </summary>
		[DxfCodeValue(421)]
		public Color FaceStyleMonoColor { get; set; }

		[DxfCodeValue(170)]
		public short HaloGap { get; set; }

		/// <summary>
		/// Internal use only flag
		/// </summary>
		[DxfCodeValue(291)]
		public bool InternalFlag { get; internal set; }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectVisualStyle;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		[DxfCodeValue(43)]
		public double OpacityLevel { get; set; }

		/// <summary>
		/// Edge hide precision flag
		/// </summary>
		[DxfCodeValue(290)]
		public bool PrecisionFlag { get; set; }

		/// <summary>
		/// Raster file name
		/// </summary>
		[DxfCodeValue(1)]
		public string RasterFile { get; set; }

		[DxfCodeValue(173)]
		public short ShadowType { get; set; }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.VisualStyle;

		/// <summary>
		/// Type
		/// </summary>
		[DxfCodeValue(70)]
		public int Type { get; set; }

		/// <summary>
		/// Default <see cref="VisualStyle"/> name.
		/// </summary>
		public const string DefaultName = "2dWireframe";
	}
}