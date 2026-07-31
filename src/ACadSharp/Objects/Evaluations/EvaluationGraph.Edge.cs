using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

public partial class EvaluationGraph
{
	public class Edge
	{
		[DxfCodeValue(92)]
		public int Data1 { get; set; }

		[DxfCodeValue(92)]
		public int Data2 { get; set; }

		[DxfCodeValue(92)]
		public int Data3 { get; set; }

		[DxfCodeValue(92)]
		public int Data4 { get; set; }

		[DxfCodeValue(92)]
		public int Data5 { get; set; }

		[DxfCodeValue(93)]
		public int Flags { get; set; }

		[DxfCodeValue(91)]
		public int FromNodeIndex { get; set; }

		[DxfCodeValue(92)]
		public int Index { get; set; }

		[DxfCodeValue(91)]
		public int ToNodeIndex { get; set; }

		[DxfCodeValue(94)]
		public int TrackedCount { get; set; }

		public Edge Clone()
		{
			return (Edge)MemberwiseClone();
		}
	}
}