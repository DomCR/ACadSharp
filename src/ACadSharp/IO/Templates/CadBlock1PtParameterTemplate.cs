using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates {
	internal class CadBlock1PtParameterTemplate : CadBlockParameterTemplate
	{
		public Block1PtParameter Block1PtParameter { get { return this.CadObject as Block1PtParameter; } }

		public CadBlock1PtParameterTemplate(Block1PtParameter cadObject)
			: base(cadObject)
		{
		}
	}
}