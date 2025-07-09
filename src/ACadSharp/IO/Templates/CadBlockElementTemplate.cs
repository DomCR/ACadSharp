using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates {
	internal class CadBlockElementTemplate : CadEvaluationExpressionTemplate
	{
		public BlockElement BlockElement { get { return this.CadObject as BlockElement; } }

		public CadBlockElementTemplate(BlockElement cadObject)
			: base(cadObject)
		{
		}
	}
}