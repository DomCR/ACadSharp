using System.Collections.Generic;

namespace ACadSharp.Objects {

	public class EvaluationGraph : CadObject {

		public EvaluationGraph() {
		}

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectEvalGraph;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.EvalGraph;


		public IList<GraphNode> Nodes { get; } = new List<GraphNode>();


		public class GraphNode {

			public int Index { get; set; }

			internal int NextNodeIndex { get; set; }

			public GraphNode Next { get; internal set; }

			public int Flags { get; set; }
			public int Data1 { get; internal set; }
			public int Data2 { get; internal set; }
			public int Data3 { get; internal set; }
			public int Data4 { get; internal set; }

			public CadObject NodeObject;

			//public int
		}
	}
}