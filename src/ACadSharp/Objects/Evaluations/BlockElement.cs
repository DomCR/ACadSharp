using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

[DxfSubClass(DxfSubclassMarker.BlockElement)]
public abstract class BlockElement : EvaluationExpression
{
	/// <summary>
	/// Block element name.
	/// </summary>
	[DxfCodeValue(300)]
	public string ElementName { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockElement;

	[DxfCodeValue(1071)]
	public int Value1071 { get; set; }

	[DxfCodeValue(98)]
	public override int Value98 { get; set; }

	[DxfCodeValue(99)]
	public override int Value99 { get; set; }
}