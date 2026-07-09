using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates;

internal class CadBlockMoveActionTemplate : CadBlockActionTemplate
{
	public CadBlockMoveActionTemplate()
		: base(new BlockMoveAction())
	{
	}

	public CadBlockMoveActionTemplate(BlockMoveAction moveAction)
		: base(moveAction)
	{
	}
}
