using ACadSharp.Attributes;

namespace ACadSharp.Objects;

/// <summary>
/// Annotation scale representation of a radial dimension.
/// </summary>
[DxfName(DxfFileToken.RadialDimensionObjectContextData)]
[DxfSubClass(DxfSubclassMarker.RadialDimensionObjectContextData)]
public class RadialDimensionObjectContextData : DimensionObjectContextData
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.RadialDimensionObjectContextData;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.RadialDimensionObjectContextData;
}
