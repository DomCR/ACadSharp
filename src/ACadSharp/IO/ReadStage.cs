namespace ACadSharp.IO;

/// <summary>
/// Represents the stage of a read operation.
/// </summary>
public enum ReadStage
{
	/// <summary>
	/// The initial reading stage where raw data is parsed from the file.
	/// </summary>
	Read,

	/// <summary>
	/// The building stage where parsed data is assembled into objects.
	/// </summary>
	Build
}