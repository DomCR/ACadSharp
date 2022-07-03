using ACadSharp.Blocks;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadDimensionStyleTemplate : CadTemplate<DimensionStyle>
	{
		public string DIMBL_Name { get; internal set; }
		public string DIMBLK1_Name { get; internal set; }
		public string DIMBLK2_Name { get; internal set; }
		public ulong? DIMTXSTY { get; internal set; }
		public ulong? DIMLDRBLK { get; internal set; }
		public ulong? DIMBLK { get; internal set; }
		public ulong? DIMBLK1 { get; internal set; }
		public ulong? DIMBLK2 { get; internal set; }
		public ulong Dimltype { get; internal set; }
		public ulong Dimltex1 { get; internal set; }
		public ulong Dimltex2 { get; internal set; }

		public CadDimensionStyleTemplate(DimensionStyle dimStyle) : base(dimStyle) { }

		public override bool AddHandle(int dxfcode, ulong handle)
		{
			bool value = base.AddHandle(dxfcode, handle);
			if (value)
				return value;

			switch (dxfcode)
			{
				case 340:
					DIMTXSTY = handle;
					value = true;
					break;
				default:
					break;
			}

			return value;
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			//TODO: implement the dimension template for the names instead of handles

			if (this.DIMTXSTY.HasValue && builder.TryGetCadObject(this.DIMTXSTY.Value, out TextStyle style))
			{
				this.CadObject.Style = style;
			}

			if (this.DIMLDRBLK.HasValue && builder.TryGetCadObject(this.DIMLDRBLK.Value, out Block leaderArrow))
			{
				this.CadObject.LeaderArrow = leaderArrow;
			}

			if (this.DIMBLK1.HasValue && builder.TryGetCadObject(this.DIMBLK1.Value, out Block dimArrow1))
			{
				this.CadObject.DimArrow1 = dimArrow1;
			}

			if (this.DIMBLK2.HasValue && builder.TryGetCadObject(this.DIMBLK2.Value, out Block dimArrow2))
			{
				this.CadObject.DimArrow2 = dimArrow2;
			}

			if (!string.IsNullOrWhiteSpace(DIMBL_Name))
			{
				builder.Notify(new NotificationEventArgs($"DwgDimensionStyleTemplate does not implement the dimension block for : {DIMBL_Name}"));
			}

			if (!string.IsNullOrWhiteSpace(DIMBLK1_Name))
			{
				builder.Notify(new NotificationEventArgs($"DwgDimensionStyleTemplate does not implement the dimension block for : {DIMBLK1_Name}"));
			}

			if (!string.IsNullOrWhiteSpace(DIMBLK2_Name))
			{
				builder.Notify(new NotificationEventArgs($"DwgDimensionStyleTemplate does not implement the dimension block for : {DIMBLK2_Name}"));
			}
		}
	}
}
