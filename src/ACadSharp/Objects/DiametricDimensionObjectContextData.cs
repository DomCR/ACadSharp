using ACadSharp.Attributes;

namespace ACadSharp.Objects;

/// <summary>
/// Annotation scale representation of a diametric dimension.
/// </summary>
[DxfName(DxfFileToken.DiametricDimensionObjectContextData)]
[DxfSubClass(DxfSubclassMarker.DiametricDimensionObjectContextData)]
public class DiametricDimensionObjectContextData : DimensionObjectContextData
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.DiametricDimensionObjectContextData;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.DiametricDimensionObjectContextData;
}
