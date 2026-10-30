using ACadSharp.Header;
using System;

namespace ACadSharp.IO;

/// <summary>
/// Common interface for the different Cad readers.
/// </summary>
public interface ICadReader : IDisposable
{
	/// <summary>
	/// Notification event to get information about the reading process.
	/// </summary>
	/// <remarks>
	/// The notification system informs about any issue or non critical errors during the reading.
	/// </remarks>
	event NotificationEventHandler OnNotification;

	/// <summary>
	/// Occurs when progress is reported during a long-running operation.
	/// </summary>
	/// <remarks>
	/// Subscribe to this event to receive updates about the progress of the associated operation.
	/// </remarks>
	event ProgressEventHandler OnProgress;

	/// <summary>
	/// Read the cad document.
	/// </summary>
	CadDocument Read();

	/// <summary>
	/// Read the Cad header section of the file.
	/// </summary>
	CadHeader ReadHeader();
}