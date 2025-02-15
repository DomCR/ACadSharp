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
		public ulong DimLineType { get; internal set; }
		public string DimltypeName { get; internal set; }
		public ulong? DimLineTypeExt1 { get; internal set; }
		public ulong? DimLineTypeExt2 { get; internal set; }
		public ulong? BlockHandle { get; internal set; }

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

			if (this.getTableReference(builder, this.DimLineType, this.DimltypeName, out LineType lineType))
			{
				this.CadObject.LineType = lineType;
			}

			if (this.getTableReference(builder, this.DimLineTypeExt1, null, out lineType))
			{
				this.CadObject.ExtensionLine1LineType = lineType;
			}

			if (this.getTableReference(builder, this.DimLineTypeExt2, null, out lineType))
			{
				this.CadObject.ExtensionLine2LineType = lineType;
			}

			if (this.getTableReference(builder, this.BlockHandle, null, out BlockRecord block))
			{
			}
		}
	}
}
