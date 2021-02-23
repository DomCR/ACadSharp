using ACadSharp.Attributes;
using ACadSharp.Geometry;
using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;

namespace ACadSharp.Tables
{
	public class LineType : TableEntry
	{
		/// <summary>
		/// Descriptive text for linetype
		/// </summary>
		[DxfCodeValue(DxfCode.Description)]
		public string Description { get; set; }
		public double PatternLen { get; set; }
		public char Alignment { get; set; }

		public List<LineTypeSegment> Segments { get; set; } = new List<LineTypeSegment>();

		public LineType() : base() { }
		public LineType(string name) : base(name) { }

		internal LineType(DxfEntryTemplate template) : base(template) { }
	}

	public class LineTypeSegment
	{
		public double Length { get; set; }
		public LinetypeShapeFlags Shapeflag { get; set; }
		public XY Offset { get; set; }
		public double Rotation { get; internal set; }
		public double Scale { get; internal set; }
	}

	/// <summary>
	/// Represents a line type complex element type.
	/// </summary>
	[Flags]
	public enum LinetypeShapeFlags : short
	{
		/// <summary>
		/// None.
		/// </summary>
		None = 0,
		/// <summary>
		/// Text is rotated 0 degrees, otherwise it follows the segment.
		/// </summary>
		RotationIsAbsolute = 1,
		/// <summary>
		/// Complex shape code holds the index of the shape to be drawn.
		/// </summary>
		Text = 2,
		/// <summary>
		/// Complex shape code holds the index into the text area of the string to be drawn.
		/// </summary>
		Shape = 4,
	}
}