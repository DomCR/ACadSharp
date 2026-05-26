using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates
{
	internal partial class CadBlockRotationParameterTemplate : CadBlock2PtParameterTemplate
	{
		public CadBlockRotationParameterTemplate() : base(new BlockRotationParameter())
		{
		}

		public CadBlockRotationParameterTemplate(BlockRotationParameter cadObject)
			: base(cadObject)
		{
		}
	}
}