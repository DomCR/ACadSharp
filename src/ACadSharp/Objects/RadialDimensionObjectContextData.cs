using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects;

/// <summary>
/// Annotation scale representation of a radial dimension.
/// </summary>
[DxfName(DxfFileToken.RadialDimensionObjectContextData)]
[DxfSubClass(DxfSubclassMarker.RadialDimensionObjectContextData)]
public class RadialDimensionObjectContextData : DimensionObjectContextData
{
	/// <summary>
	/// Point on the curve where the dimension line ends.
	/// </summary>
	[DxfCodeValue(11, 21, 31)]
	public XYZ FirstArcPoint { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.RadialDimensionObjectContextData;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.RadialDimensionObjectContextData;
}
