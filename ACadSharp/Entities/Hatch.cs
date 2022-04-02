using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Hatch"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityHatch"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Hatch"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityHatch)]
	[DxfSubClass(DxfSubclassMarker.Hatch)]
	public partial class Hatch : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.HATCH;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityHatch;

		/// <summary>
		/// The current elevation of the object.
		/// </summary>
		[DxfCodeValue(30)]
		public double Elevation { get; set; }

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Pattern of this hatch.
		/// </summary>
		/// <value>
		/// Default value: SOLID pattern.
		/// </value>
		[DxfCodeValue(DxfReferenceType.Name, 2)]
		public HatchPattern Pattern { get; set; } = HatchPattern.Solid;

		/// <summary>
		/// Solid fill flag
		/// </summary>
		[DxfCodeValue(70)]
		public bool IsSolid { get; set; }

		//63	For MPolygon, pattern fill color as the ACI

		/// <summary>
		/// Associativity flag
		/// </summary>
		[DxfCodeValue(71)]
		public bool IsAssociative { get; set; }

		/// <summary>
		/// Hatch style
		/// </summary>
		[DxfCodeValue(75)]
		public HatchStyleType HatchStyle { get; set; }

		/// <summary>
		/// Hatch pattern type
		/// </summary>
		[DxfCodeValue(76)]
		public HatchPatternType HatchPatternType { get; set; }

		/// <summary>
		/// Hatch pattern angle (pattern fill only)
		/// </summary>
		[DxfCodeValue(52)]
		public double PatternAngle { get { return Pattern.Angle; } set { Pattern.Angle = value; } }

		/// <summary>
		/// Hatch pattern scale or spacing(pattern fill only)
		/// </summary>
		[DxfCodeValue(41)]
		public double PatternScale { get { return Pattern.Scale; } set { Pattern.Scale = value; } }

		//73	For MPolygon, boundary annotation flag:
		//0 = boundary is not an annotated boundary
		//1 = boundary is an annotated boundary

		/// <summary>
		/// Hatch pattern double flag (pattern fill only)
		/// </summary>
		[DxfCodeValue(77)]
		public bool IsDouble { get; set; }

		//78	Number of pattern definition lines
		//varies
		//Pattern line data.Repeats number of times specified by code 78. See Pattern Data

		/// <summary>
		/// Pixel size used to determine the density to perform various intersection and ray casting operations in hatch pattern computation for associative hatches and hatches created with the Flood method of hatching
		/// </summary>
		[DxfCodeValue(47)]
		public double PixelSize { get; set; }


		//11	For MPolygon, offset vector

		//99	For MPolygon, number of degenerate boundary paths(loops), where a degenerate boundary path is a border that is ignored by the hatch

		/// <summary>
		/// Seed points
		/// </summary>
		//10	Seed point(in OCS)
		//DXF: X value; APP: 2D point(multiple entries)
		//20	DXF: Y value of seed point(in OCS); (multiple entries)
		[DxfCodeValue(DxfReferenceType.Count, 98)]
		public List<XY> SeedPoints { get; set; } = new List<XY>();

		//450	Indicates solid hatch or gradient; if solid hatch, the values for the remaining codes are ignored but must be present.Optional; if code 450 is in the file, then the following codes must be in the file: 451, 452, 453, 460, 461, 462, and 470. If code 450 is not in the file, then the following codes must not be in the file: 451, 452, 453, 460, 461, 462, and 470
		//0 = Solid hatch
		//1 = Gradient

		//451	Zero is reserved for future use

		//452	Records how colors were defined and is used only by dialog code:
		//0 = Two-color gradient
		//1 = Single-color gradient

		//453	Number of colors:
		//0 = Solid hatch
		//2 = Gradient

		//460	Rotation angle in radians for gradients(default = 0, 0)

		//461	Gradient definition; corresponds to the Centered option on the Gradient Tab of the Boundary Hatch and Fill dialog box.Each gradient has two definitions, shifted and non-shifted.A Shift value describes the blend of the two definitions that should be used.A value of 0.0 means only the non-shifted version should be used, and a value of 1.0 means that only the shifted version should be used.

		//462	Color tint value used by dialog code (default = 0, 0; range is 0.0 to 1.0). The color tint value is a gradient color and controls the degree of tint in the dialog when the Hatch group code 452 is set to 1.

		//463	Reserved for future use:
		//0 = First value
		//1 = Second value

		//470	String(default = LINEAR)

		/// <summary>
		/// Boundary paths (loops)
		/// </summary>
		//91	Number of boundary paths(loops)
		//varies
		//Boundary path data.Repeats number of times specified by code 91. See Boundary Path Data
		[DxfCodeValue(DxfReferenceType.Count, 91)]
		public List<BoundaryPath> Paths { get; set; } = new List<BoundaryPath>();

		public Hatch() : base() { }
	}
}
