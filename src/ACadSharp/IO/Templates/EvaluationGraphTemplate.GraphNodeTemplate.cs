using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates
{
	internal partial class CadEvaluationGraphTemplate
	{
		public class GraphNodeTemplate : ICadTemplate
		{
			public EvaluationGraph.Node Node { get; } = new();

			public ulong? ExpressionHandle { get; set; }

			public void Build(CadDocumentBuilder builder)
			{
				if (builder.TryGetCadObject(this.ExpressionHandle, out EvaluationExpression evExpression))
				{
					this.Node.Expression = evExpression;
				}
				else
				{
					builder.Notify($"Evaluation graph couldn't find the EvaluationExpression with handle {this.ExpressionHandle}", NotificationType.Warning);
				}
			}
		}
	}
}