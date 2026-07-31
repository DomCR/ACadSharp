using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKROTATIONPARAMETER object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockRotationParameter"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockRotationParameter"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockRotationParameter)]
[DxfSubClass(DxfSubclassMarker.BlockRotationParameter)]
public class BlockRotationParameter : Block2PtParameter
{
	[DxfCodeValue(306)]
	public string Description { get; set; }

	[DxfCodeValue(305)]
	public string Label { get; set; }

	[DxfCodeValue(140)]
	public double LabelOffset { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockRotationParameter;

	[DxfCodeValue(1011, 1021, 1031)]
	public XYZ Point { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockRotationParameter;

	public ParameterValueSet ValueSet { get; set; }
}