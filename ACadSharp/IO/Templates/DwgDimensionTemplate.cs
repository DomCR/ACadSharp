using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class DwgDimensionTemplate : DwgEntityTemplate
	{
		public ulong StyleHandle { get; set; }
		public ulong BlockHandle { get; set; }

		public DwgDimensionTemplate(Dimension dimension) : base(dimension)
		{
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			Dimension dimension = this.CadObject as Dimension;

			if (builder.TryGetCadObject<DimensionStyle>(StyleHandle, out DimensionStyle style))
			{
				dimension.Style = style;
			}

			if (builder.TryGetCadObject<BlockReference>(BlockHandle, out BlockReference block))
			{
				dimension.Block = block;
			}
		}
	}
}
