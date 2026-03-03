using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
///
/// </summary>
[DxfSubClass(DxfSubclassMarker.EvalGraphExpr)]
public abstract class EvaluationExpression : CadObject
{
	[DxfCodeValue(90)]
	public int Id { get; set; }

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.UNLISTED;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.EvalGraphExpr;

	[DxfCodeValue(98)]
	public int Value98 { get; set; }

	[DxfCodeValue(99)]
	public int Value99 { get; set; }
}