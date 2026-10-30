using System;

namespace ACadSharp.IO;

/// <summary>
/// Represents the method that handles a notification event raised by an object.
/// </summary>
/// <param name="sender">The source of the event.</param>
/// <param name="e">A NotificationEventArgs object that contains the event data.</param>
public delegate void NotificationEventHandler(object sender, NotificationEventArgs e);

/// <summary>
/// Provides data for notification events, including the message, notification type, and any associated exception.
/// </summary>
/// <remarks>Use this class with event handlers to receive detailed information about notifications, such as
/// informational messages, warnings, or errors. The properties expose the notification message, its type, and an
/// optional exception if the notification is related to an error condition.</remarks>
public class NotificationEventArgs : EventArgs
{
	/// <summary>
	/// Gets the message.
	/// </summary>
	public string Message { get; }

	/// <summary>
	/// Gets the type of notification.
	/// </summary>
	public NotificationType NotificationType { get; }

	/// <summary>
	/// Gets the exception that caused the current operation to fail.
	/// </summary>
	public Exception Exception { get; }

	public NotificationEventArgs(string message, NotificationType notificationType = NotificationType.None, Exception exception = null)
	{
		this.Message = message;
		this.NotificationType = notificationType;
		this.Exception = exception;
	}
}
