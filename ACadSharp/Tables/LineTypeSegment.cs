using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Tables
{
	public class LineTypeSegment
	{
		public double Length { get; set; }
		public LinetypeShapeFlags Shapeflag { get; set; }

		//44	X = X offset value(optional); multiple entries can exist
		//45	Y = Y offset value(optional); multiple entries can exist
		[DxfCodeValue(44, 45)]
		public XY Offset { get; set; }

		//50	R = (relative) or A = (absolute) rotation value in radians of embedded shape or text; one per element if code 74 specifies an embedded shape or text string
		/// <summary>
		/// Rotation value in radians of embedded shape or text
		/// </summary>
		[DxfCodeValue(50)]
		public double Rotation { get; set; }

		/// <summary>
		/// Scale value
		/// </summary>
		[DxfCodeValue(46)]
		public double Scale { get; set; }
	}
}