using ACadSharp.IO.DWG;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.Templates
{
	internal class DwgTableEntryTemplate<T> : DwgTemplate<T>
		where T : TableEntry
	{
		public DwgTableEntryTemplate(T entry) : base(entry) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);
		}
	}

	internal class CadLineTypeTemplate : DwgTableEntryTemplate<LineType>
	{
		private List<int> readedCodes = new List<int>();

		public CadLineTypeTemplate(LineType entry) : base(entry) { }

		public override bool CheckDxfCode(int dxfcode, object value)
		{
			bool found = base.CheckDxfCode(dxfcode, value);
			if (found)
				return found;

			var segment = this.CadObject.Segments.LastOrDefault();
			if (segment == null || readedCodes.Contains(dxfcode))
			{
				//Clean the codes create a new element
				readedCodes.Clear();
				segment = new LineTypeSegment();
				this.CadObject.Segments.Add(segment);
			}

			switch (dxfcode)
			{
				case 9:
					segment.Text = value as string;
					found = true;
					break;
				case 74:
					segment.Shapeflag = (LinetypeShapeFlags)value;
					found = true;
					break;
				case 44:
					segment.Offset = new CSMath.XY(segment.Offset.X, (double)value);
					found = true;
					break;
				case 45:
					segment.Offset = new CSMath.XY((double)value, segment.Offset.Y);
					found = true;
					break;
				case 46:
					segment.Scale = (double)value;
					found = true;
					break;
				case 49:
					segment.Length = (double)value;
					found = true;
					break;
				case 50:
					segment.Rotation = (double)value;
					found = true;
					break;
				default:
					break;
			}

			if (found)
				readedCodes.Add(dxfcode);

			return found;
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);
		}
	}
}
