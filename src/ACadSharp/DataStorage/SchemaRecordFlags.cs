using System;

namespace ACadSharp.DataStorage;

[Flags]
public enum SchemaRecordFlags
{
	None = 0,

	Unknown0 = 1,

	NoType = 2,

	Unknown1 = 8,
}