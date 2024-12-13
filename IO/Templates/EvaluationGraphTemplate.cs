using System.Collections.Generic;

using ACadSharp.Objects;

namespace ACadSharp.IO.Templates {
	internal class EvaluationGraphTemplate : CadTemplate<EvaluationGraph>{

		public EvaluationGraphTemplate(EvaluationGraph evaluationGraph)
			: base(evaluationGraph) {
		}

		public IDictionary<EvaluationGraph.GraphNode, ulong> NodeHandles { get; } = new Dictionary<EvaluationGraph.GraphNode, ulong>();

		public override void Build(CadDocumentBuilder builder) {
			base.Build(builder);

			foreach (EvaluationGraph.GraphNode node in this.CadObject.Nodes) {
				var nodeHandle = this.NodeHandles[node];
				if (builder.TryGetCadObject(nodeHandle, out CadObject nodeObject)) {
					node.NodeObject = nodeObject;
				}
			}
		}
	}
}