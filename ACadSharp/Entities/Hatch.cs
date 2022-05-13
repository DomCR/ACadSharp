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
		/// Seed points codes (in OCS) : 10 | 20
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 98)]
		public List<XY> SeedPoints { get; set; } = new List<XY>();

		/// <summary>
		/// Seed points codes (in OCS) : 10 | 20
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name, 470)]
		public HatchGradientPattern GradientColor { get; set; } //= "LINEAR";

		/// <summary>
		/// Boundary paths (loops)
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 91)]
		public List<BoundaryPath> Paths { get; set; } = new List<BoundaryPath>();

		public Hatch() : base() { }
	}
}
