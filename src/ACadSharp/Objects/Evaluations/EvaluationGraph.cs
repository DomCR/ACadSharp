using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Objects.Evaluations
{
	/// <summary>
	/// Represents an evaluation graph containing a list of <see cref="GraphNode"/>
	/// objects.
	/// </summary>
	[DxfName(DxfFileToken.ObjectEvalGraph)]
	[DxfSubClass(DxfSubclassMarker.EvalGraph)]
	public partial class EvaluationGraph : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectEvalGraph;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.EvalGraph;

		[DxfCodeValue(96)]
		public int Value96 { get; set; }

		[DxfCodeValue(97)]
		public int Value97 { get; set; }

		/// <summary>
		/// Gets a list of <see cref="GraphNode"/> objects.
		/// </summary>
		public IList<GraphNode> Nodes { get; private set; } = new List<GraphNode>();

		public EvaluationGraph() { }

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			EvaluationGraph clone = (EvaluationGraph)base.Clone();

			clone.Nodes = new List<GraphNode>();
			foreach (var item in this.Nodes)
			{
				clone.Nodes.Add((GraphNode)item.Clone());
			}

			return clone;
		}
	}
}