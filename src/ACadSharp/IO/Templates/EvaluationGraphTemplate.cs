using System.Collections.Generic;
using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates
{
	internal class EvaluationGraphTemplate : CadTemplate<EvaluationGraph>
	{
		public IDictionary<EvaluationGraph.GraphNode, ulong> NodeHandles { get; } = new Dictionary<EvaluationGraph.GraphNode, ulong>();

		public EvaluationGraphTemplate(EvaluationGraph evaluationGraph)
			: base(evaluationGraph)
		{
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			foreach (EvaluationGraph.GraphNode node in this.CadObject.Nodes)
			{
				var nodeHandle = this.NodeHandles[node];
				if (builder.TryGetCadObject(nodeHandle, out EvaluationExpression evExpression))
				{
					node.NodeObject = evExpression;
				}
				else
				{
					builder.Notify($"Evaluation graph with handle {this.CadObject.Handle} couldn't find the EvaluationExpression with handle {nodeHandle}", NotificationType.Warning);
				}
			}
		}
	}
}