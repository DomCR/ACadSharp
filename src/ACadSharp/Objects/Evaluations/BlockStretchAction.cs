using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

[DxfName(DxfFileToken.ObjectBlockStretchAction)]
[DxfSubClass(DxfSubclassMarker.BlockStretchAction)]
public class BlockStretchAction : BlockAction
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockStretchAction;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockStretchAction;

}