using ACadSharp.Tables;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.Templates
{
	internal class CadLineTypeTemplate : DwgTableEntryTemplate<LineType>
	{
		public ulong? LtypeControlHandle { get; set; }

		public ulong? StyleHandle { get; set; }

		private List<int> readedCodes = new List<int>();

		public CadLineTypeTemplate(LineType entry) : base(entry) { }

		public override bool AddHandle(int dxfcode, ulong handle)
		{
			bool found = base.AddHandle(dxfcode, handle);
			if (found)
				return found;

			switch (dxfcode)
			{
				case 340:
					this.StyleHandle = handle;
					found = true;
					break;
				default:
					break;
			}

			return found;
		}

		public override bool CheckDxfCode(int dxfcode, object value)
		{
			bool found = base.CheckDxfCode(dxfcode, value);
			if (found)
				return found;

			var segment = this.CadObject.Segments.LastOrDefault();
			if (segment == null || this.readedCodes.Contains(dxfcode))
			{
				//Clean the codes create a new element
				this.readedCodes.Clear();
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
				case 75:
					segment.ShapeNumber = (short)value;
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
				this.readedCodes.Add(dxfcode);

			return found;
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (this.LtypeControlHandle.HasValue && this.LtypeControlHandle.Value > 0)
			{
				builder.NotificationHandler?.Invoke(
					this.CadObject,
					new NotificationEventArgs($"LtypeControlHandle not assigned : {this.LtypeControlHandle}"));
			}
		}
	}
}
