using System.Collections.Generic;

using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadMLeaderTemplate : CadEntityTemplate
	{
		public CadMLeaderTemplate(MultiLeader entity) : base(entity) { }

		public ulong LeaderStyleHandle { get; internal set; }

		public ulong LeaderLineTypeHandle { get; internal set; }

		public ulong MTextStyleHandle { get; internal set; }

		public ulong BlockContentHandle { get; internal set; }

		public ulong ArrowheadHandle { get; internal set; }

		public IDictionary<ulong, bool> ArrowheadHandles { get; } = new Dictionary<ulong, bool>();


		public IDictionary<MultiLeader.BlockAttribute, ulong> BlockAttributeHandles { get; } = new Dictionary<MultiLeader.BlockAttribute, ulong>();


		public CadMLeaderAnnotContextTemplate CadMLeaderAnnotContextTemplate { get; set; }

		protected override void build(CadDocumentBuilder builder)
		{
			base.Build(builder);
			this.CadMLeaderAnnotContextTemplate.Build(builder);

			MultiLeader multiLeader = (MultiLeader)this.CadObject;

			if (builder.TryGetCadObject(this.LeaderStyleHandle, out MultiLeaderStyle leaderStyle))
			{
				multiLeader.Style = leaderStyle;
			}
			if (builder.TryGetCadObject(this.LeaderLineTypeHandle, out LineType lineType))
			{
				multiLeader.LeaderLineType = lineType;
			}
			if (builder.TryGetCadObject(this.MTextStyleHandle, out TextStyle textStyle))
			{
				multiLeader.TextStyle = textStyle;
			}
			if (builder.TryGetCadObject(this.BlockContentHandle, out BlockRecord blockContent))
			{
				multiLeader.BlockContent = blockContent;
			}
			if (builder.TryGetCadObject(this.ArrowheadHandle, out BlockRecord arrowHead))
			{
				multiLeader.Arrowhead = arrowHead;
			}

			//	TODO
			foreach (KeyValuePair<ulong, bool> arrowHeadHandleEntries in ArrowheadHandles) {
			}

			foreach (MultiLeader.BlockAttribute blockAttribute in multiLeader.BlockAttributes)
			{
				ulong attributeHandle = this.BlockAttributeHandles[blockAttribute];
				if (builder.TryGetCadObject(attributeHandle, out AttributeDefinition attributeDefinition))
				{
					blockAttribute.AttributeDefinition = attributeDefinition;
				}
			}
		}
	}
}
