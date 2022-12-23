using ACadSharp.Header;
using CSUtilities.IO;
using System;
using System.IO;

namespace ACadSharp.IO
{
	public abstract class CadReaderBase : ICadReader
	{
		public event NotificationEventHandler OnNotification;

		internal readonly StreamIO _fileStream;

		protected CadReaderBase(NotificationEventHandler notification)
		{
			this.OnNotification += notification;
		}

		protected CadReaderBase(string filename, NotificationEventHandler notification = null) : this(notification)
		{
			this._fileStream = new StreamIO(filename, FileMode.Open, FileAccess.Read);
		}

		protected CadReaderBase(Stream stream, NotificationEventHandler notification = null) : this(notification)
		{
			this._fileStream = new StreamIO(stream);
		}

		/// <inheritdoc/>
		public abstract CadDocument Read();

		/// <inheritdoc/>
		public abstract CadHeader ReadHeader();

		/// <inheritdoc/>
		public virtual void Dispose()
		{
			this._fileStream.Dispose();
		}

		protected void triggerNotification(string message, NotificationType notificationType)
		{
			this.onNotificationEvent(null, new NotificationEventArgs(message, notificationType));
		}

		protected void onNotificationEvent(object sender, NotificationEventArgs e)
		{
			this.OnNotification?.Invoke(this, e);
		}
	}
}
