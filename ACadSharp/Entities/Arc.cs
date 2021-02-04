using ACadSharp.Attributes;
using ACadSharp.Geometry;
using ACadSharp.IO.Templates;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Entities
{
	public class Arc : Entity
	{
		public override ObjectType ObjectType => ObjectType.ARC;
		public override string ObjectName => DxfFileToken.EntityArc;

		//100	Subclass marker(AcDbCircle)
		/// <summary>
		/// Specifies the distance a 2D AutoCAD object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(DxfCode.Thickness)]
		public double Thickness { get; set; } = 0.0;
		/// <summary>
		/// Specifies the center of an arc, circle, ellipse, view, or viewport.
		/// </summary>
		[DxfCodeValue(DxfCode.XCoordinate, DxfCode.YCoordinate, DxfCode.ZCoordinate)]
		public XYZ Center { get; set; } = XYZ.Zero;
		/// <summary>
		/// Specifies the radius of an arc, circle, or position marker.
		/// </summary>
		[DxfCodeValue(DxfCode.Real)]
		public double Radius { get; set; } = 1.0;

		//100	Subclass marker(AcDbArc)
		/// <summary>
		/// The start angle in radians.
		/// </summary>
		[DxfCodeValue(DxfCode.Angle)]
		public double StartAngle { get; set; } = 0.0;
		/// <summary>
		/// The end angle in radians. Use 6.28 radians to specify a closed circle or ellipse.
		/// </summary>
		[DxfCodeValue(DxfCode.EndAngle)]
		public double EndAngle { get; set; } = 180.0;

		public Arc() : base() { }

		internal Arc(DxfEntityTemplate template) : base(template) { }
	}

	public class Hatch : Entity
	{
		public override ObjectType ObjectType => ObjectType.HATCH;
		public override string ObjectName => DxfFileToken.EntityHatch;

		//100	Subclass marker(AcDbHatch)
		/// <summary>
		/// The current elevation of the object.
		/// </summary>
		[DxfCodeValue(DxfCode.ZCoordinate)]
		public double Elevation { get; set; }

		//2	Hatch pattern name	(HatchPattern)

		//70	Solid fill flag(0 = pattern fill; 1 = solid fill); for MPolygon, the version of MPolygon

		//63	For MPolygon, pattern fill color as the ACI

		//71	Associativity flag(0 = non-associative; 1 = associative); for MPolygon, solid-fill flag(0 = lacks solid fill; 1 = has solid fill)

		//91	Number of boundary paths(loops)	
		//varies
		//Boundary path data.Repeats number of times specified by code 91. See Boundary Path Data

		//75	Hatch style:
		//0 = Hatch “odd parity” area (Normal style)
		//1 = Hatch outermost area only (Outer style)
		//2 = Hatch through entire area (Ignore style)

		//76	Hatch pattern type:
		//0 = User-defined
		//1 = Predefined
		//2 = Custom

		//52	Hatch pattern angle (pattern fill only)

		//41

		//Hatch pattern scale or spacing(pattern fill only)

		//73

		//For MPolygon, boundary annotation flag:

		//0 = boundary is not an annotated boundary

		//1 = boundary is an annotated boundary

		//77

		//Hatch pattern double flag(pattern fill only):

		//0 = not double

		//1 = double

		//78

		//Number of pattern definition lines

		//varies

		//Pattern line data.Repeats number of times specified by code 78. See Pattern Data

		//47

		//Pixel size used to determine the density to perform various intersection and ray casting operations in hatch pattern computation for associative hatches and hatches created with the Flood method of hatching

		//98

		//Number of seed points

		//11

		//For MPolygon, offset vector

		//99

		//For MPolygon, number of degenerate boundary paths(loops), where a degenerate boundary path is a border that is ignored by the hatch

		//10

		//Seed point(in OCS)

		//DXF: X value; APP: 2D point(multiple entries)

		//20

		//DXF: Y value of seed point(in OCS); (multiple entries)

		//450

		//Indicates solid hatch or gradient; if solid hatch, the values for the remaining codes are ignored but must be present.Optional; if code 450 is in the file, then the following codes must be in the file: 451, 452, 453, 460, 461, 462, and 470. If code 450 is not in the file, then the following codes must not be in the file: 451, 452, 453, 460, 461, 462, and 470

		//0 = Solid hatch

		//1 = Gradient

		//451

		//Zero is reserved for future use

		//452

		//Records how colors were defined and is used only by dialog code:

		//0 = Two-color gradient

		//1 = Single-color gradient

		//453

		//Number of colors:

		//0 = Solid hatch

		//2 = Gradient

		//460

		//Rotation angle in radians for gradients(default = 0, 0)

		//461

		//Gradient definition; corresponds to the Centered option on the Gradient Tab of the Boundary Hatch and Fill dialog box.Each gradient has two definitions, shifted and non-shifted.A Shift value describes the blend of the two definitions that should be used.A value of 0.0 means only the non-shifted version should be used, and a value of 1.0 means that only the shifted version should be used.

		//462


		//Color tint value used by dialog code (default = 0, 0; range is 0.0 to 1.0). The color tint value is a gradient color and controls the degree of tint in the dialog when the Hatch group code 452 is set to 1.

		//463

		//Reserved for future use:

		//0 = First value

		//1 = Second value

		//470	String(default = LINEAR)

		public Hatch() : base() { }

		internal Hatch(DxfEntityTemplate template) : base(template) { }
	}
}
