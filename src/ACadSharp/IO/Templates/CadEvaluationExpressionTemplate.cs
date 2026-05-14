using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates {
	internal class CadEvaluationExpressionTemplate : CadTemplate<EvaluationExpression>
	{
		public CadEvaluationExpressionTemplate(EvaluationExpression cadObject)
			: base(cadObject)
		{
		}
	}
}