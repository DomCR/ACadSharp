using ACadSharp.Attributes;
using CSUtilities.Extensions;

namespace ACadSharp.Objects
{
	public enum AmbientColorMethod
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
		public AmbientColorMethod AmbientColorMethod { get; set; } = AmbientColorMethod.Current;

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
				ObjectExtensions.InRange(value, 0, 1, $"{nameof(AmbientColorFactor)} valid values are from 0.0 to 1.0");
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
		public AmbientColorMethod DiffuseColorMethod { get; set; } = AmbientColorMethod.Current;

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
				ObjectExtensions.InRange(value, 0, 1, $"{nameof(DiffuseColorFactor)} valid values are from 0.0 to 1.0");
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
		public string DiffuseMapRileName { get; set; }

		/// <summary>
		/// Projection method of diffuse map mapper.
		/// </summary>
		[DxfCodeValue(73)]
		public ProjectionMethod ProjectionMethod { get; set; } = ProjectionMethod.Planar;

		/// <summary>
		/// Tiling method of diffuse map mapper.
		/// </summary>
		[DxfCodeValue(74)]
		public TilingMethod DiffuseMapper { get; set; } = TilingMethod.Tile;

		/// <summary>
		/// Auto transform method of diffuse map mapper.
		/// </summary>
		[DxfCodeValue(75)]
		public AutoTransformMethodFlags AutoTransformDiffuse { get; set; } = AutoTransformMethodFlags.NoAutoTransform;

		//43

		//Transform matrix of diffuse map mapper(16 reals; row major format; default = identity matrix)

		//44

		//Specular gloss factor(real, default = 0.5)

		//76

		//Specular color method(default = 0) :

		//0 = Use current color

		//1 = Override current color

		//45

		//Specular color factor(real, default = 1.0; valid range is 0.0 to 1.0)

		//92

		//Specular color value(unsigned 32-bit integer representing an AcCmEntityColor)

		//46

		//Specular map blend factor(real; default = 1.0)

		//77

		//Specular map source(default = 1) :

		//0 = Use current scene

		//1 = Use image file(specified by file name; null file name specifies no map)

		//4

		//Specular map file name(string; default = null string)

		//78

		//Projection method of specular map mapper(default = 1):

		//1 = Planar

		//2 = Box

		//3 = Cylinder

		//4 = Sphere

		//79

		//Tiling method of specular map mapper(default = 1):

		//1 = Tile

		//2 = Crop

		//3 = Clamp

		//170

		//Auto transform method of specular map mapper(bitset; default = 1):

		//1 = No auto transform

		//2 = Scale mapper to current entity extents; translate mapper to entity origin

		//4 = Include current block transform in mapper transform

		//47

		//Transform matrix of specular map mapper(16 reals; row major format; default = identity matrix)

		//48

		//Blend factor of reflection map(real, default = 1.0)

		//171

		//Reflection map source(default = 1) :

		//0 = Use current scene

		//1 = Use image file(specified by file name; null file name specifies no map)

		//6

		//Reflection map file name(string; default = null string)

		//172

		//Projection method of reflection map mapper(default = 1):

		//1 = Planar

		//2 = Box

		//3 = Cylinder

		//4 = Sphere

		//173

		//Tiling method of reflection map mapper(default = 1):

		//1 = Tile

		//2 = Crop

		//3 = Clamp

		//174

		//Auto transform method of reflection map mapper(bitset; default = 1):

		//1 = No auto transform

		//2 = Scale mapper to current entity extents; translate mapper to entity origin

		//4 = Include current block transform in mapper transform

		//49

		//Transform matrix of reflection map mapper(16 reals; row major format; default = identity matrix)

		//140

		//Opacity percent(real; default = 1.0)

		//141

		//Blend factor of opacity map(real; default = 1.0)

		//175

		//Opacity map source(default = 1) :

		//0 = Use current scene

		//1 = Use image file(specified by file name; null file name specifies no map)

		//7

		//Opacity map file name(string; default = null string)

		//176

		//Projection method of opacity map mapper(default = 1):

		//1 = Planar

		//2 = Box

		//3 = Cylinder

		//4 = Sphere

		//177

		//Tiling method of opacity map mapper(default = 1):

		//1 = Tile

		//2 = Crop

		//3 = Clamp

		//178

		//Auto transform method of opacity map mapper(bitset; default = 1):

		//1 = No auto transform

		//2 = Scale mapper to current entity extents; translate mapper to entity origin

		//4 = Include current block transform in mapper transform

		//142

		//Transform matrix of opacity map mapper(16 reals; row major format; default = identity matrix)

		//143

		//Blend factor of bump map(real; default = 1.0)

		//179

		//Bump map source(default = 1) :

		//0 = Use current scene

		//1 = Use image file(specified by file name; null file name specifies no map)

		//8

		//Bump map file name(string; default = null string)

		//270

		//Projection method of bump map mapper(default = 1):

		//1 = Planar

		//2 = Box

		//3 = Cylinder

		//4 = Sphere

		//271

		//Tiling method of bump map mapper(default = 1):

		//1 = Tile

		//2 = Crop

		//3 = Clamp

		//272

		//Auto transform method of bump map mapper(bitset; default = 1):

		//1 = No auto transform

		//2 = Scale mapper to current entity extents; translate mapper to entity origin

		//4 = Include current block transform in mapper transform

		//144

		//Transform matrix of bump map mapper(16 reals; row major format; default = identity matrix)

		//145

		//Refraction index(real; default = 1.0)

		//146

		//Blend factor of refraction map(real; default = 1.0)

		//273

		//Refraction map source(default = 1) :

		//0 = Use current scene

		//1 = Use image file(specified by file name; null file name specifies no map)

		//9

		//Refraction map file name(string; default = null string)

		//274

		//Projection method of refraction map mapper(default = 1):

		//1 = Planar

		//2 = Box

		//3 = Cylinder

		//4 = Sphere

		//275

		//Tiling method of refraction map mapper(default = 1):

		//1 = Tile

		//2 = Crop

		//3 = Clamp

		//276

		//Auto transform method of refraction map mapper(bitset; default = 1):

		//1 = No auto transform

		//2 = Scale mapper to current entity extents; translate mapper to entity origin

		//4 = Include current block transform in mapper transform

		//147

		//Transform matrix of refraction map mapper(16 reals; row major format; default = identity matrix)

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
		//94	Channel Flags
	}
}
