using System;

namespace ACadSharp.DataStorage;

[Flags]
internal enum SchemaPropertyFlags
{
	None = 0,
	Unknown1 = 1,
	NoType = 2,
	Unknown2 = 8
}