using ACadSharp.Classes;
using ACadSharp.Exceptions;
using ACadSharp.Header;
using ACadSharp.IO.DWG;
using CSUtilities.Converters;
using CSUtilities.IO;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ACadSharp.IO
{
	public abstract class CadReaderBase : ICadReader
	{
		public event NotificationEventHandler OnNotificationHandler;

		internal readonly StreamIO _fileStream;

		protected CadReaderBase(NotificationEventHandler notification)
		{
			this.OnNotificationHandler += notification;
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
		public void Dispose()
		{
			this._fileStream.Dispose();
		}

		protected void triggerNotification(object sender, NotificationEventArgs e)
		{
			this.OnNotificationHandler?.Invoke(this, e);
		}
	}
}
