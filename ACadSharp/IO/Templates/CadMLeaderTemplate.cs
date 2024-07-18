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

		//	Context-Data Handles
		public ulong AnnotContextTextStyleHandle { get; internal set; }

		public ulong AnnotContextBlockRecordHandle { get; internal set; }

		public IList<LeaderLineSubTemplate> LeaderLineSubTemplates { get; } = new List<LeaderLineSubTemplate>();
		

		public class LeaderLineSubTemplate
		{
			public MultiLeaderAnnotContext.LeaderLine LeaderLine { get; }

			public LeaderLineSubTemplate(MultiLeaderAnnotContext.LeaderLine leaderLine)
			{ 
				this.LeaderLine = leaderLine;
			}

			public ulong LineTypeHandle { get; internal set; }

			public ulong ArrowSymbolHandle { get; internal set; }
		}


		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			MultiLeader multiLeader = (MultiLeader)this.CadObject;
			MultiLeaderAnnotContext annotContext = multiLeader.ContextData;

			if (builder.TryGetCadObject(this.AnnotContextTextStyleHandle, out TextStyle annotContextTextStyle))
			{
				annotContext.TextStyle = annotContextTextStyle;
			}
			if (builder.TryGetCadObject(this.AnnotContextBlockRecordHandle, out BlockRecord annotContextBlockRecord))
			{
				annotContext.BlockContent = annotContextBlockRecord;
			}

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

			foreach (LeaderLineSubTemplate leaderLineSubTemplate in this.LeaderLineSubTemplates)
			{
				MultiLeaderAnnotContext.LeaderLine leaderLine = leaderLineSubTemplate.LeaderLine;
				if (builder.TryGetCadObject(leaderLineSubTemplate.LineTypeHandle, out LineType leaderLinelineType))
				{
					leaderLine.LineType = leaderLinelineType;
				}
				if (builder.TryGetCadObject(leaderLineSubTemplate.ArrowSymbolHandle, out BlockRecord arrowhead))
				{
					leaderLine.Arrowhead = arrowhead;
				}
			}
		}
	}
}
