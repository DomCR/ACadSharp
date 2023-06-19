using ACadSharp.Blocks;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadDimensionStyleTemplate : CadTableEntryTemplate<DimensionStyle>
	{
		public string DIMBL_Name { get; internal set; }
		public string DIMBLK1_Name { get; internal set; }
		public string DIMBLK2_Name { get; internal set; }
		public ulong? TextStyleHandle { get; internal set; }
		public ulong? DIMLDRBLK { get; internal set; }
		public ulong? DIMBLK { get; internal set; }
		public ulong? DIMBLK1 { get; internal set; }
		public ulong? DIMBLK2 { get; internal set; }
		public ulong Dimltype { get; internal set; }
		public ulong Dimltex1 { get; internal set; }
		public ulong Dimltex2 { get; internal set; }

		public CadDimensionStyleTemplate() : base(new DimensionStyle()) { }

		public CadDimensionStyleTemplate(DimensionStyle dimStyle) : base(dimStyle) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			//TODO: implement the dimension template for the names instead of handles

			if (builder.TryGetCadObject(this.TextStyleHandle, out TextStyle style))
			{
				this.CadObject.Style = style;
			}

			if (builder.TryGetCadObject(this.DIMLDRBLK, out Block leaderArrow))
			{
				this.CadObject.LeaderArrow = leaderArrow;
			}

			if (builder.TryGetCadObject(this.DIMBLK1, out Block dimArrow1))
			{
				this.CadObject.DimArrow1 = dimArrow1;
			}

			if (builder.TryGetCadObject(this.DIMBLK2, out Block dimArrow2))
			{
				this.CadObject.DimArrow2 = dimArrow2;
			}

			if (!string.IsNullOrWhiteSpace(DIMBL_Name))
			{
				builder.Notify($"DwgDimensionStyleTemplate does not implement the dimension block for : {DIMBL_Name}");
			}

			if (!string.IsNullOrWhiteSpace(DIMBLK1_Name))
			{
				builder.Notify($"DwgDimensionStyleTemplate does not implement the dimension block for : {DIMBLK1_Name}");
			}

			if (!string.IsNullOrWhiteSpace(DIMBLK2_Name))
			{
				builder.Notify($"DwgDimensionStyleTemplate does not implement the dimension block for : {DIMBLK2_Name}");
			}
		}
	}
}
