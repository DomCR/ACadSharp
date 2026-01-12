using System.Collections.Generic;
using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadMLeaderTemplate : CadEntityTemplate<MultiLeader>
	{
		public ulong ArrowheadHandle { get; set; }

		public IDictionary<ulong, bool> ArrowheadHandles { get; } = new Dictionary<ulong, bool>();

		public IDictionary<MultiLeader.BlockAttribute, ulong> BlockAttributeHandles { get; } = new Dictionary<MultiLeader.BlockAttribute, ulong>();

		public ulong BlockContentHandle { get; set; }

		public CadMLeaderAnnotContextTemplate CadMLeaderAnnotContextTemplate { get; set; }

		public ulong? LeaderLineTypeHandle { get; set; }

		public ulong LeaderStyleHandle { get; set; }

		public ulong MTextStyleHandle { get; set; }

		public CadMLeaderTemplate() : this(new MultiLeader())
		{
		}

		public CadMLeaderTemplate(MultiLeader entity) : base(entity)
		{
			CadMLeaderAnnotContextTemplate = new CadMLeaderAnnotContextTemplate(entity.ContextData);
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

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
			foreach (KeyValuePair<ulong, bool> arrowHeadHandleEntries in ArrowheadHandles)
			{
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