using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects;

/// <summary>
/// Annotation scale representation of a diametric dimension.
/// </summary>
[DxfName(DxfFileToken.DiametricDimensionObjectContextData)]
[DxfSubClass(DxfSubclassMarker.DiametricDimensionObjectContextData)]
public class DiametricDimensionObjectContextData : DimensionObjectContextData
{
	/// <summary>
	/// Point on the curve where the dimension line starts.
	/// </summary>
	[DxfCodeValue(11, 21, 31)]
	public XYZ FirstArcPoint { get; set; }

	/// <summary>
	/// Point on the curve opposite to <see cref="FirstArcPoint"/>.
	/// </summary>
	[DxfCodeValue(12, 22, 32)]
	public XYZ DefinitionPoint { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.DiametricDimensionObjectContextData;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.DiametricDimensionObjectContextData;
}
