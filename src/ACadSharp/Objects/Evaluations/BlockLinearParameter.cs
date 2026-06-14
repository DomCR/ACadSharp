using ACadSharp.Attributes;
using System.Collections.Generic;

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
	[DxfCodeValue(306)]
	public string Description { get; set; }

	/// <summary>
	/// Distance increment between two allowed values.
	/// </summary>
	[DxfCodeValue(143)]
	public double Increment { get; set; }

	/// <summary>
	/// Label text.
	/// </summary>
	[DxfCodeValue(305)]
	public string Label { get; set; }

	[DxfCodeValue(140)]
	public double LabelOffset { get; set; }

	/// <summary>
	/// Maximum allowed distance.
	/// </summary>
	[DxfCodeValue(142)]
	public double Maximum { get; set; }

	/// <summary>
	/// Minimum allowed distance.
	/// </summary>
	[DxfCodeValue(141)]
	public double Minimum { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockLinearParameter;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.UNLISTED;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockLinearParameter;

	/// <summary>
	/// Discrete list of allowed distance values.
	/// </summary>
	public List<double> Values { get; } = new List<double>();
}