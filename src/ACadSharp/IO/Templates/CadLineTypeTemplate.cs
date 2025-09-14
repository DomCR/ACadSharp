using ACadSharp.Tables;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadLineTypeTemplate : CadTableEntryTemplate<LineType>
	{
		public class SegmentTemplate
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

		public CadLineTypeTemplate() : base(new LineType()) { }

		public CadLineTypeTemplate(LineType entry) : base(entry) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			foreach (var item in this.SegmentTemplates)
			{
				item.Build(builder);

				this.CadObject.AddSegment(item.Segment);
			}
		}
	}
}
