using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

[DxfName(DxfFileToken.ObjectBlockScaleAction)]
[DxfSubClass(DxfSubclassMarker.BlockScaleAction)]
public class BlockScaleAction : BlockActionBasePt
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockScaleAction;

	public EvalConnection ScaleConnection { get; set; } = new EvalConnection();

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockScaleAction;

	public EvalConnection XScaleConnection { get; set; } = new EvalConnection();

	public EvalConnection YScaleConnection { get; set; } = new EvalConnection();
}