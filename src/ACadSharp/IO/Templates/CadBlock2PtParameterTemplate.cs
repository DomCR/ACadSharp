using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates {
	internal class CadBlock2PtParameterTemplate : CadBlockParameterTemplate
	{
		public Block2PtParameter Block2PtParameter { get { return this.CadObject as Block2PtParameter; } }

		public CadBlock2PtParameterTemplate(Block2PtParameter cadObject)
			: base(cadObject)
		{
		}
	}
}