using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects;

/// <summary>
/// Annotation scale representation of a jogged radial dimension.
/// </summary>
[DxfName(DxfFileToken.RadialDimensionLargeObjectContextData)]
[DxfSubClass(DxfSubclassMarker.RadialDimensionLargeObjectContextData)]
public class RadialDimensionLargeObjectContextData : RadialDimensionObjectContextData
{
	/// <summary>
	/// Override center point.
	/// </summary>
	[DxfCodeValue(12, 22, 32)]
	public XYZ OverrideCenter { get; set; }

	/// <summary>
	/// Jog point.
	/// </summary>
	[DxfCodeValue(13, 23, 33)]
	public XYZ JogPoint { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.RadialDimensionLargeObjectContextData;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.RadialDimensionLargeObjectContextData;
}
