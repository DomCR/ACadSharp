using ACadSharp.Attributes;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Numerics;
using System.Security.Claims;
using System.Threading;
using System;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Wipeout"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityWipeout"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Wipeout"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityWipeout)]
	[DxfSubClass(DxfSubclassMarker.Wipeout)]
	public class Wipeout : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		//100	Subclass marker(AcDbRasterImage)

		/// <summary>
		/// Class version
		/// </summary>
		[DxfCodeValue(90)]
		public int ClassVersion { get; set; }

		/// <summary>
		/// Insertion point(in WCS)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ InsertPoint { get; set; }

		/// <summary>
		/// U-vector of a single pixel(points along the visual bottom of the image, starting at the insertion point) (in WCS)
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ UVector { get; set; }

		/// <summary>
		/// V-vector of a single pixel(points along the visual left side of the image, starting at the insertion point) (in WCS)
		/// </summary>
		[DxfCodeValue(12, 22, 32)]
		public XYZ VVector { get; set; }

		//13	Image size in pixels
		//DXF: U value; APP: 2D point(U and V values)
		//23	DXF: V value of image size in pixels
		[DxfCodeValue(13, 23)]
		public XY Size { get; set; }

		//340

		//Hard reference to imagedef object

		//70

		//Image display properties:

		//1 = Show image

		//2 = Show image when not aligned with screen

		//4 = Use clipping boundary

		//8 = Transparency is on

		//280

		//Clipping state: 0 = Off; 1 = On

		//281

		//Brightness value(0-100; default = 50)

		//282

		//Contrast value(0-100; default = 50)

		//283

		//Fade value(0-100; default = 0)

		//360

		//Hard reference to imagedef_reactor object

		//71

		//Clipping boundary type. 1 = Rectangular; 2 = Polygonal

		//91

		//Number of clip boundary vertices that follow

		//14

		//Clip boundary vertex(in OCS)

		//DXF: X value; APP: 2D point(multiple entries)

		//NOTE 1) For rectangular clip boundary type, two opposite corners must be specified.Default is (-0.5,-0.5), (size.x-0.5, size.y-0.5). 2) For polygonal clip boundary type, three or more vertices must be specified.Polygonal vertices must be listed sequentially

		//24

		//DXF: Y value of clip boundary vertex(in OCS) (multiple entries)
	}
}
