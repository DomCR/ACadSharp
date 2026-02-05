using ACadSharp.Objects;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadMLeaderStyleTemplate : CadTemplate<MultiLeaderStyle>
	{
		public ulong? ArrowheadHandle { get; set; }

		public ulong? BlockContentHandle { get; set; }

		public ulong? LeaderLineTypeHandle { get; set; }

		public ulong? MTextStyleHandle { get; set; }

		public CadMLeaderStyleTemplate() : this(new())
		{
		}

		public CadMLeaderStyleTemplate(MultiLeaderStyle entry) : base(entry)
		{
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (builder.TryGetCadObject(this.LeaderLineTypeHandle, out LineType lineType))
			{
				this.CadObject.LeaderLineType = lineType;
			}

			if (builder.TryGetCadObject(this.ArrowheadHandle, out BlockRecord arrowhead))
			{
				this.CadObject.Arrowhead = arrowhead;
			}

			if (builder.TryGetCadObject(this.MTextStyleHandle, out TextStyle textStyle))
			{
				this.CadObject.TextStyle = textStyle;
			}

			if (builder.TryGetCadObject(this.BlockContentHandle, out BlockRecord blockContent))
			{
				this.CadObject.BlockContent = blockContent;
			}
		}
	}
}