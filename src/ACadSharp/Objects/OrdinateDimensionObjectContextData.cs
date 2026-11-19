using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects;

/// <summary>
/// Annotation scale representation of an ordinate dimension.
/// </summary>
[DxfName(DxfFileToken.OrdinateDimensionObjectContextData)]
[DxfSubClass(DxfSubclassMarker.OrdinateDimensionObjectContextData)]
public class OrdinateDimensionObjectContextData : DimensionObjectContextData
{
	/// <summary>
	/// Definition point.
	/// </summary>
	[DxfCodeValue(11, 21, 31)]
	public XYZ DefinitionPoint { get; set; }

	/// <summary>
	/// End point of the leader.
	/// </summary>
	[DxfCodeValue(12, 22, 32)]
	public XYZ LeaderEndpoint { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.OrdinateDimensionObjectContextData;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.OrdinateDimensionObjectContextData;
}
