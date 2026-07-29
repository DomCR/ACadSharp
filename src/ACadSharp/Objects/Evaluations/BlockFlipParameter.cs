using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKFLIPPARAMETER object, used in AutoCAD to control a
/// flip in a dynamic block.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockFlipParameter"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockFlipParameter"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockFlipParameter)]
[DxfSubClass(DxfSubclassMarker.BlockFlipParameter)]
public class BlockFlipParameter : Block2PtParameter
{
	/// <summary>
	/// Gets or sets the base state name.
	/// </summary>
	[DxfCodeValue(307)]
	public string BaseStateName { get; set; }

	/// <summary>
	/// Gets or sets the connection.
	/// </summary>
	public EvalConnection Connection { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	[DxfCodeValue(306)]
	public string Description { get; set; }

	/// <summary>
	/// Gets or sets the flipped state name.
	/// </summary>
	[DxfCodeValue(308)]
	public string FlippedStateName { get; set; }

	/// <summary>
	/// Gets or sets the label.
	/// </summary>
	[DxfCodeValue(305)]
	public string Label { get; set; }

	/// <summary>
	/// Gets or sets the label position.
	/// </summary>
	[DxfCodeValue(1012, 1022, 1032)]
	public XYZ LabelPosition { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockFlipParameter;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockFlipParameter;
}
