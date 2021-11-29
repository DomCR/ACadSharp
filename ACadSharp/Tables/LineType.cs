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
		public double PatternLen { get; set; }
		public char Alignment { get; set; }

		public List<LineTypeSegment> Segments { get; set; } = new List<LineTypeSegment>();

		public LineType() : base() { }
		public LineType(string name) : base(name) { }

		internal LineType(DxfEntryTemplate template) : base(template) { }
	}
}