using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO
{
	public delegate void NotificationEventHandler(object sender, NotificationEventArgs e);

	public enum NotificationType
	{
		NotImplemented = -1,
		None = 0,
		NotSupported = 1,
		Warning = 2,
		Error = 3,
	}

	public class NotificationEventArgs : EventArgs
	{
		public string Message { get; }

		public NotificationType NotificationType { get; }

		public Exception Exception { get; }

		public NotificationEventArgs(string message, NotificationType notificationType = NotificationType.None, Exception exception = null)
		{
			this.Message = message;
			this.NotificationType = notificationType;
			this.Exception = exception;
		}
	}
}
