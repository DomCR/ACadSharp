using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadDimensionStyleTemplate : CadTableEntryTemplate<DimensionStyle>
	{
		public string TextStyle_Name { get; internal set; }
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

			if (this.getTableReference(builder, this.TextStyleHandle, TextStyle_Name, out TextStyle style))
			{
				this.CadObject.Style = style;
			}

			if (this.getTableReference(builder, this.DIMLDRBLK, this.DIMBL_Name, out BlockRecord leaderArrow))
			{
				this.CadObject.LeaderArrow = leaderArrow;
			}

			if (this.getTableReference(builder, this.DIMBLK1, this.DIMBLK1_Name, out BlockRecord dimArrow1))
			{
				this.CadObject.DimArrow1 = dimArrow1;
			}

			if (this.getTableReference(builder, this.DIMBLK2, this.DIMBLK2_Name, out BlockRecord dimArrow2))
			{
				this.CadObject.DimArrow2 = dimArrow2;
			}
		}
	}
}
