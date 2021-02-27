using ACadSharp.Attributes;
using ACadSharp.Geometry;
using ACadSharp.IO.Templates;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public class HatchPattern
	{
		public static HatchPattern Solid
		{
			get
			{
				HatchPattern pattern = new HatchPattern("SOLID");

				return pattern;
			}
		}

		public class Line
		{
			public double Angle { get; internal set; }
			public XY BasePoint { get; internal set; }
			public XY Offset { get; internal set; }
			public List<double> DashLengths { get; set; } = new List<double>();
		}

		public string Name { get; set; }
		public double Angle { get; set; }
		public double Scale { get; set; }

		public List<Line> Lines { get; set; } = new List<Line>();

		public HatchPattern(string name)
		{
			Name = name;
		}
	}

	public class HatchGradientPattern : HatchPattern
	{
		public int Reserved { get; set; }
		public double Shift { get; set; }
		public bool IsSingleColorGradient { get; set; }
		public double ColorTint { get; set; }
		public List<Color> Colors { get; set; } = new List<Color>();
		public HatchGradientPattern(string name) : base(name) { }
	}

	public class HatchBoundaryPath
	{
		#region Boundary path edge classes
		public abstract class Edge
		{

		}
		public class Line : Edge
		{
			public XY Start { get; set; }
			public XY End { get; set; }
		}
		public class Arc : Edge
		{
			public XY Center { get; set; }
			public double Radius { get; set; }
			public double StartAngle { get; set; }
			public double EndAngle { get; set; }
			public bool CounterClockWise { get; set; }
		}
		public class Ellipse : Edge
		{
			public XY Center { get; set; }
			public XY MajorAxisEndPoint { get; set; }
			public double MinorToMajorRatio { get; set; }
			public double StartAngle { get; set; }
			public double EndAngle { get; set; }
			public bool CounterClockWise { get; set; }
		}
		public class Spline : Edge
		{
			public int Degree { get; internal set; }
			public bool Rational { get; internal set; }
			public bool Periodic { get; internal set; }
			public List<double> Knots { get; set; } = new List<double>();
			/// <remarks>
			/// Position values are only X and Y, Z represents the weight.
			/// </remarks>
			public List<XYZ> ControlPoints { get; set; } = new List<XYZ>();
		}
		public class Polyline : Edge
		{
			public bool IsClosed { get; set; }
			/// <remarks>
			/// Position values are only X and Y, Z represents the Bulge.
			/// </remarks>
			public List<XYZ> Vertices { get; set; } = new List<XYZ>();
		}
		#endregion

		public List<Edge> Edges { get; set; } = new List<Edge>();
		public BoundaryPathFlags Flags { get; set; }
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
		/// <summary>
		/// Pattern of this hatch.
		/// </summary>
		/// <value>
		/// Default value: SOLID pattern.
		/// </value>
		[DxfCodeValue(DxfCode.ShapeName)]
		public HatchPattern Pattern { get; set; } = HatchPattern.Solid;

		//70	Solid fill flag(0 = pattern fill; 1 = solid fill); for MPolygon, the version of MPolygon
		[DxfCodeValue(DxfCode.Int16)]
		public bool IsSolid { get; set; }

		//63	For MPolygon, pattern fill color as the ACI

		[DxfCodeValue(DxfCode.HatchAssociative)]
		public bool IsAssociative { get; set; }

		//91	Number of boundary paths(loops)

		//varies
		//Boundary path data.Repeats number of times specified by code 91. See Boundary Path Data

		[DxfCodeValue(DxfCode.SmoothType)]
		public HatchStyleType HatchStyle { get;  set; }
		[DxfCodeValue(DxfCode.HatchPatternType)]
		public HatchPatternType HatchPatternType { get;  set; }

		[DxfCodeValue(DxfCode.HatchPatternAngle)]
		public double PatternAngle { get { return Pattern.Angle; } set { Pattern.Angle = value; } }

		//41	Hatch pattern scale or spacing(pattern fill only)
		[DxfCodeValue(DxfCode.XScaleFactor)]
		public double PatternScale { get { return Pattern.Scale; } set { Pattern.Scale = value; } }

		//73	For MPolygon, boundary annotation flag:
		//0 = boundary is not an annotated boundary
		//1 = boundary is an annotated boundary

		[DxfCodeValue(DxfCode.HatchIsDoubleFlag)]
		public bool IsDouble { get; set; }

		//78	Number of pattern definition lines
		//varies
		//Pattern line data.Repeats number of times specified by code 78. See Pattern Data

		//47	Pixel size used to determine the density to perform various intersection and ray casting operations in hatch pattern computation for associative hatches and hatches created with the Flood method of hatching
		[DxfCodeValue(DxfCode.PixelScale)]
		public double PixelSize { get; set; }

		//98	Number of seed points

		//11	For MPolygon, offset vector

		//99	For MPolygon, number of degenerate boundary paths(loops), where a degenerate boundary path is a border that is ignored by the hatch

		//10	Seed point(in OCS)
		//DXF: X value; APP: 2D point(multiple entries)
		//20	DXF: Y value of seed point(in OCS); (multiple entries)
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

		public List<HatchBoundaryPath> Paths { get; set; } = new List<HatchBoundaryPath>();

		public Hatch() : base() { }

		internal Hatch(DxfEntityTemplate template) : base(template) { }
	}

	public class BoundaryPath
	{

	}
}
