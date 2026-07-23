using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

[DxfName(DxfFileToken.ObjectBlockRotateAction)]
[DxfSubClass(DxfSubclassMarker.BlockRotationAction)]
public class BlockRotationAction : BlockActionBasePt
{
	public EvalConnection Connection { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockRotateAction;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockRotationAction;
}