using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

[DxfName(DxfFileToken.ObjectBlockMoveAction)]
[DxfSubClass(DxfSubclassMarker.BlockMoveAction)]
public class BlockMoveAction : BlockAction
{
	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockMoveAction;
}