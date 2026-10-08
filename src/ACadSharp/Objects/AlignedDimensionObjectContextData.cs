using ACadSharp.Attributes;

namespace ACadSharp.Objects;

/// <summary>
/// Annotation scale representation of an aligned or linear dimension.
/// </summary>
[DxfName(DxfFileToken.AlignedDimensionObjectContextData)]
[DxfSubClass(DxfSubclassMarker.AlignedDimensionObjectContextData)]
public class AlignedDimensionObjectContextData : DimensionObjectContextData
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.AlignedDimensionObjectContextData;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.AlignedDimensionObjectContextData;
}
