using ACadSharp.Tables;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadLineTypeTemplate : CadTableEntryTemplate<LineType>
	{
		public class SegmentTemplate : ICadObjectTemplate
		{
			public ulong? StyleHandle { get; set; }

			public LineType.Segment Segment { get; set; } = new LineType.Segment();

			public void Build(CadDocumentBuilder builder)
			{
				if (builder.TryGetCadObject<TextStyle>(this.StyleHandle, out TextStyle style))
				{
					this.Segment.Style = style;
				}
			}
		}

		public ulong? LtypeControlHandle { get; set; }

		public double? TotalLen { get; set; }

		public List<SegmentTemplate> SegmentTemplates { get; set; } = new List<SegmentTemplate>();

		private List<int> _readedCodes = new List<int>();

		public CadLineTypeTemplate() : base(new LineType()) { }

		public CadLineTypeTemplate(LineType entry) : base(entry) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			foreach (var item in this.SegmentTemplates)
			{
				item.Build(builder);
				this.CadObject.AddSegment(item.Segment);
			}
		}
	}
}
