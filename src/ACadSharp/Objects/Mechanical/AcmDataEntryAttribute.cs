namespace ACadSharp.Objects.Mechanical;

/// <summary>
/// A named value stored by an AutoCAD Mechanical data entry.
/// </summary>
public class AcmDataEntryAttribute
{
	public string Name { get; set; } = string.Empty;

	public string Value { get; set; } = string.Empty;

	/// <summary>
	/// Gets the optional third string stored with the name and value.
	/// </summary>
	public string Metadata { get; set; } = string.Empty;
}
