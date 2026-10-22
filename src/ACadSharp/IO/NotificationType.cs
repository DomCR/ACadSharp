namespace ACadSharp.IO;

/// <summary>
/// Defines the type of notification raised during read/write operations.
/// </summary>
public enum NotificationType
{
	/// <summary>
	/// The feature is not yet implemented.
	/// </summary>
	NotImplemented = -1,

	/// <summary>
	/// No notification.
	/// </summary>
	None = 0,

	/// <summary>
	/// The feature is not supported.
	/// </summary>
	NotSupported = 1,

	/// <summary>
	/// A warning occurred during the operation.
	/// </summary>
	Warning = 2,

	/// <summary>
	/// An error occurred during the operation.
	/// </summary>
	Error = 3,
}