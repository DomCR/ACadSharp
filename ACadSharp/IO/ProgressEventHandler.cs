using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO
{
	public delegate void NotificationEventHandler(object sender, NotificationEventArgs e);

	public enum NotificationType
	{
		None,
		NotSuported,
		ObjectNotFound,
	}

	public class NotificationEventArgs : EventArgs
	{
		public string Message { get; }
		public NotificationType NotificationType { get; set; }

		public NotificationEventArgs(string message, NotificationType notificationType = NotificationType.None)
		{
			this.Message = message;
			this.NotificationType = notificationType;
		}
	}
}
