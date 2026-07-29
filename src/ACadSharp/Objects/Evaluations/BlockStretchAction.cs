using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKSTRETCHACTION object, used in AutoCAD to control a
/// stretch action in a dynamic block.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockStretchAction"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockStretchAction"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockStretchAction)]
[DxfSubClass(DxfSubclassMarker.BlockStretchAction)]
public class BlockStretchAction : StretchActionBase
{
	/// <inheritdoc/>
	[DxfCodeValue(141)]
	public override double AngleOffset { get; set; }

	/// <inheritdoc/>
	[DxfCollectionCodeValue(1011, 1021)]
	[DxfCodeValue(DxfReferenceType.Count, 72)]
	public override List<XY> Boundary { get; protected set; } = new();

	/// <inheritdoc/>
	[DxfCodeValue(140)]
	public override double DistanceMultiplier { get; set; }

	public EvalConnection EndXDeltaConnection { get; set; }

	public EvalConnection EndYDeltaConnection { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockStretchAction;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockStretchAction;

	/// <inheritdoc/>
	[DxfCodeValue(280)]
	public byte UnknownFlag { get; set; }
}
