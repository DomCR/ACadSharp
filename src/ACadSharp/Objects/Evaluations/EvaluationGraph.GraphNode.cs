using System;
using ACadSharp.Attributes;


namespace ACadSharp.Objects.Evaluations
{
	public partial class EvaluationGraph
	{
		public class Edge
		{

		}

		/// <summary>
		/// Represents a graph node of a <see cref="EvaluationGraph"/>.
		/// </summary>
		public class Node : ICloneable
		{
			/// <summary>
			/// Gets or sets the index of this <see cref="Node"/> in the list of
			/// graph nodes in the owning <see cref="EvaluationGraph"/>.
			/// </summary>
			[DxfCodeValue(91)]
			public int Index { get; set; }

			/// <summary>
			/// Gets or sets the index of the next <see cref="Node"/> in the list of
			/// graph nodes in the owning <see cref="EvaluationGraph"/>.
			/// </summary>
			[DxfCodeValue(95)]
			internal int NextNodeIndex { get; set; }

			/// <summary>
			/// Gets the next <see cref="Node"/> in the list of
			/// graph nodes in the owning <see cref="EvaluationGraph"/>.
			/// </summary>
			[Obsolete("Next reference may not be needed if the reference is the index.")]
			public Node Next { get; internal set; }

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
			/// Gets a <see cref="EvaluationExpression"/> associated with this <see cref="Node"/>.
			/// </summary>
			[DxfCodeValue(360)]
			public EvaluationExpression Expression { get; internal set; }

			public object Clone()
			{
				Node clone = (Node)MemberwiseClone();

				clone.Next = (Node)Next?.Clone();
				clone.Expression = (EvaluationExpression)Expression?.Clone();

				return clone;
			}
		}
	}
}