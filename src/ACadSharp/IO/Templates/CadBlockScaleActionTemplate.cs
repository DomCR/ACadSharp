using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates;

internal class CadBlockScaleActionTemplate : CadBlockActionBasePtTemplate
{
	public CadBlockScaleActionTemplate()
		: base(new BlockScaleAction())
	{
	}
	public CadBlockScaleActionTemplate(BlockScaleAction scaleAction)
		: base(scaleAction)
	{
	}
}
