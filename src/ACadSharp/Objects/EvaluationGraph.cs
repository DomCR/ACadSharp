using System;
using System.Collections.Generic;

using ACadSharp.Attributes;


namespace ACadSharp.Objects
{

	/// <summary>
	/// Represents an evaluation graph containing a list of <see cref="GraphNode"/>
	/// objects.
	/// </summary>
	public class EvaluationGraph : NonGraphicalObject 
	{

		public EvaluationGraph() {}

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectEvalGraph;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.EvalGraph;

		/// <summary>
		/// Gets a list of <see cref="GraphNode"/> objects.
		/// </summary>
		public IList<GraphNode> Nodes { get; } = new List<GraphNode>();


		/// <summary>
		/// Represents a graph node of a <see cref="EvaluationGraph"/>.
		/// </summary>
		public class GraphNode : ICloneable
		{

			/// <summary>
			/// Gets or sets the index of this <see cref="GraphNode"/> in the list of
			/// graph nodes in the owning <see cref="EvaluationGraph"/>.
			/// </summary>
			[DxfCodeValue(91)]
			public int Index { get; set; }

			/// <summary>
			/// Gets or sets the index of the next <see cref="GraphNode"/> in the list of
			/// graph nodes in the owning <see cref="EvaluationGraph"/>.
			/// </summary>
			[DxfCodeValue(95)]
			internal int NextNodeIndex { get; set; }

			/// <summary>
			/// Gets the next <see cref="GraphNode"/> in the list of
			/// graph nodes in the owning <see cref="EvaluationGraph"/>.
			/// </summary>
			public GraphNode Next { get; internal set; }

			/// <summary>
			/// Unknown
			/// </summary>
			[DxfCodeValue(93)]
			public int Flags { get; set; }

			/// <summary>
			/// Unknown
			/// </summary>
			[DxfCodeValue(92)]
			public int Data1 { get; internal set; }

			/// <summary>
			/// Unknown
			/// </summary>
			[DxfCodeValue(92)]
			public int Data2 { get; internal set; }

			/// <summary>
			/// Unknown
			/// </summary>
			[DxfCodeValue(92)]
			public int Data3 { get; internal set; }

			/// <summary>
			/// Unknown
			/// </summary>
			[DxfCodeValue(92)]
			public int Data4 { get; internal set; }

			/// <summary>
			/// Gets a <see cref="CadObject"/> associated with this <see cref="CadObject"/>.
			/// </summary>
			[DxfCodeValue(360)]
			public CadObject NodeObject { get; internal set; }

			
			public object Clone()
			{
				GraphNode clone = (GraphNode)MemberwiseClone();

				clone.Next = (GraphNode)Next.Clone();
				clone.NodeObject = NodeObject.Clone();

				return clone;
			}
		}


		/// <inheritdoc/>
		public override CadObject Clone()
		{
			EvaluationGraph clone = (EvaluationGraph)base.Clone();

			clone.Nodes.Clear();
			foreach (var item in this.Nodes)
			{
				clone.Nodes.Add((GraphNode)item.Clone());
			}

			return clone;
		}
	}
}