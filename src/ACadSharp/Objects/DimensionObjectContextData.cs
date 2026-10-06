using ACadSharp.Tables;

namespace ACadSharp.Objects;

/// <summary>
/// Annotation scale representation of a dimension (ACDB_*DIMOBJECTCONTEXTDATA_CLASS): the dimension's
/// geometry block for one annotation scale. Only the block is read.
/// </summary>
public class DimensionObjectContextData : AnnotScaleObjectContextData
{
	/// <summary>Anonymous block holding the dimension's graphics at <see cref="AnnotScaleObjectContextData.Scale"/>.</summary>
	public BlockRecord Block { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => "ACDB_DIMOBJECTCONTEXTDATA_CLASS";
}
