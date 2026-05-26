namespace ACadSharp.Objects;

[System.Flags]
public enum FieldStateFlags
{
	Unknown = 0,

	Initialized = 1,

	Compiled = 2,

	Modified = 4,

	Evaluated = 8,

	Cached = 16
}
