using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Objects
{
	public class Material : CadObject
	{
		//1	Material name(string)

		//2	Description(string, default null string)

		//70

		//Ambient color method(default = 0) :

		//0 = Use current color

		//1 = Override current color

		//40

		//Ambient color factor(real, default = 1.0; valid range is 0.0 to 1.0)

		//90

		//Ambient color value(unsigned 32-bit integer representing an AcCmEntityColor)

		//71

		//Diffuse color method(default = 0) :

		//0 = Use current color

		//1 = Override current color

		//41

		//Diffuse color factor(real, default = 1.0; valid range is 0.0 to 1.0)

		//91

		//Diffuse color value(unsigned 32-bit integer representing an AcCmEntityColor)

		//42

		//Diffuse map blend factor(real, default = 1.0)

		//72

		//Diffuse map source(default = 1) :

		//0 = Use current scene

		//1 = Use image file(specified by file name; null file name specifies no map)

		//3

		//Diffuse map file name(string, default = null string)

		//73

		//Projection method of diffuse map mapper(default = 1):

		//1 = Planar

		//2 = Box

		//3 = Cylinder

		//4 = Sphere

		//74

		//Tiling method of diffuse map mapper(default = 1):

		//1 = Tile

		//2 = Crop

		//3 = Clamp

		//75

		//Auto transform method of diffuse map mapper(bitset, default = 1) :

		//1= No auto transform

		//2 = Scale mapper to current entity extents; translate mapper to entity origin

		//4 = Include current block transform in mapper transform

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
