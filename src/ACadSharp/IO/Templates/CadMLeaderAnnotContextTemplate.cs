using System.Collections.Generic;

using ACadSharp.Objects;
using ACadSharp.Tables;


namespace ACadSharp.IO.Templates {
	internal class CadMLeaderAnnotContextTemplate : CadAnnotScaleObjectContextDataTemplate {
		public CadMLeaderAnnotContextTemplate(MultiLeaderObjectContextData cadObject)
			: base(cadObject)
		{
		}

		public ulong TextStyleHandle { get; internal set; }

		public ulong BlockRecordHandle { get; internal set; }

		public IList<LeaderLineSubTemplate> LeaderLineSubTemplates { get; } = new List<LeaderLineSubTemplate>();


		public class LeaderLineSubTemplate
		{
			public MultiLeaderObjectContextData.LeaderLine LeaderLine { get; }

			public LeaderLineSubTemplate(MultiLeaderObjectContextData.LeaderLine leaderLine) {
				this.LeaderLine = leaderLine;
			}

			public ulong LineTypeHandle { get; internal set; }

			public ulong ArrowSymbolHandle { get; internal set; }
		}


		protected override void build(CadDocumentBuilder builder)
		{
			base.Build(builder);
			MultiLeaderObjectContextData annotContext = (MultiLeaderObjectContextData)this.CadObject;

			if (builder.TryGetCadObject(this.TextStyleHandle, out TextStyle annotContextTextStyle)) {
				annotContext.TextStyle = annotContextTextStyle;
			}
			if (builder.TryGetCadObject(this.BlockRecordHandle, out BlockRecord annotContextBlockRecord)) {
				annotContext.BlockContent = annotContextBlockRecord;
			}

			foreach (LeaderLineSubTemplate leaderLineSubTemplate in this.LeaderLineSubTemplates) {
				MultiLeaderObjectContextData.LeaderLine leaderLine = leaderLineSubTemplate.LeaderLine;
				if (builder.TryGetCadObject(leaderLineSubTemplate.LineTypeHandle, out LineType leaderLinelineType)) {
					leaderLine.LineType = leaderLinelineType;
				}
				if (builder.TryGetCadObject(leaderLineSubTemplate.ArrowSymbolHandle, out BlockRecord arrowhead)) {
					leaderLine.Arrowhead = arrowhead;
				}
			}
		}
	}
}
