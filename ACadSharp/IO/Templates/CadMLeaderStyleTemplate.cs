using ACadSharp.Objects;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadMLeaderStyleTemplate : CadTemplate<MultiLeaderStyle>
	{
		public CadMLeaderStyleTemplate(MultiLeaderStyle entry) : base(entry) { }

		public ulong LeaderLineTypeHandle { get; internal set; }

		public ulong ArrowheadHandle { get; internal set; }

		public ulong MTextStyleHandle { get; internal set; }

		public ulong BlockContentHandle { get; internal set; }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (builder.TryGetCadObject(this.LeaderLineTypeHandle, out LineType lineType))
			{
				this.CadObject.LeaderLineType = lineType;
			}

			//if (builder.TryGetCadObject(this.ArrowheadHandle, out Arr arrowhead)) {
			//	this.CadObject.Arrowhead = arrowhead;
			//}
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
