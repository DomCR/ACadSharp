using ACadSharp.Tables;
using System.Collections.Generic;
using System.Linq;

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

		public List<SegmentTemplate> SegmentTemplates { get; set; } = new List<SegmentTemplate>();

		private List<int> _readedCodes = new List<int>();

		public CadLineTypeTemplate() : base(new LineType()) { }

		public CadLineTypeTemplate(LineType entry) : base(entry) { }

		public override bool CheckDxfCode(int dxfcode, object value)
		{
			bool found = base.CheckDxfCode(dxfcode, value);
			if (found)
				return found;

			var template = this.SegmentTemplates.LastOrDefault();
			if (template == null || this._readedCodes.Contains(dxfcode))
			{
				//Clean the codes create a new element
				this._readedCodes.Clear();
				template = new SegmentTemplate();
				this.SegmentTemplates.Add(template);
			}

			switch (dxfcode)
			{
				case 9:
					template.Segment.Text = value as string;
					found = true;
					break;
				case 74:
					template.Segment.Shapeflag = (LinetypeShapeFlags)value;
					found = true;
					break;
				case 75:
					template.Segment.ShapeNumber = (short)value;
					found = true;
					break;
				case 44:
					template.Segment.Offset = new CSMath.XY(template.Segment.Offset.X, (double)value);
					found = true;
					break;
				case 45:
					template.Segment.Offset = new CSMath.XY((double)value, template.Segment.Offset.Y);
					found = true;
					break;
				case 46:
					template.Segment.Scale = (double)value;
					found = true;
					break;
				case 49:
					template.Segment.Length = (double)value;
					found = true;
					break;
				case 50:
					template.Segment.Rotation = (double)value;
					found = true;
					break;
				case 340:
					template.StyleHandle = (ulong)value;
					found = true;
					break;
				default:
					break;
			}

			if (found)
				this._readedCodes.Add(dxfcode);

			return found;
		}

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
