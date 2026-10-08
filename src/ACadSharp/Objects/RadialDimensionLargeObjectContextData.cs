using ACadSharp.Attributes;

namespace ACadSharp.Objects;

/// <summary>
/// Annotation scale representation of a jogged radial dimension.
/// </summary>
[DxfName(DxfFileToken.RadialDimensionLargeObjectContextData)]
[DxfSubClass(DxfSubclassMarker.RadialDimensionLargeObjectContextData)]
public class RadialDimensionLargeObjectContextData : DimensionObjectContextData
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.RadialDimensionLargeObjectContextData;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.RadialDimensionLargeObjectContextData;
}
