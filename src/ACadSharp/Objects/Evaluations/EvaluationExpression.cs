using ACadSharp.Attributes;
using static ACadSharp.Objects.Evaluations.EvaluationGraph;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents an evaluation expression used in AutoCAD to define dynamic block parameters and constraints.
/// </summary>
[DxfSubClass(DxfSubclassMarker.EvalGraphExpr)]
public abstract class EvaluationExpression : CadObject
{
	public DxfValuePair EvaluatedValue { get; set; }

	/// <summary>
	/// Gets or sets the unique identifier for the evaluation expression.
	/// </summary>
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

public class DxfValuePair
{
	public DxfCode Code { get; }

	public GroupCodeValueType GroupCodeValueType { get { return GroupCodeValue.TransformValue((int)Code); } }

	public object Value { get; }

	public DxfValuePair(DxfCode code, object value)
	{
		Code = code;
		Value = value;
	}
}