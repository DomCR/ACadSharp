using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKFLIPACTION object, used in AutoCAD to control a
/// flip action in a dynamic block.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockFlipAction"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockFlipAction"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockFlipAction)]
[DxfSubClass(DxfSubclassMarker.BlockFlipAction)]
public class BlockFlipAction : BlockAction
{
	public EvalConnection Flip { get; set; }

	public EvalConnection UpdatedFlip { get; set; }

	public EvalConnection UpdatedBase { get; set; }

	public EvalConnection UpdatedEnd { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockFlipAction;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockFlipAction;
}
