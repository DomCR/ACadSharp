using ACadSharp.Header;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace ACadSharp.IO
{
	/// <summary>
	/// Common interface for the different Cad readers.
	/// </summary>
	public interface ICadReader : IDisposable
	{
		/// <summary>
		/// Notification event to get information about the reading process.
		/// </summary>
		/// <remarks>
		/// The notification system informs about any issue or non critical errors during the reading.
		/// </remarks>
		event NotificationEventHandler OnNotification;

		/// <summary>
		/// Read the Cad header section of the file.
		/// </summary>
		CadHeader ReadHeader();

		/// <summary>
		/// Read a cad document.
		/// </summary>
		CadDocument Read();

		/// <summary>
		/// Read a cad document asynchronously.
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<CadDocument> ReadAsync(CancellationToken cancellationToken = default);
	}
}
