using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates {
	internal class CadBlockParameterTemplate : CadBlockElementTemplate
	{
		public BlockParameter BlockParameter { get { return this.CadObject as BlockParameter; } }

		public CadBlockParameterTemplate(BlockParameter cadObject)
			: base(cadObject)
		{
		}
	}
}