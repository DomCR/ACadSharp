namespace ACadSharp.IO;

/// <summary>
/// Represents the file format of a CAD file.
/// </summary>
public enum CadFileFormat
{
	/// <summary>
	/// Represents an unknown or unspecified value.
	/// </summary>
	Unknown,

	/// <summary>
	/// Represents the DWG file format, commonly used for storing two and three-dimensional design data and metadata.
	/// </summary>
	DWG,

	/// <summary>
	/// Represents a Drawing Exchange Format (DXF) entity or utility for working with DXF files.
	/// </summary>
	DXF
}