using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects;

/// <summary>
/// Annotation scale representation of an angular or arc length dimension.
/// </summary>
[DxfName(DxfFileToken.AngularDimensionObjectContextData)]
[DxfSubClass(DxfSubclassMarker.AngularDimensionObjectContextData)]
public class AngularDimensionObjectContextData : DimensionObjectContextData
{
	/// <summary>
	/// Point on the dimension arc.
	/// </summary>
	[DxfCodeValue(11, 21, 31)]
	public XYZ ArcPoint { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.AngularDimensionObjectContextData;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.AngularDimensionObjectContextData;
}
