using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKFLIPGRIP object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockFlipGrip"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockFlipGrip"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockFlipGrip)]
[DxfSubClass(DxfSubclassMarker.BlockFlipGrip)]
public class BlockFlipGrip : BlockGrip
{
	/// <summary>
	/// Gets or sets the X component of the direction vector.
	/// </summary>
	[DxfCodeValue(140)]
	public double DirectionX { get; set; }

	/// <summary>
	/// Gets or sets the Y component of the direction vector.
	/// </summary>
	[DxfCodeValue(141)]
	public double DirectionY { get; set; }

	/// <summary>
	/// Gets or sets the Z component of the direction vector.
	/// </summary>
	[DxfCodeValue(142)]
	public double DirectionZ { get; set; }

	/// <summary>
	/// Gets or sets the expression id.
	/// </summary>
	[DxfCodeValue(93)]
	public int FlipExpressionId { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockFlipGrip;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockFlipGrip;
}