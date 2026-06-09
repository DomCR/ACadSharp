using System;

namespace ACadSharp.Objects;

public partial class TableStyle
{
	[Flags]
	public enum MarginFlags
	{
		None = 0,

		Override = 1,
	}
}