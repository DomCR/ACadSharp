namespace ACadSharp.Objects;

[System.Flags]
public enum EvaluationStatusFlags
{
	NotEvaluated = 1,

	Success = 2,

	EvaluatorNotFound = 4,

	SyntaxError = 8,

	InvalidCode = 0x10,

	InvalidContext = 0x20,

	OtherError = 0x40
}
