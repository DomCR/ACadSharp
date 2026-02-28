using ACadSharp.Attributes;
using CSMath;
using CSUtilities.Extensions;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="Material"/> object
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectMaterial"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Material"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectMaterial)]
	[DxfSubClass(DxfSubclassMarker.Material)]
	public class Material : NonGraphicalObject
	{
		/// <summary>
		/// Ambient color value.
		/// </summary>
		[DxfCodeValue(90)]
		public Color AmbientColor { get; set; }

		/// <summary>
		/// Ambient color factor.
		/// </summary>
		/// <value>
		/// valid range is 0.0 to 1.0)
		/// </value>
		[DxfCodeValue(40)]
		public double AmbientColorFactor
		{
			get { return this._ambientColorFactor; }
			set
			{
				ObjectExtensions.InRange(value, 0, 1);
				this._ambientColorFactor = value;
			}
		}

		/// <summary>
		/// Ambient color method.
		/// </summary>
		[DxfCodeValue(70)]
		public ColorMethod AmbientColorMethod { get; set; } = ColorMethod.Current;

		/// <summary>
		/// Auto transform method of bump map mapper.
		/// </summary>
		[DxfCodeValue(272)]
		public AutoTransformMethodFlags BumpAutoTransform { get; set; } = AutoTransformMethodFlags.NoAutoTransform;

		/// <summary>
		/// Bump map blend factor.
		/// </summary>
		/// <value>
		/// default = 1.0
		/// </value>
		[DxfCodeValue(143)]
		public double BumpMapBlendFactor { get; set; } = 1.0;

		/// <summary>
		/// Bump map file name.
		/// </summary>
		/// <remarks>
		/// null file name specifies no map.
		/// </remarks>
		[DxfCodeValue(8)]
		public string BumpMapFileName { get; set; }

		/// <summary>
		/// Bump map source.
		/// </summary>
		[DxfCodeValue(179)]
		public MapSource BumpMapSource { get; set; } = MapSource.UseImageFile;

		/// <summary>
		/// Transform matrix of bump map mapper.
		/// </summary>
		[DxfCodeValue(144)]
		public Matrix4 BumpMatrix { get; set; } = Matrix4.Identity;

		/// <summary>
		/// Bump method of specular map mapper.
		/// </summary>
		[DxfCodeValue(270)]
		public ProjectionMethod BumpProjectionMethod { get; set; } = ProjectionMethod.Planar;

		/// <summary>
		/// Tiling method of bump map mapper.
		/// </summary>
		[DxfCodeValue(271)]
		public TilingMethod BumpTilingMethod { get; set; } = TilingMethod.Tile;

		/// <summary>
		/// Channel Flags.
		/// </summary>
		[DxfCodeValue(94)]
		public int ChannelFlags { get; set; }

		/// <summary>
		/// Material description.
		/// </summary>
		[DxfCodeValue(2)]
		public string Description { get; set; }

		/// <summary>
		/// Auto transform method of diffuse map mapper.
		/// </summary>
		[DxfCodeValue(75)]
		public AutoTransformMethodFlags DiffuseAutoTransform { get; set; } = AutoTransformMethodFlags.NoAutoTransform;

		/// <summary>
		/// Diffuse color value.
		/// </summary>
		[DxfCodeValue(91)]
		public Color DiffuseColor { get; set; }

		/// <summary>
		/// Diffuse color factor.
		/// </summary>
		/// <value>
		/// valid range is 0.0 to 1.0)
		/// </value>
		[DxfCodeValue(41)]
		public double DiffuseColorFactor
		{
			get { return this._diffuseColorFactor; }
			set
			{
				ObjectExtensions.InRange(value, 0, 1);
				this._diffuseColorFactor = value;
			}
		}

		/// <summary>
		/// Diffuse color method.
		/// </summary>
		[DxfCodeValue(71)]
		public ColorMethod DiffuseColorMethod { get; set; } = ColorMethod.Current;

		/// <summary>
		/// Diffuse map blend factor.
		/// </summary>
		[DxfCodeValue(42)]
		public double DiffuseMapBlendFactor { get; set; } = 1.0;

		/// <summary>
		/// Diffuse map file name.
		/// </summary>
		/// <remarks>
		/// null file name specifies no map.
		/// </remarks>
		[DxfCodeValue(3)]
		public string DiffuseMapFileName { get; set; }

		/// <summary>
		/// Diffuse map source.
		/// </summary>
		[DxfCodeValue(72)]
		public MapSource DiffuseMapSource { get; set; } = MapSource.UseImageFile;

		/// <summary>
		/// Transform matrix of diffuse map mapper.
		/// </summary>
		[DxfCodeValue(43)]
		public Matrix4 DiffuseMatrix { get; set; } = Matrix4.Identity;

		/// <summary>
		/// Projection method of diffuse map mapper.
		/// </summary>
		[DxfCodeValue(73)]
		public ProjectionMethod DiffuseProjectionMethod { get; set; } = ProjectionMethod.Planar;

		/// <summary>
		/// Tiling method of diffuse map mapper.
		/// </summary>
		[DxfCodeValue(74)]
		public TilingMethod DiffuseTilingMethod { get; set; } = TilingMethod.Tile;

		[DxfCodeValue(93)]
		public int IlluminationModel { get; set; } = 0;

		/// <summary>
		/// Material name.
		/// </summary>
		[DxfCodeValue(1)]
		public override string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
				base.Name = value;
			}
		}

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectMaterial;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <summary>
		/// Opacity percent.
		/// </summary>
		[DxfCodeValue(140)]
		public double Opacity { get; set; } = 1.0;

		/// <summary>
		/// Auto transform method of opacity map mapper.
		/// </summary>
		[DxfCodeValue(178)]
		public AutoTransformMethodFlags OpacityAutoTransform { get; set; } = AutoTransformMethodFlags.NoAutoTransform;

		/// <summary>
		/// Opacity map blend factor.
		/// </summary>
		/// <value>
		/// default = 1.0
		/// </value>
		[DxfCodeValue(141)]
		public double OpacityMapBlendFactor { get; set; } = 1.0;

		/// <summary>
		/// Opacity map file name.
		/// </summary>
		/// <remarks>
		/// null file name specifies no map.
		/// </remarks>
		[DxfCodeValue(7)]
		public string OpacityMapFileName { get; set; }

		/// <summary>
		/// Opacity map source.
		/// </summary>
		[DxfCodeValue(175)]
		public MapSource OpacityMapSource { get; set; } = MapSource.UseImageFile;

		/// <summary>
		/// Transform matrix of opacity map mapper.
		/// </summary>
		[DxfCodeValue(142)]
		public Matrix4 OpacityMatrix { get; set; } = Matrix4.Identity;

		/// <summary>
		/// Opacity method of specular map mapper.
		/// </summary>
		[DxfCodeValue(176)]
		public ProjectionMethod OpacityProjectionMethod { get; set; } = ProjectionMethod.Planar;

		/// <summary>
		/// Tiling method of opacity map mapper.
		/// </summary>
		[DxfCodeValue(177)]
		public TilingMethod OpacityTilingMethod { get; set; } = TilingMethod.Tile;

		/// <summary>
		/// Auto transform method of reflection map mapper.
		/// </summary>
		[DxfCodeValue(174)]
		public AutoTransformMethodFlags ReflectionAutoTransform { get; set; } = AutoTransformMethodFlags.NoAutoTransform;

		/// <summary>
		/// Blend factor of reflection map.
		/// </summary>
		[DxfCodeValue(48)]
		public double ReflectionMapBlendFactor { get; set; } = 1.0;

		/// <summary>
		/// Reflection map file name.
		/// </summary>
		/// <remarks>
		/// null file name specifies no map.
		/// </remarks>
		[DxfCodeValue(6)]
		public string ReflectionMapFileName { get; set; }

		/// <summary>
		/// Reflection map source.
		/// </summary>
		[DxfCodeValue(171)]
		public MapSource ReflectionMapSource { get; set; } = MapSource.UseImageFile;

		/// <summary>
		/// Transform matrix of reflection map mapper.
		/// </summary>
		[DxfCodeValue(49)]
		public Matrix4 ReflectionMatrix { get; set; } = Matrix4.Identity;

		/// <summary>
		/// Projection method of specular map mapper.
		/// </summary>
		[DxfCodeValue(172)]
		public ProjectionMethod ReflectionProjectionMethod { get; set; } = ProjectionMethod.Planar;

		/// <summary>
		/// Tiling method of reflection map mapper.
		/// </summary>
		[DxfCodeValue(173)]
		public TilingMethod ReflectionTilingMethod { get; set; } = TilingMethod.Tile;

		[DxfCodeValue(468)]
		public double Reflectivity { get; set; } = 0.0;

		/// <summary>
		/// Auto transform method of refraction map mapper.
		/// </summary>
		[DxfCodeValue(276)]
		public AutoTransformMethodFlags RefractionAutoTransform { get; set; } = AutoTransformMethodFlags.NoAutoTransform;

		/// <summary>
		/// Refraction index.
		/// </summary>
		[DxfCodeValue(145)]
		public double RefractionIndex { get; set; } = 1.0;

		/// <summary>
		/// Bump map refraction factor.
		/// </summary>
		/// <value>
		/// default = 1.0
		/// </value>
		[DxfCodeValue(146)]
		public double RefractionMapBlendFactor { get; set; } = 1.0;

		/// <summary>
		/// Refraction map file name.
		/// </summary>
		/// <remarks>
		/// null file name specifies no map.
		/// </remarks>
		[DxfCodeValue(9)]
		public string RefractionMapFileName { get; set; }

		/// <summary>
		/// Refraction map source.
		/// </summary>
		[DxfCodeValue(273)]
		public MapSource RefractionMapSource { get; set; } = MapSource.UseImageFile;

		/// <summary>
		/// Transform matrix of refraction map mapper.
		/// </summary>
		[DxfCodeValue(147)]
		public Matrix4 RefractionMatrix { get; set; } = Matrix4.Identity;

		/// <summary>
		/// Projection method of refraction map mapper.
		/// </summary>
		[DxfCodeValue(274)]
		public ProjectionMethod RefractionProjectionMethod { get; set; } = ProjectionMethod.Planar;

		/// <summary>
		/// Tiling method of refraction map mapper.
		/// </summary>
		[DxfCodeValue(275)]
		public TilingMethod RefractionTilingMethod { get; set; } = TilingMethod.Tile;

		/// <summary>
		/// Auto transform method of specular map mapper.
		/// </summary>
		[DxfCodeValue(170)]
		public AutoTransformMethodFlags SpecularAutoTransform { get; set; } = AutoTransformMethodFlags.NoAutoTransform;

		/// <summary>
		/// Specular color.
		/// </summary>
		[DxfCodeValue(92)]
		public Color SpecularColor { get; set; }

		/// <summary>
		/// Specular color factor.
		/// </summary>
		/// <value>
		/// valid range is 0.0 to 1.0)
		/// </value>
		[DxfCodeValue(45)]
		public double SpecularColorFactor
		{
			get { return this._specularColorFactor; }
			set
			{
				ObjectExtensions.InRange(value, 0, 1);
				this._specularColorFactor = value;
			}
		}

		/// <summary>
		/// Specular color method.
		/// </summary>
		[DxfCodeValue(76)]
		public ColorMethod SpecularColorMethod { get; set; } = ColorMethod.Current;

		/// <summary>
		/// Specular gloss factor.
		/// </summary>
		/// <value>
		/// default = 0.5
		/// </value>
		[DxfCodeValue(44)]
		public double SpecularGlossFactor { get; set; } = 0.5;

		/// <summary>
		/// Specular map blend factor.
		/// </summary>
		/// <value>
		/// default = 1.0
		/// </value>
		[DxfCodeValue(46)]
		public double SpecularMapBlendFactor { get; set; } = 1.0;

		/// <summary>
		/// Specular map file name.
		/// </summary>
		/// <remarks>
		/// null file name specifies no map.
		/// </remarks>
		[DxfCodeValue(4)]
		public string SpecularMapFileName { get; set; }

		/// <summary>
		/// Specular map source.
		/// </summary>
		[DxfCodeValue(77)]
		public MapSource SpecularMapSource { get; set; } = MapSource.UseImageFile;

		/// <summary>
		/// Transform matrix of specular map mapper.
		/// </summary>
		[DxfCodeValue(47)]
		public Matrix4 SpecularMatrix { get; set; } = Matrix4.Identity;

		/// <summary>
		/// Projection method of specular map mapper.
		/// </summary>
		[DxfCodeValue(78)]
		public ProjectionMethod SpecularProjectionMethod { get; set; } = ProjectionMethod.Planar;

		/// <summary>
		/// Tiling method of specular map mapper.
		/// </summary>
		[DxfCodeValue(79)]
		public TilingMethod SpecularTilingMethod { get; set; } = TilingMethod.Tile;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Material;

		[DxfCodeValue(148)]
		public double Translucence { get; set; } = 0.0;

		public const string ByBlockName = "ByBlock";

		public const string ByLayerName = "ByLayer";

		public const string GlobalName = "Global";

		private double _ambientColorFactor = 1.0;

		private double _diffuseColorFactor = 1.0;

		private double _specularColorFactor = 1.0;

		/// <summary>
		/// Initializes a new instance of the <see cref="Material"/> class with the specified name.
		/// </summary>
		/// <param name="name">The name of the material. This value cannot be null or empty.</param>
		public Material(string name) : base(name) { }

		internal Material()
		{ }

		//460
		//Color Bleed Scale

		//461	Indirect Dump Scale
		//462	Reflectance Scale
		//463

		//Transmittance Scale
		//290	Two-sided Material
		//464	Luminance
		//270	Luminance Mode
		//271

		//Normal Map Method
		//465	Normal Map Strength
		//42	Normal Map Blend Factor
		//72

		//Normal Map Source
		//3	Normal Map Source File Name
		//73	Normal Mapper Projection
		//74	Normal Mapper Tiling
		//75	Normal Mapper Auto Transform
		//43	Normal Mapper Transform
		//293	Materials Anonymous
		//272	Global Illumination Mode
		//273	Final Gather Mode
		//300	GenProcName
		//291	GenProcValBool
		//271	GenProcValInt
		//469	GenProcValReal
		//301	GenProcValText
		//292	GenProcTableEnd
		//62	GenProcValColorIndex
		//420	GenProcValColorRGB
		//430	GenProcValColorName
		//270	Map UTile
		//90	Self-Illuminaton
	}
}