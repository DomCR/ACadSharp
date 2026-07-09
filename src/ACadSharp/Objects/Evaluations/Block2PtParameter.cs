using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents the dynamic block 2 point parameter.
/// </summary>
[DxfSubClass(DxfSubclassMarker.Block2PtParameter)]
public abstract class Block2PtParameter : BlockParameter
{
	/// <summary>
	/// Base point the distance is measured from (start point or middle point).
	/// </summary>
	[DxfCodeValue(177)]
	public LinearParameterBaseLocation BaseLocation { get; set; }

	/// <summary>
	/// Gets or sets the first point.
	/// </summary>
	[DxfCodeValue(1010, 1020, 1030)]
	public XYZ FirstPoint { get; set; }

	/// <summary>
	/// Gets or sets the first point displacement in X direction.
	/// </summary>
	public EvalParameterProperty FirstPointDisplacementX { get; set; } = new EvalParameterProperty();

	/// <summary>
	/// Gets or sets the first point displacement in Y direction.
	/// </summary>
	public EvalParameterProperty FirstPointDisplacementY { get; set; } = new EvalParameterProperty();

	/// <summary>
	/// Gets or sets the grip ids.
	/// </summary>
	/// <remarks>
	/// Size 4 array of grip ids.
	/// </remarks>
	[DxfCollectionCodeValue(91)]
	public long[] GripIds { get; } = new long[4];

	/// <summary>
	/// Gets or sets the second point.
	/// </summary>
	[DxfCodeValue(1011, 1021, 1031)]
	public XYZ SecondPoint { get; set; }

	/// <summary>
	/// Gets or sets the second point displacement in X direction.
	/// </summary>
	public EvalParameterProperty SecondPointDisplacementX { get; set; } = new EvalParameterProperty();

	/// <summary>
	/// Gets or sets the second point displacement in Y direction.
	/// </summary>
	public EvalParameterProperty SecondPointDisplacementY { get; set; } = new EvalParameterProperty();

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.Block2PtParameter;
}