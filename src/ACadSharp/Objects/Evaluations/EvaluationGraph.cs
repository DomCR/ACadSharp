using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Objects.Evaluations
{
	/// <summary>
	/// Represents an evaluation graph containing a list of <see cref="Node"/>
	/// objects.
	/// </summary>
	[DxfName(DxfFileToken.ObjectEvalGraph)]
	[DxfSubClass(DxfSubclassMarker.EvalGraph)]
	public partial class EvaluationGraph : NonGraphicalObject
	{
		/// <summary>
		/// Dictionary entry name for the object <see cref="EvaluationGraph"/>
		/// </summary>
		public const string DictionaryEntryName = "ACAD_ENHANCEDBLOCK";

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
		/// Gets a list of <see cref="Node"/> objects.
		/// </summary>
		public IList<Node> Nodes { get; private set; } = new List<Node>();

		public IList<Edge> Edges { get; private set; } = new List<Edge>();

		public EvaluationGraph() { }

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			EvaluationGraph clone = (EvaluationGraph)base.Clone();

			clone.Nodes = new List<Node>();
			foreach (var item in this.Nodes)
			{
				clone.Nodes.Add((Node)item.Clone());
			}

			return clone;
		}
	}
}