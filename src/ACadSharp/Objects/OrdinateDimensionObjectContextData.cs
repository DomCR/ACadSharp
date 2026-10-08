using ACadSharp.Attributes;

namespace ACadSharp.Objects;

/// <summary>
/// Annotation scale representation of an ordinate dimension.
/// </summary>
[DxfName(DxfFileToken.OrdinateDimensionObjectContextData)]
[DxfSubClass(DxfSubclassMarker.OrdinateDimensionObjectContextData)]
public class OrdinateDimensionObjectContextData : DimensionObjectContextData
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.OrdinateDimensionObjectContextData;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.OrdinateDimensionObjectContextData;
}
