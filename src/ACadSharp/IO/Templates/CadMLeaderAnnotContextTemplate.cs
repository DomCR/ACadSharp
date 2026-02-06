using System.Collections.Generic;

using ACadSharp.Objects;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadMLeaderAnnotContextTemplate : CadAnnotScaleObjectContextDataTemplate
	{
		public ulong BlockRecordHandle { get; internal set; }

		public IList<LeaderLineTemplate> LeaderLineTemplates { get; } = new List<LeaderLineTemplate>();

		public ulong TextStyleHandle { get; internal set; }

		public CadMLeaderAnnotContextTemplate(MultiLeaderObjectContextData cadObject)
			: base(cadObject)
		{
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);
			MultiLeaderObjectContextData annotContext = (MultiLeaderObjectContextData)this.CadObject;

			if (builder.TryGetCadObject(this.TextStyleHandle, out TextStyle annotContextTextStyle))
			{
				annotContext.TextStyle = annotContextTextStyle;
			}
			if (builder.TryGetCadObject(this.BlockRecordHandle, out BlockRecord annotContextBlockRecord))
			{
				annotContext.BlockContent = annotContextBlockRecord;
			}

			foreach (LeaderLineTemplate leaderLineSubTemplate in this.LeaderLineTemplates)
			{
				leaderLineSubTemplate.Build(builder);
			}
		}

		public class LeaderLineTemplate : ICadTemplate
		{
			public ulong? ArrowSymbolHandle { get; internal set; }

			public MultiLeaderObjectContextData.LeaderLine LeaderLine { get; }

			public ulong? LineTypeHandle { get; internal set; }

			public LeaderLineTemplate() : this(new()) { }

			public LeaderLineTemplate(MultiLeaderObjectContextData.LeaderLine leaderLine)
			{
				this.LeaderLine = leaderLine;
			}

			public void Build(CadDocumentBuilder builder)
			{
				if (builder.TryGetCadObject(LineTypeHandle, out LineType leaderLinelineType))
				{
					LeaderLine.LineType = leaderLinelineType;
				}
				if (builder.TryGetCadObject(ArrowSymbolHandle, out BlockRecord arrowhead))
				{
					LeaderLine.Arrowhead = arrowhead;
				}
			}
		}
	}
}