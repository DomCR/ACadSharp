using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects.Evaluations;

[DxfSubClass(DxfSubclassMarker.BlockActionBasePt)]
public abstract class BlockActionBasePt : BlockAction
{
	/// <summary>
	/// Gets or sets the base point of the action.
	/// </summary>
	[DxfCodeValue(1011, 1021, 1031)]
	public XYZ BasePoint { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockActionBasePt;

	public EvalConnection UpdateBaseX { get; set; } = new EvalConnection();

	public EvalConnection UpdateBaseY { get; set; } = new EvalConnection();

	[DxfCodeValue(1012, 1022, 1032)]
	public XYZ Value1012 { get; set; }

	[DxfCodeValue(280)]
	public bool Value280 { get; set; }
}