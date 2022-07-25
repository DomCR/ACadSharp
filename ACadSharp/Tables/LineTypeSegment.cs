using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Tables
{
	public partial class LineType
	{
		public class Segment
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
			/// Shape number 
			/// </summary>
			[DxfCodeValue(75)]
			public short ShapeNumber { get; set; }

			/// <summary>
			/// Offset
			/// </summary>
			[DxfCodeValue(44, 45)]
			public XY Offset { get; set; }

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
			public TextStyle Style { get; set; }

			/// <summary>
			/// Line type where this segment belongs
			/// </summary>
			public LineType LineType { get; internal set; }
		}
	}
}