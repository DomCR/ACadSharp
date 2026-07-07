using ACadSharp.Attributes;
using CSMath;
using System.Linq;

namespace ACadSharp.Objects.Evaluations;

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
	/// Gets or sets the grip ids.
	/// </summary>
	/// <remarks>
	/// Size 4 array of grip ids.
	/// </remarks>
	[DxfCollectionCodeValue(91)]
	public long[] GripIds { get; } = new long[4];

	/// <summary>
	/// Gets or sets the properties.
	/// </summary>
	public EvalParameterProperty[] Properties { get; } = Enumerable.Repeat(new EvalParameterProperty(), 4).ToArray();

	/// <summary>
	/// Gets or sets the second point.
	/// </summary>
	[DxfCodeValue(1011, 1021, 1031)]
	public XYZ SecondPoint { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.Block2PtParameter;
}