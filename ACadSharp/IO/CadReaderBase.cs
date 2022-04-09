using ACadSharp.Header;
using CSUtilities.IO;
using System.IO;

namespace ACadSharp.IO
{
	public abstract class CadReaderBase : ICadReader
	{
		public NotificationEventHandler OnNotificationHandler;

		internal readonly StreamIO _fileStream;

		private CadReaderBase(NotificationEventHandler notification)
		{
			this.OnNotificationHandler = notification;
		}

		protected CadReaderBase(string filename, NotificationEventHandler notification) : this(notification)
		{
			this._fileStream = new StreamIO(filename, FileMode.Open, FileAccess.Read);
		}

		protected CadReaderBase(Stream stream, NotificationEventHandler notification) : this(notification)
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
	}
}
