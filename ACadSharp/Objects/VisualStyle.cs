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
	public class VisualStyle : CadObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectVisualStyle;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.VisualStyle;

		/// <summary>
		/// Raster file name
		/// </summary>
		[DxfCodeValue(1)]
		public string RasterFile { get; set; }

		/// <summary>
		/// Description
		/// </summary>
		[DxfCodeValue(2)]
		public string Description { get; set; }

		/// <summary>
		/// Type
		/// </summary>
		[DxfCodeValue(70)]
		public int Type { get; set; }

		//71	Face lighting model
		//0 =Invisible
		//1 = Visible
		//2 = Phong
		//3 = Gooch
		[DxfCodeValue(71)]
		public FaceLightingModelType FaceLightingModel { get; set; }

		//72	Face lighting quality
		//0 = No lighting
		//1 = Per face lighting
		//2 = Per vertex lighting
		[DxfCodeValue(72)]
		public FaceLightingQualityType FaceLightingQuality { get; set; }

		//73	Face color mode
		//0 = No color
		//1 = Object color
		//2 = Background color
		//3 = Custom color
		//4 = Mono color
		//5 = Tinted
		//6 = Desaturated
		[DxfCodeValue(73)]
		public FaceColorMode FaceColorMode { get; set; }

		//90
		//Face modifiers
		//0 = No modifiers
		//1 = Opacity
		//2 = Specular
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
		/// Color
		/// </summary>
		[DxfCodeValue(62, 63)]
		public Color Color { get; set; }

		/// <summary>
		/// Face style mono color
		/// </summary>
		[DxfCodeValue(421)]
		public Color FaceStyleMonoColor { get; set; }

		//74
		//Edge style model
		//0 = No edges
		//1 = Isolines
		//2 = Facet edges

		//91

		//Edge style

		//64

		//Edge intersection color

		//65

		//Edge obscured color

		//75

		//Edge obscured linetype

		//175

		//Edge intersection linetype

		//42

		//Edge crease angle

		//92

		//Edge modifiers

		//66

		//Edge color

		//43

		//Edge opacity level

		//76

		//Edge width

		//77

		//Edge overhang

		//78

		//Edge jitter

		//67

		//Edge silhouette color

		//79

		//Edge silhouette width

		//170

		//Edge halo gap

		//171

		//Number of edge isolines

		//174

		//Edge style apply flag

		//93

		//Display style display settings

		//44	Brightness

		//173	Shadow type

		/// <summary>
		/// Edge hide precision flag
		/// </summary>
		[DxfCodeValue(290)]
		public bool PrecisionFlag { get; set; }

		/// <summary>
		/// Internal use only flag
		/// </summary>
		[DxfCodeValue(291)]
		public bool InternalFlag { get; internal set; }
	}
}
