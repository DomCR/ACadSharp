using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects;

/// <summary>
/// Annotation scale representation of an aligned or linear dimension.
/// </summary>
[DxfName(DxfFileToken.AlignedDimensionObjectContextData)]
[DxfSubClass(DxfSubclassMarker.AlignedDimensionObjectContextData)]
public class AlignedDimensionObjectContextData : DimensionObjectContextData
{
	/// <summary>
	/// Point on the dimension line.
	/// </summary>
	[DxfCodeValue(11, 21, 31)]
	public XYZ DimensionLinePoint { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.AlignedDimensionObjectContextData;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.AlignedDimensionObjectContextData;
}
