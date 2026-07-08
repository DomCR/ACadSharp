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

	/// <summary>
	/// Gets the list of <see cref="EvalConnection"/> objects that define the connections for this <see cref="BlockActionBasePt"/>.
	/// </summary>
	public EvalConnection[] Connections { get; } = new EvalConnection[2];

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockActionBasePt;

	[DxfCodeValue(1012, 1022, 1032)]
	public XYZ Value1012 { get; set; }

	[DxfCodeValue(280)]
	public bool Value280 { get; set; }
}