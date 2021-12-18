using ACadSharp.Attributes;
using ACadSharp.Types;
using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;

namespace ACadSharp.Tables
{
	public class LineType : TableEntry
	{
		public static readonly LineType ByLayer = new LineType("ByLayer");

		public override ObjectType ObjectType => ObjectType.LTYPE;
		public override string ObjectName => DxfFileToken.TableLinetype;


		/// <summary>
		/// Descriptive text for linetype
		/// </summary>
		[DxfCodeValue(3)]
		public string Description { get; set; }

		/// <summary>
		/// Total pattern length
		/// </summary>
		[DxfCodeValue(40)]
		public double PatternLen { get; set; }

		/// <summary>
		/// Alignment code
		/// </summary>
		/// <value>
		/// value is always 65, the ASCII code for A
		/// </value>
		[DxfCodeValue(72)]
		public char Alignment { get; set; } = 'A';

		//73	The number of linetype elements

		//49	Dash, dot or space length(one entry per element)

		//74	Complex linetype element type(one per element). Default is 0 (no embedded shape/text)
		//The following codes are bit values:
		//1 = If set, code 50 specifies an absolute rotation; if not set, code 50 specifies a relative rotation
		//2 = Embedded element is a text string
		//4 = Embedded element is a shape

		//75	Shape number(one per element) if code 74 specifies an embedded shape

		//If code 74 specifies an embedded text string, this value is set to 0

		//If code 74 is set to 0, code 75 is omitted

		//340	Pointer to STYLE object (one per element if code 74 > 0)

		//9	Text string (one per element if code 74 = 2)

		public List<LineTypeSegment> Segments { get; set; } = new List<LineTypeSegment>();

		public LineType() : base() { }

		public LineType(string name) : base(name) { }
	}
}