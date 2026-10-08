using ACadSharp.Attributes;
using ACadSharp.Tables;

namespace ACadSharp.Objects;

/// <summary>
/// Annotation scale representation of a dimension: the dimension's geometry block for one annotation scale.
/// </summary>
/// <remarks>
/// Only the block is read, the dimension data fields are not.
/// </remarks>
[DxfSubClass(DxfSubclassMarker.DimensionObjectContextData)]
public abstract class DimensionObjectContextData : AnnotScaleObjectContextData
{
	/// <summary>Anonymous block holding the dimension's graphics at <see cref="AnnotScaleObjectContextData.Scale"/>.</summary>
	public BlockRecord Block { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.DimensionObjectContextData;
}
