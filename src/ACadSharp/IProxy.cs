using ACadSharp.Classes;
using System.IO;

namespace ACadSharp;

/// <summary>
/// Represents a proxy object containing custom data.
/// </summary>
public interface IProxy
{
	/// <summary>
	/// Application object's class ID.
	/// </summary>
	int ClassNumber { get; }

	/// <summary>
	/// Application object's class.
	/// </summary>
	DxfClass DxfClass { get; set; }

	/// <summary>
	/// Gets or sets the maintenance version number.
	/// </summary>
	int MaintenanceVersion { get; set; }

	/// <summary>
	/// Original custom object data format.
	/// </summary>
	bool OriginalDataFormatDxf { get; set; }

	/// <summary>
	/// Proxy object class ID.
	/// </summary>
	int ProxyClassId { get; }

	/// <summary>
	/// Gets or sets the retained data of the original object.
	/// </summary>
	Stream Data { get; set; }

	/// <summary>
	/// Gets or sets the AutoCAD version.
	/// </summary>
	ACadVersion Version { get; set; }
}
