namespace ACadSharp.Objects.Evaluations;

public partial class EvaluationGraph
{
	public class Edge
	{
		public int Data1 { get; set; }

		public int Data2 { get; set; }

		public int Data3 { get; set; }

		public int Data4 { get; set; }

		public int Data5 { get; set; }

		public int Flags { get; set; }

		public int FromNodeIndex { get; set; }

		public int Id { get; set; }

		public int ToNodeIndex { get; set; }

		public int TrackedCount { get; set; }

		public Edge Clone()
		{
			return (Edge)MemberwiseClone();
		}
	}
}