using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKLINEARPARAMETER object, used in AutoCAD to control a
/// distance between two points in a dynamic block.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockLinearParameter"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockLinearParameter"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockLinearParameter)]
[DxfSubClass(DxfSubclassMarker.BlockLinearParameter)]
public class BlockLinearParameter : Block2PtParameter
{
	/// <summary>
	/// Gets or sets the description of the parameter.
	/// </summary>
	[DxfCodeValue(306)]
	public string Description { get; set; }

	/// <summary>
	/// Label text.
	/// </summary>
	[DxfCodeValue(305)]
	public string Label { get; set; }

	/// <summary>
	/// Gets or sets the label offset.
	/// </summary>
	[DxfCodeValue(140)]
	public double LabelOffset { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockLinearParameter;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockLinearParameter;

	public ParameterValueSet ValueSet { get; set; } = new ParameterValueSet();
}
