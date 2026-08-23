using ACadSharp.Classes;

namespace ACadSharp;

/// <summary>
/// Represents a proxy object containing custom data.
/// </summary>
public interface IProxy
{
	/// <summary>
	/// Application object's class ID.
	/// </summary>
	int ClassId { get; }

	/// <summary>
	/// Application object's class.
	/// </summary>
	DxfClass DxfClass { get; set; }

	int MaintenanceVersion { get; set; }

	/// <summary>
	/// Original custom object data format.
	/// </summary>
	bool OriginalDataFormatDxf { get; set; }

	/// <summary>
	/// Proxy object class ID.
	/// </summary>
	int ProxyClassId { get; }

	ACadVersion Version { get; set; }
}
