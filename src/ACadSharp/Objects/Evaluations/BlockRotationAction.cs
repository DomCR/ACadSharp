using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

[DxfName(DxfFileToken.ObjectBlockRotateAction)]
[DxfSubClass(DxfSubclassMarker.BlockRotationAction)]
public class BlockRotationAction : BlockActionBasePt
{
	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockRotationAction;

	[DxfCodeValue(303)]
	public string Value303 { get; set; }

	[DxfCodeValue(94)]
	public int Value94 { get; set; }
}