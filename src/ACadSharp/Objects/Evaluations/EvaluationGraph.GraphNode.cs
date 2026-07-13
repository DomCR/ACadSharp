using System;
using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

public partial class EvaluationGraph
{
	/// <summary>
	/// Represents a graph node of a <see cref="EvaluationGraph"/>.
	/// </summary>
	public class Node
	{
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
		/// Gets a <see cref="EvaluationExpression"/> associated with this <see cref="Node"/>.
		/// </summary>
		[DxfCodeValue(360)]
		public EvaluationExpression Expression { get; set; }

		/// <summary>
		/// Unknown
		/// </summary>
		[DxfCodeValue(93)]
		public NodeFlags Flags { get; set; }

		/// <summary>
		/// Gets or sets the index of the next <see cref="Node"/> in the list of
		/// graph nodes in the owning <see cref="EvaluationGraph"/>.
		/// </summary>
		[DxfCodeValue(95)]
		public int Id { get; set; }

		/// <summary>
		/// Gets or sets the index of this <see cref="Node"/> in the list of
		/// graph nodes in the owning <see cref="EvaluationGraph"/>.
		/// </summary>
		[DxfCodeValue(91)]
		public int Index { get; set; }

		private EvaluationExpression _expression;

		/// <summary>
		/// Creates a deep copy of this <see cref="Node"/>.
		/// </summary>
		/// <returns>A deep copy of this <see cref="Node"/>.</returns>
		public Node Clone()
		{
			Node clone = (Node)this.MemberwiseClone();

			clone.Expression = (EvaluationExpression)this.Expression?.Clone();

			return clone;
		}
	}
}