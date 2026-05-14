using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates;

internal class CadBlockRotationActionTemplate : CadBlockActionBasePtTemplate
{
	public CadBlockRotationActionTemplate() : base(new BlockRotationAction())
	{
	}

	public CadBlockRotationActionTemplate(BlockRotationAction blockAction)
		: base(blockAction)
	{
	}
}