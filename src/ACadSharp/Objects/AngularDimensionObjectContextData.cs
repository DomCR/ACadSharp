using ACadSharp.Attributes;

namespace ACadSharp.Objects;

/// <summary>
/// Annotation scale representation of an angular dimension.
/// </summary>
[DxfName(DxfFileToken.AngularDimensionObjectContextData)]
[DxfSubClass(DxfSubclassMarker.AngularDimensionObjectContextData)]
public class AngularDimensionObjectContextData : DimensionObjectContextData
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.AngularDimensionObjectContextData;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.AngularDimensionObjectContextData;
}
