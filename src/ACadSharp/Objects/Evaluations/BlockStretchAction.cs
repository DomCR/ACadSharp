using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Objects.Evaluations;

[DxfName(DxfFileToken.ObjectBlockStretchAction)]
[DxfSubClass(DxfSubclassMarker.BlockStretchAction)]
public partial class BlockStretchAction : BlockAction
{
	/// <summary>
	/// Gets or sets the angle offset for the move action.
	/// </summary>
	[DxfCodeValue(141)]
	public double AngleOffset { get; set; }

	[DxfCollectionCodeValue(1011, 1021)]
	[DxfCodeValue(DxfReferenceType.Count, 72)]
	public List<XY> Boundary { get; private set; } = new();

	/// <summary>
	/// Gets or sets the distance multiplier for the move action.
	/// </summary>
	[DxfCodeValue(140)]
	public double DistanceMultiplier { get; set; }

	public EvalConnection EndXDelta { get; set; }

	public EvalConnection EndYDelta { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockStretchAction;

	public List<StretchBind> StretchBindings { get; private set; } = new();

	public List<StretchNode> StretchNodes { get; private set; } = new();

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockStretchAction;

	/// <summary>
	/// Gets or sets an unknown flag value.
	/// </summary>
	[DxfCodeValue(280)]
	public byte UnknownFlag { get; set; }
}