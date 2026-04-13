using System;

namespace ACadSharp.Objects;

/// <summary>
/// Layout flags
/// </summary>
[Flags]
public enum LayoutFlags : short
{
	/// <summary>
	/// None
	/// </summary>
	None = 0,
	/// <summary>
	/// Indicates the PSLTSCALE value for this layout when this layout is current
	/// </summary>
	PaperSpaceLinetypeScaling = 1,
	/// <summary>
	/// Indicates the LIMCHECK value for this layout when this layout is current
	/// </summary>
	LimitsChecking = 2,
}
