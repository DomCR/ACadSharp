using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates;

internal class CadBlockRotationParameterTemplate : CadBlock2PtParameterTemplate
{
	public CadBlockRotationParameterTemplate() : base(new BlockRotationParameter())
	{
	}

	public CadBlockRotationParameterTemplate(BlockRotationParameter cadObject)
		: base(cadObject)
	{
	}
}