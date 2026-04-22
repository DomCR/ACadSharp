using System;
using System.IO;

namespace ACadSharp.IO;

/// <summary>
/// Provides factory methods for creating CAD file readers based on file format.
/// </summary>
/// <remarks>Use this class to obtain an appropriate ICadReader instance for supported CAD file types such as DWG
/// and DXF. The factory selects the correct reader implementation based on the file extension. This class does not
/// validate file existence or content; it only inspects the file extension.</remarks>
public static class CadReaderFactory
{
	/// <summary>
	/// Creates an <see cref="ICadReader"/> instance for the specified CAD file format.
	/// </summary>
	/// <remarks>Supported file formats include DWG and DXF. The method selects the appropriate reader based on the
	/// file extension.</remarks>
	/// <param name="filename">The path to the CAD file to read. The file extension determines the reader type. Must not be null or empty.</param>
	/// <param name="notification">An optional event handler for receiving notifications during the reading process. Can be null if no notifications
	/// are needed.</param>
	/// <returns>An <see cref="ICadReader"/> instance capable of reading the specified CAD file.</returns>
	/// <exception cref="NotSupportedException">Thrown if the file extension is not supported.</exception>
	public static ICadReader CreateReader(string filename, NotificationEventHandler notification = null)
	{
		switch (GetFileFormat(filename))
		{
			case CadFileFormat.DWG:
				return new DwgReader(filename, notification);
			case CadFileFormat.DXF:
				return new DxfReader(filename, notification);
			case CadFileFormat.Unknown:
			default:
				throw new NotSupportedException($"Extension {Path.GetExtension(filename)} is not supported.");
		}
	}

	/// <summary>
	/// Determines the CAD file format based on the file extension of the specified filename.
	/// </summary>
	/// <remarks>The method does not validate whether the file exists or whether the file content matches the
	/// extension. Only the file extension is considered when determining the format.</remarks>
	/// <param name="filename">The path or name of the file whose format is to be identified. The file extension is used to determine the format.</param>
	/// <returns>A value of the CadFileFormat enumeration that corresponds to the file's extension. Returns  <see cref="CadFileFormat.DWG"/> for
	/// ".dwg" files,  <see cref="CadFileFormat.DXF"/> for ".dxf" files, or <see cref="CadFileFormat.Unknown"/> if the extension is not recognized.</returns>
	public static CadFileFormat GetFileFormat(string filename)
	{
		switch (Path.GetExtension(filename))
		{
			case ".dwg":
				return CadFileFormat.DWG;
			case ".dxf":
				return CadFileFormat.DXF;
			default:
				return CadFileFormat.Unknown;
		}
	}
}
