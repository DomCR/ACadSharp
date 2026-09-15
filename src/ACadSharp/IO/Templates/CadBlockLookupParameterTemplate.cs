using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates
{
	internal class CadBlockLookupParameterTemplate : CadBlock1PtParameterTemplate
	{
		public BlockLookupParameter BlockLookupParameter { get { return this.CadObject as BlockLookupParameter; } }

		public CadBlockLookupParameterTemplate(BlockLookupParameter cadObject)
			: base(cadObject)
		{
		}
	}
}

