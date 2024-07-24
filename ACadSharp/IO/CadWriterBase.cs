using CSUtilities.Text;
using System.Text;
using System;
using System.IO;
using ACadSharp.Classes;

namespace ACadSharp.IO
{
	/// <summary>
	/// Configuration for the <see cref="CadWriterBase{T}"/> class.
	/// </summary>
	public class CadWriterConfiguration
	{
		/// <summary>
		/// The writer will close the stream once the operation is completed.
		/// </summary>
		/// <value>
		/// default: true
		/// </value>
		public bool CloseStream { get; set; } = true;

		/// <summary>
		/// Will not ignore the <see cref="ACadSharp.Objects.XRecord"/> objects in the document.
		/// </summary>
		/// <remarks>
		/// Due the complexity of XRecords, if this flag is set to true, it may cause a corruption of the file if the records have been modified manually.
		/// </remarks>
		/// <value>
		/// default: false
		/// </value>
		public bool WriteXRecords { get; set; } = false;
	}

	/// <summary>
	/// Base class for the CAD writers.
	/// </summary>
	public abstract class CadWriterBase<T> : ICadWriter
		where T : CadWriterConfiguration, new()
	{
		/// <summary>
		/// Notification event to get information about the writing process.
		/// </summary>
		/// <remarks>
		/// The notification system informs about any issue or non critical errors during the writing.
		/// </remarks>
		public event NotificationEventHandler OnNotification;

		/// <summary>
		/// Configuration for the writer.
		/// </summary>
		public T Configuration { get; set; } = new T();

		protected Stream _stream;

		protected CadDocument _document;

		protected CadWriterBase(Stream stream, CadDocument document)
		{
			this._stream = stream;
			this._document = document;
		}

		/// <inheritdoc/>
		public virtual void Write()
		{
			DxfClassCollection.UpdateDxfClasses(_document);
		}

		/// <inheritdoc/>
		public abstract void Dispose();

		protected Encoding getListedEncoding(string codePage)
		{
			CodePage code = CadUtils.GetCodePage(codePage);

			try
			{
#if !NET48
				Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif
				return Encoding.GetEncoding((int)code);
			}
			catch (Exception ex)
			{
				this.triggerNotification($"Encoding with code {code} not found, using Windows-1252 as default", NotificationType.Warning, ex);
			}

			return TextEncoding.Windows1252();
		}

		protected void triggerNotification(string message, NotificationType notificationType, Exception ex = null)
		{
			this.triggerNotification(this, new NotificationEventArgs(message, notificationType, ex));
		}

		protected void triggerNotification(object sender, NotificationEventArgs e)
		{
			this.OnNotification?.Invoke(sender, e);
		}
	}
}
