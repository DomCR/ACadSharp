using ACadSharp.Attributes;
using ACadSharp.Types;
using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;

namespace ACadSharp.Tables
{
	/// <summary>
	/// Represents a <see cref="LineType"/> entry
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.TableLinetype"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Linetype"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableLinetype)]
	[DxfSubClass(DxfSubclassMarker.Linetype)]
	public class LineType : TableEntry
	{
		public static LineType ByLayer { get { return new LineType("ByLayer"); } }

		public static LineType ByBlock { get { return new LineType("ByBlock"); } }

		public static LineType Continuous { get { return new LineType("Continuous"); } }

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.LTYPE;

		/// <inheritdoc/>
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

		/// <summary>
		/// Linetype Segments
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 73)]
		public List<LineTypeSegment> Segments { get; set; } = new List<LineTypeSegment>();

		//74	Complex linetype element type(one per element). Default is 0 (no embedded shape/text)
		//The following codes are bit values:
		//1 = If set, code 50 specifies an absolute rotation; if not set, code 50 specifies a relative rotation
		//2 = Embedded element is a text string
		//4 = Embedded element is a shape

		//75	Shape number(one per element) if code 74 specifies an embedded shape
		//If code 74 specifies an embedded text string, this value is set to 0
		//If code 74 is set to 0, code 75 is omitted

		/// <summary>
		/// Pointer to STYLE object (one per element if code 74 > 0)
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public TextStyle Style { get; set; }

		public LineType() : base() { }

		public LineType(string name) : base(name) { }
	}
}