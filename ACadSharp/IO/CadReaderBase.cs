using ACadSharp.Header;
using CSUtilities.IO;
using System.IO;

namespace ACadSharp.IO
{
	public abstract class CadReaderBase : ICadReader
	{
		protected NotificationEventHandler notificationHandler;

		internal readonly StreamIO _fileStream;

		private CadReaderBase(NotificationEventHandler notification)
		{
			this.notificationHandler = notification;
		}

		protected CadReaderBase(string filename, NotificationEventHandler notification) : this(notification)
		{
			this._fileStream = new StreamIO(filename);
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
