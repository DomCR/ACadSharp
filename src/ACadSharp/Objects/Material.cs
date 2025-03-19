using ACadSharp.Attributes;
using CSMath;
using CSUtilities.Extensions;

namespace ACadSharp.Objects
{
	public enum ColorMethod
	{
		Current = 0,
		Override = 1,
	}

	public enum MapSource
	{
		UseCurrentScene = 0,
		UseImageFile = 1,
	}

	public enum ProjectionMethod
	{
		None = 0,
		Planar = 1,
		Box = 2,
		Cylinder = 3,
		Sphere = 4
	}

	public enum TilingMethod
	{
		None = 0,
		Tile = 1,
		Crop = 2,
		Clamp = 3
	}

	[System.Flags]
	public enum AutoTransformMethodFlags
	{
		/// <summary>
		/// None.
		/// </summary>
		None = 0,
		/// <summary>
		/// No auto transform.
		/// </summary>
		NoAutoTransform = 1,
		/// <summary>
		/// Scale mapper to current entity extents; translate mapper to entity origin.
		/// </summary>
		ScaleMapper = 2,
		/// <summary>
		/// Include current block transform in mapper transform.
		/// </summary>
		IncludeCurrentBlock = 4
	}

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
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectMaterial;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Material;

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

		/// <summary>
		/// Material description.
		/// </summary>
		[DxfCodeValue(2)]
		public string Description { get; set; }

		/// <summary>
		/// Ambient color method.
		/// </summary>
		[DxfCodeValue(70)]
		public ColorMethod AmbientColorMethod { get; set; } = ColorMethod.Current;

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

		private double _ambientColorFactor = 1.0;

		/// <summary>
		/// Ambient color value.
		/// </summary>
		[DxfCodeValue(90)]
		public Color AmbientColor { get; set; }

		/// <summary>
		/// Ambient color method.
		/// </summary>
		[DxfCodeValue(71)]
		public ColorMethod DiffuseColorMethod { get; set; } = ColorMethod.Current;

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

		private double _diffuseColorFactor = 1.0;

		/// <summary>
		/// Diffuse color value.
		/// </summary>
		[DxfCodeValue(91)]
		public Color DiffuseColor { get; set; }

		/// <summary>
		/// Diffuse map blend factor.
		/// </summary>
		[DxfCodeValue(42)]
		public double DiffuseMapBlendFactor { get; set; } = 1.0;

		/// <summary>
		/// Diffuse map source.
		/// </summary>
		[DxfCodeValue(72)]
		public MapSource DiffuseMapSource { get; set; } = MapSource.UseImageFile;

		/// <summary>
		/// Diffuse map file name.
		/// </summary>
		/// <remarks>
		/// null file name specifies no map.
		/// </remarks>
		[DxfCodeValue(3)]
		public string DiffuseMapFileName { get; set; }

		/// <summary>
		/// Projection method of diffuse map mapper.
		/// </summary>
		[DxfCodeValue(73)]
		public ProjectionMethod DiffuseProjectionMethod { get; set; } = ProjectionMethod.Planar;

		/// <summary>
		/// Tiling method of diffuse map mapper.
		/// </summary>
		[DxfCodeValue(74)]
		public TilingMethod DiffuseMapper { get; set; } = TilingMethod.Tile;

		/// <summary>
		/// Auto transform method of diffuse map mapper.
		/// </summary>
		[DxfCodeValue(75)]
		public AutoTransformMethodFlags DiffuseAutoTransform { get; set; } = AutoTransformMethodFlags.NoAutoTransform;

		/// <summary>
		/// Transform matrix of diffuse map mapper.
		/// </summary>
		[DxfCodeValue(43)]
		public Matrix4 DiffuseMatrix { get; set; } = Matrix4.Identity;

		/// <summary>
		/// Specular gloss factor.
		/// </summary>
		/// <value>
		/// default = 0.5
		/// </value>
		[DxfCodeValue(44)]
		public double SpecularGlossFactor { get; set; } = 0.5;

		/// <summary>
		/// Specular color method.
		/// </summary>
		[DxfCodeValue(76)]
		public ColorMethod SpecularColorMethod { get; set; } = ColorMethod.Current;

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

		private double _specularColorFactor = 1.0;

		/// <summary>
		/// Specular color.
		/// </summary>
		[DxfCodeValue(92)]
		public Color SpecularColor { get; set; }

		/// <summary>
		/// Specular map blend factor.
		/// </summary>
		/// <value>
		/// default = 1.0
		/// </value>
		[DxfCodeValue(46)]
		public double SpecularMapBlendFactor { get; set; } = 1.0;

		/// <summary>
		/// Specular map source.
		/// </summary>
		[DxfCodeValue(77)]
		public MapSource SpecularMapSource { get; set; } = MapSource.UseImageFile;

		/// <summary>
		/// Specular map file name.
		/// </summary>
		/// <remarks>
		/// null file name specifies no map.
		/// </remarks>
		[DxfCodeValue(4)]
		public string SpecularMapFileName { get; set; }

		/// <summary>
		/// Projection method of specular map mapper.
		/// </summary>
		[DxfCodeValue(78)]
		public ProjectionMethod SpecularProjectionMethod { get; set; } = ProjectionMethod.Planar;

		/// <summary>
		/// Tiling method of specular map mapper.
		/// </summary>
		[DxfCodeValue(79)]
		public TilingMethod SpecularMapper { get; set; } = TilingMethod.Tile;

		/// <summary>
		/// Auto transform method of specular map mapper.
		/// </summary>
		[DxfCodeValue(170)]
		public AutoTransformMethodFlags SpecularAutoTransform { get; set; } = AutoTransformMethodFlags.NoAutoTransform;

		/// <summary>
		/// Transform matrix of specular map mapper.
		/// </summary>
		[DxfCodeValue(47)]
		public Matrix4 SpecularMatrix { get; set; } = Matrix4.Identity;

		/// <summary>
		/// Blend factor of reflection map.
		/// </summary>
		[DxfCodeValue(48)]
		public double ReflectionMapBlendFactor { get; set; } = 1.0;

		/// <summary>
		/// Reflection map source.
		/// </summary>
		[DxfCodeValue(171)]
		public MapSource ReflectionMapSource { get; set; } = MapSource.UseImageFile;

		/// <summary>
		/// Reflection map file name.
		/// </summary>
		/// <remarks>
		/// null file name specifies no map.
		/// </remarks>
		[DxfCodeValue(6)]
		public string ReflectionMapFileName { get; set; }

		/// <summary>
		/// Projection method of specular map mapper.
		/// </summary>
		[DxfCodeValue(172)]
		public ProjectionMethod ReflectionProjectionMethod { get; set; } = ProjectionMethod.Planar;

		/// <summary>
		/// Tiling method of reflection map mapper.
		/// </summary>
		[DxfCodeValue(173)]
		public TilingMethod ReflectionMapper { get; set; } = TilingMethod.Tile;

		/// <summary>
		/// Auto transform method of reflection map mapper.
		/// </summary>
		[DxfCodeValue(174)]
		public AutoTransformMethodFlags ReflectionAutoTransform { get; set; } = AutoTransformMethodFlags.NoAutoTransform;

		/// <summary>
		/// Transform matrix of reflection map mapper.
		/// </summary>
		[DxfCodeValue(49)]
		public Matrix4 ReflectionMatrix { get; set; } = Matrix4.Identity;

		/// <summary>
		/// Opacity percent.
		/// </summary>
		[DxfCodeValue(140)]
		public double Opacity { get; set; } = 1.0;

		/// <summary>
		/// Opacity map blend factor.
		/// </summary>
		/// <value>
		/// default = 1.0
		/// </value>
		[DxfCodeValue(141)]
		public double OpacityMapBlendFactor { get; set; } = 1.0;

		/// <summary>
		/// Opacity map source.
		/// </summary>
		[DxfCodeValue(175)]
		public MapSource OpacityMapSource { get; set; } = MapSource.UseImageFile;

		/// <summary>
		/// Opacity map file name.
		/// </summary>
		/// <remarks>
		/// null file name specifies no map.
		/// </remarks>
		[DxfCodeValue(7)]
		public string OpacityMapFileName { get; set; }

		/// <summary>
		/// Opacity method of specular map mapper.
		/// </summary>
		[DxfCodeValue(176)]
		public ProjectionMethod OpacityProjectionMethod { get; set; } = ProjectionMethod.Planar;

		/// <summary>
		/// Tiling method of opacity map mapper.
		/// </summary>
		[DxfCodeValue(177)]
		public TilingMethod OpacityMapper { get; set; } = TilingMethod.Tile;

		/// <summary>
		/// Auto transform method of opacity map mapper.
		/// </summary>
		[DxfCodeValue(178)]
		public AutoTransformMethodFlags OpacityAutoTransform { get; set; } = AutoTransformMethodFlags.NoAutoTransform;

		/// <summary>
		/// Transform matrix of opacity map mapper.
		/// </summary>
		[DxfCodeValue(142)]
		public Matrix4 OpacityMatrix { get; set; } = Matrix4.Identity;

		/// <summary>
		/// Bump map blend factor.
		/// </summary>
		/// <value>
		/// default = 1.0
		/// </value>
		[DxfCodeValue(143)]
		public double BumpMapBlendFactor { get; set; } = 1.0;

		/// <summary>
		/// Bump map source.
		/// </summary>
		[DxfCodeValue(179)]
		public MapSource BumpMapSource { get; set; } = MapSource.UseImageFile;

		/// <summary>
		/// Bump map file name.
		/// </summary>
		/// <remarks>
		/// null file name specifies no map.
		/// </remarks>
		[DxfCodeValue(8)]
		public string BumpMapFileName { get; set; }

		/// <summary>
		/// Bump method of specular map mapper.
		/// </summary>
		[DxfCodeValue(270)]
		public ProjectionMethod BumpProjectionMethod { get; set; } = ProjectionMethod.Planar;

		/// <summary>
		/// Tiling method of bump map mapper.
		/// </summary>
		[DxfCodeValue(271)]
		public TilingMethod BumpMapper { get; set; } = TilingMethod.Tile;

		/// <summary>
		/// Auto transform method of bump map mapper.
		/// </summary>
		[DxfCodeValue(272)]
		public AutoTransformMethodFlags BumpAutoTransform { get; set; } = AutoTransformMethodFlags.NoAutoTransform;

		/// <summary>
		/// Transform matrix of bump map mapper.
		/// </summary>
		[DxfCodeValue(144)]
		public Matrix4 BumpMatrix { get; set; } = Matrix4.Identity;

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
		/// Refraction map source.
		/// </summary>
		[DxfCodeValue(273)]
		public MapSource RefractionMapSource { get; set; } = MapSource.UseImageFile;

		/// <summary>
		/// Refraction map file name.
		/// </summary>
		/// <remarks>
		/// null file name specifies no map.
		/// </remarks>
		[DxfCodeValue(9)]
		public string RefractionMapFileName { get; set; }

		/// <summary>
		/// Projection method of refraction map mapper.
		/// </summary>
		[DxfCodeValue(274)]
		public ProjectionMethod RefractionProjectionMethod { get; set; } = ProjectionMethod.Planar;

		/// <summary>
		/// Tiling method of refraction map mapper.
		/// </summary>
		[DxfCodeValue(275)]
		public TilingMethod RefractionMapper { get; set; } = TilingMethod.Tile;

		/// <summary>
		/// Auto transform method of refraction map mapper.
		/// </summary>
		[DxfCodeValue(276)]
		public AutoTransformMethodFlags RefractionAutoTransform { get; set; } = AutoTransformMethodFlags.NoAutoTransform;

		/// <summary>
		/// Transform matrix of refraction map mapper.
		/// </summary>
		[DxfCodeValue(147)]
		public Matrix4 RefractionMatrix { get; set; } = Matrix4.Identity;

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
		//300

		//GenProcName
		//291	GenProcValBool
		//271	GenProcValInt
		//469	GenProcValReal
		//301	GenProcValText
		//292	GenProcTableEnd
		//62	GenProcValColorIndex
		//420	GenProcValColorRGB
		//430	GenProcValColorName
		//270	Map UTile
		//148	Translucence
		//90	Self-Illuminaton
		//468	Reflectivity
		//93	Illumination Model

		/// <summary>
		/// Channel Flags.
		/// </summary>
		[DxfCodeValue(94)]
		public int ChannelFlags { get; set; }
	}
}
