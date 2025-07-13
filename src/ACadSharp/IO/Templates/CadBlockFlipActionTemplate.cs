using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates {
	internal class CadBlockFlipActionTemplate : CadBlockActionTemplate
	{
		public BlockFlipAction BlockFlipAction { get { return this.CadObject as BlockFlipAction; } }

		public CadBlockFlipActionTemplate(BlockFlipAction cadObject)
			: base(cadObject)
		{
		}
	}
}