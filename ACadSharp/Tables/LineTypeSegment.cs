using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Tables
{
	public class LineTypeSegment
	{
		/// <summary>
		/// Dash, dot or space length 
		/// </summary>
		[DxfCodeValue(49)]
		public double Length { get; set; }

		/// <summary>
		/// Complex linetype element type
		/// </summary>
		[DxfCodeValue(74)]
		public LinetypeShapeFlags Shapeflag { get; set; }

		/// <summary>
		/// Offset
		/// </summary>
		[DxfCodeValue(44, 45)]
		public XY Offset { get; set; }

		//50	R = (relative) or A = (absolute) rotation value in radians of embedded shape or text;
		//one per element if code 74 specifies an embedded shape or text string
		
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

		/// <summary>
		/// Text string
		/// </summary>
		/// <remarks>
		/// Only present if <see cref="LinetypeShapeFlags.Text"/> is present
		/// </remarks>
		[DxfCodeValue(9)]
		public string Text { get; set; }

		/// <summary>
		/// Pointer to STYLE object (one per element if code 74 > 0)
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public TextStyle Style { get; set; }	//TODO: set this to the parent

		/// <summary>
		/// Line type where this segment belongs
		/// </summary>
		public LineType LineType { get; }
	}
}