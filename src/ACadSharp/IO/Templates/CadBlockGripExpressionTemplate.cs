using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates
{
	internal class CadBlockGripExpressionTemplate : CadEvaluationExpressionTemplate
	{
		public CadBlockGripExpressionTemplate() : base(new BlockGripExpression())
		{
		}

		public CadBlockGripExpressionTemplate(BlockGripExpression grip) : base(grip)
		{
		}
	}
}