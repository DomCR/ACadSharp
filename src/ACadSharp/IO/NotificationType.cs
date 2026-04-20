namespace ACadSharp.IO;

/// <summary>
/// Specifies the type of notification or status reported by an operation or component.
/// </summary>
/// <remarks>Use this enumeration to indicate the outcome or status of an operation, such as whether it completed
/// successfully, encountered an error, or is not supported. The values can be used to control application flow or to
/// provide user feedback based on the result of an action.</remarks>
public enum NotificationType
{
	NotImplemented = -1,
	None = 0,
	NotSupported = 1,
	Warning = 2,
	Error = 3,
}
