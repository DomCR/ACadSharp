using System;

namespace ACadSharp.IO;

/// <summary>
/// Interface for writing a <see cref="CadDocument"/> into a stream.
/// </summary>
public interface ICadWriter : IDisposable
{
	/// <summary>
	/// Notification event to get information about the writing process.
	/// </summary>
	/// <remarks>
	/// The notification system informs about any issue or non critical errors during the writing.
	/// </remarks>
	event NotificationEventHandler OnNotification;

	/// <summary>
	/// Write the <see cref="CadDocument"/> into the stream.
	/// </summary>
	void Write();
}