using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates;

internal partial class CadBlockLinearParameterTemplate : CadBlock2PtParameterTemplate
{
	public CadBlockLinearParameterTemplate() : base(new BlockLinearParameter())
	{
	}

	public CadBlockLinearParameterTemplate(BlockLinearParameter parameter)
		: base(parameter)
	{
	}
}
