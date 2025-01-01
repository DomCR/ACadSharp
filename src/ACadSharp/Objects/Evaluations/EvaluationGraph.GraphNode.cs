using System;
using ACadSharp.Attributes;


namespace ACadSharp.Objects.Evaluations
{
	public partial class EvaluationGraph
	{
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
			/// Gets a <see cref="EvaluationExpression"/> associated with this <see cref="GraphNode"/>.
			/// </summary>
			[DxfCodeValue(360)]
			public EvaluationExpression NodeObject { get; internal set; }

			public object Clone()
			{
				GraphNode clone = (GraphNode)MemberwiseClone();

				clone.Next = (GraphNode)Next.Clone();
				clone.NodeObject = (EvaluationExpression)NodeObject.Clone();

				return clone;
			}
		}
	}
}