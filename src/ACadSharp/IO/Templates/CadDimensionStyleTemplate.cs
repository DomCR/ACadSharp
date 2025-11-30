using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadDimensionStyleTemplate : CadTableEntryTemplate<DimensionStyle>
	{
		public string DIMBL_Name { get; set; }

		public ulong? DIMBLK { get; set; }

		public ulong? DIMBLK1 { get; set; }

		public string DIMBLK1_Name { get; set; }

		public ulong? DIMBLK2 { get; set; }

		public string DIMBLK2_Name { get; set; }

		public ulong? DIMLDRBLK { get; set; }

		public ulong Dimltex1 { get; set; }

		public ulong Dimltex2 { get; set; }

		public ulong? Dimltype { get; set; }

		public string TextStyle_Name { get; set; }

		public ulong? TextStyleHandle { get; set; }

		public ulong? BlockHandle { get; set; }

		public bool DxfFlagsAssigned { get; set; } = false;

		public CadDimensionStyleTemplate() : base(new DimensionStyle())
		{
		}

		public CadDimensionStyleTemplate(DimensionStyle dimStyle) : base(dimStyle)
		{
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (this.getTableReference(builder, this.TextStyleHandle, TextStyle_Name, out TextStyle style))
			{
				this.CadObject.Style = style;
			}

			if (this.getTableReference(builder, this.Dimltype, null, out LineType linetType))
			{
				this.CadObject.LineType = linetType;
			}

			if (this.getTableReference(builder, this.Dimltex1, null, out LineType linetTypeEx1))
			{
				this.CadObject.LineTypeExt1 = linetTypeEx1;
			}

			if (this.getTableReference(builder, this.Dimltex2, null, out LineType linetTypeEx2))
			{
				this.CadObject.LineTypeExt2 = linetTypeEx2;
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

			if (this.getTableReference(builder, this.BlockHandle, null, out BlockRecord external))
			{
			}
		}
	}
}