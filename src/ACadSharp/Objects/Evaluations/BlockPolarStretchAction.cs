using ACadSharp.Attributes;
using ACadSharp.Entities;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKPOLARSTRETCHACTION object, used in AutoCAD to control a
/// polar stretch action in a dynamic block.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockPolarStretchAction"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockPolarStretchAction"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockPolarStretchAction)]
[DxfSubClass(DxfSubclassMarker.BlockPolarStretchAction)]
public class BlockPolarStretchAction : StretchActionBase
{
	/// <inheritdoc/>
	[DxfCodeValue(140)]
	public override double AngleOffset { get; set; }

	public EvalConnection BaseConnection { get; set; }

	public EvalConnection BaseXDeltaConnection { get; set; }

	public EvalConnection BaseYDeltaConnection { get; set; }

	/// <inheritdoc/>
	[DxfCollectionCodeValue(1011, 1021)]
	[DxfCodeValue(DxfReferenceType.Count, 73)]
	public override List<XY> Boundary { get; protected set; } = new List<XY>();

	/// <inheritdoc/>
	[DxfCodeValue(141)]
	public override double DistanceMultiplier { get; set; }

	public EvalConnection EndConnection { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockPolarStretchAction;

	public List<Entity> RotateBindings { get; private set; } = new List<Entity>();

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockPolarStretchAction;

	public EvalConnection UpdatedBaseConnection { get; set; }

	public EvalConnection UpdatedEndConnection { get; set; }
}
