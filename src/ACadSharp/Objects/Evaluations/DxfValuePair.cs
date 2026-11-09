namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a pair of values used in evaluation expressions, consisting of a code and its corresponding value.
/// </summary>
public class DxfValuePair	//Same as XRecord.Entry??
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