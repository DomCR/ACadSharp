using ACadSharp.Geometry;
using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables
{
	public class UCS : TableEntry
	{
		public override ObjectType ObjectType => ObjectType.UCS;

		public XYZ PaperSpaceInsertionBase { get; internal set; }
		public XYZ PaperSpaceExtMin { get; internal set; }
		public XYZ PaperSpaceExtMax { get; internal set; }
		public XY PaperSpaceLimitsMin { get; internal set; }
		public XY PaperSpaceLimitsMax { get; internal set; }
		public double PaperSpaceElevation { get; internal set; }
		public XYZ Origin { get; internal set; }
		public XYZ XAxis { get; internal set; }
		public XYZ YAxis { get; internal set; }
		public XYZ OrthographicTopDOrigin { get; internal set; }
		public XYZ OrthographicBottomDOrigin { get; internal set; }
		public XYZ OrthographicLeftDOrigin { get; internal set; }
		public XYZ OrthographicRightDOrigin { get; internal set; }
		public XYZ OrthographicFrontDOrigin { get; internal set; }
		public XYZ OrthographicBackDOrigin { get; internal set; }

		public UCS() : base() { }

		internal UCS(DxfEntryTemplate template) : base(template) { }
	}
}
