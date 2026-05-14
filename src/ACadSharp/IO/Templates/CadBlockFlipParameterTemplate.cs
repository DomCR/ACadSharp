using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates {
	internal class CadBlockFlipParameterTemplate : CadBlock2PtParameterTemplate
	{
		public Block2PtParameter BlockFlipParameter { get { return this.CadObject as BlockFlipParameter; } }

		public CadBlockFlipParameterTemplate(BlockFlipParameter cadObject)
			: base(cadObject)
		{
		}
	}
}