namespace ACadSharp.Header;

/// <summary>
/// Specifies how plot styles are assigned.
/// </summary>
/// <remarks>Named plot styles are assigned by name; color-dependent plot styles are assigned based on object
/// color.</remarks>
public enum PlotStyleMode : short
{
	/// <summary>
	/// Uses named plot styles.
	/// </summary>
	Named = 0,

	/// <summary>
	/// Plot styles are assigned based on color.
	/// </summary>
	ColorDependent = 1
}