using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates
{
	internal class CadBlockGripLocationComponentTemplate : CadEvaluationExpressionTemplate
	{
		public CadBlockGripLocationComponentTemplate() : base(new BlockGripLocationComponent())
		{
		}

		public CadBlockGripLocationComponentTemplate(BlockGripLocationComponent grip) : base(grip)
		{
		}
	}
}