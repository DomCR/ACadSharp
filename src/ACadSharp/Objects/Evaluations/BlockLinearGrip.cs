using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKLINEARGRIP object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockLinearGrip"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockLinearGrip"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockLinearGrip)]
[DxfSubClass(DxfSubclassMarker.BlockLinearGrip)]
public class BlockLinearGrip : BlockGrip
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockLinearGrip;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockLinearGrip;

	/// <summary>
	/// Gets or sets the distance in the X direction.
	/// </summary>
	[DxfCodeValue(140)]
	public double XDistance { get; set; }

	/// <summary>
	/// Gets or sets the distance in the Y direction.
	/// </summary>
	[DxfCodeValue(141)]
	public double YDistance { get; set; }

	/// <summary>
	/// Gets or sets the distance in the Z direction.
	/// </summary>
	[DxfCodeValue(142)]
	public double ZDistance { get; set; }
}