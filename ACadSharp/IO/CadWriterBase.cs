using CSUtilities.Text;
using System.Text;
using System;
using System.IO;

namespace ACadSharp.IO
{
	public abstract class CadWriterBase : ICadWriter
	{
		public event NotificationEventHandler OnNotification;

		/// <summary>
		/// Notifies the writer to close the stream once the operation is completed
		/// </summary>
		/// <value>
		/// true
		/// </value>
		public bool CloseStream { get; set; } = true;

		protected Stream _stream;

		protected CadDocument _document;

		protected CadWriterBase(Stream stream, CadDocument document)
		{
			this._stream = stream;
			this._document = document;
		}

		/// <inheritdoc/>
		public abstract void Write();

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

		protected void validateDocument()
		{
			//TODO: Implement the document validation to check the structure
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
