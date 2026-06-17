using ACadSharp.Attributes;

namespace ACadSharp.Objects;

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
	/// <summary>
	/// Gets or sets the overall brightness level.
	/// </summary>
	[DxfCodeValue(44)]
	public double Brightness { get; set; }

	/// <summary>
	/// Gets or sets the visual style color.
	/// </summary>
	[DxfCodeValue(62, 63)]
	public Color Color { get; set; }

	/// <summary>
	/// Gets or sets the user-defined description.
	/// </summary>
	[DxfCodeValue(2)]
	public string Description { get; set; }

	/// <summary>
	/// Gets or sets bit flags that control display settings.
	/// </summary>
	[DxfCodeValue(93)]
	public int DisplaySettings { get; set; }

	/// <summary>
	/// Gets or sets edge style application flags.
	/// </summary>
	[DxfCodeValue(174)]
	public int EdgeApplyStyleFlag { get; set; }

	/// <summary>
	/// Gets or sets the edge color index/value.
	/// </summary>
	[DxfCodeValue(66)]
	public Color EdgeColor { get; set; }

	/// <summary>
	/// Gets or sets the edge crease angle.
	/// </summary>
	[DxfCodeValue(42)]
	public double EdgeCreaseAngle { get; set; }

	/// <summary>
	/// Gets or sets the color used for edge intersections.
	/// </summary>
	[DxfCodeValue(64)]
	public Color EdgeIntersectionColor { get; set; }

	/// <summary>
	/// Gets or sets the line type used for edge intersections.
	/// </summary>
	[DxfCodeValue(175)]
	public int EdgeIntersectionLineType { get; set; }

	/// <summary>
	/// Gets or sets the number of isolines displayed on edges.
	/// </summary>
	[DxfCodeValue(171)]
	public short EdgeIsolineCount { get; set; }

	/// <summary>
	/// Gets or sets the edge jitter amount.
	/// </summary>
	[DxfCodeValue(78)]
	public int EdgeJitter { get; set; }

	/// <summary>
	/// Gets or sets edge modifier flags.
	/// </summary>
	[DxfCodeValue(92)]
	public int EdgeModifiers { get; set; }

	/// <summary>
	/// Gets or sets the color used for obscured edges.
	/// </summary>
	[DxfCodeValue(65)]
	public Color EdgeObscuredColor { get; set; }

	/// <summary>
	/// Gets or sets the line type used for obscured edges.
	/// </summary>
	[DxfCodeValue(75)]
	public int EdgeObscuredLineType { get; set; }

	/// <summary>
	/// Gets or sets the edge overhang amount.
	/// </summary>
	[DxfCodeValue(77)]
	public int EdgeOverhang { get; set; }

	/// <summary>
	/// Gets or sets the color used for edge silhouettes.
	/// </summary>
	[DxfCodeValue(67)]
	public Color EdgeSilhouetteColor { get; set; }

	/// <summary>
	/// Gets or sets the silhouette edge width.
	/// </summary>
	[DxfCodeValue(79)]
	public int EdgeSilhouetteWidth { get; set; }

	/// <summary>
	/// Gets or sets edge style flags.
	/// </summary>
	[DxfCodeValue(91)]
	public int EdgeStyle { get; set; }

	/// <summary>
	/// Gets or sets the edge style model.
	/// </summary>
	[DxfCodeValue(74)]
	public EdgeStyleModel EdgeStyleModel { get; set; }

	/// <summary>
	/// Gets or sets the default edge width.
	/// </summary>
	[DxfCodeValue(76)]
	public int EdgeWidth { get; set; }

	/// <summary>
	/// Gets or sets the face color mode.
	/// </summary>
	[DxfCodeValue(73)]
	public FaceColorMode FaceColorMode { get; set; }

	/// <summary>
	/// Gets or sets the lighting model used to render faces.
	/// </summary>
	[DxfCodeValue(71)]
	public FaceLightingModelType FaceLightingModel { get; set; }

	/// <summary>
	/// Gets or sets the face lighting quality.
	/// </summary>
	[DxfCodeValue(72)]
	public FaceLightingQualityType FaceLightingQuality { get; set; }

	/// <summary>
	/// Gets or sets face modifier flags.
	/// </summary>
	[DxfCodeValue(90)]
	public FaceModifierType FaceModifiers { get; set; }

	/// <summary>
	/// Gets or sets the face opacity level.
	/// </summary>
	[DxfCodeValue(40)]
	public double FaceOpacityLevel { get; set; }

	/// <summary>
	/// Gets or sets the face specular highlight level.
	/// </summary>
	[DxfCodeValue(41)]
	public double FaceSpecularLevel { get; set; }

	/// <summary>
	/// Gets or sets the monochrome face color.
	/// </summary>
	[DxfCodeValue(421)]
	public Color FaceStyleMonoColor { get; set; }

	/// <summary>
	/// Gets or sets the halo gap value.
	/// </summary>
	[DxfCodeValue(170)]
	public short HaloGap { get; set; }

	/// <summary>
	/// Gets a value indicating whether this visual style is marked for internal use only.
	/// </summary>
	[DxfCodeValue(291)]
	public bool InternalFlag { get; internal set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectVisualStyle;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.UNLISTED;

	/// <summary>
	/// Gets or sets the overall opacity level.
	/// </summary>
	[DxfCodeValue(43)]
	public double OpacityLevel { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether edge hide precision mode is enabled.
	/// </summary>
	[DxfCodeValue(290)]
	public bool PrecisionFlag { get; set; }

	/// <summary>
	/// Gets or sets the raster file name used by this style.
	/// </summary>
	[DxfCodeValue(1)]
	public string RasterFile { get; set; }

	/// <summary>
	/// Gets or sets the shadow type.
	/// </summary>
	[DxfCodeValue(173)]
	public short ShadowType { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.VisualStyle;

	/// <summary>
	/// Gets or sets the visual style type.
	/// </summary>
	[DxfCodeValue(70)]
	public int Type { get; set; }

	/// <summary>
	/// Default <see cref="VisualStyle"/> name.
	/// </summary>
	public const string DefaultName = "2dWireframe";

	/// <summary>
	/// Initializes a new instance of the <see cref="VisualStyle"/> class.
	/// </summary>
	public VisualStyle() : base() { }

	/// <summary>
	/// Initializes a new instance of the <see cref="VisualStyle"/> class with the specified name.
	/// </summary>
	/// <param name="name">The name of the visual style.</param>
	public VisualStyle(string name) : base(name) { }
}