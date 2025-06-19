using CSUtilities.Text;
using System.Text;
using System;
using System.IO;
using ACadSharp.Classes;

namespace ACadSharp.IO
{
	/// <summary>
	/// Base class for the CAD writers.
	/// </summary>
	public abstract class CadWriterBase<T> : ICadWriter
		where T : CadWriterConfiguration, new()
	{
		/// <inheritdoc/>
		public event NotificationEventHandler OnNotification;

		/// <summary>
		/// Configuration for the writer.
		/// </summary>
		public T Configuration { get; set; } = new T();

		protected Stream _stream;

		protected CadDocument _document;

		protected Encoding _encoding;

		protected CadWriterBase(Stream stream, CadDocument document)
		{
			this._stream = stream;
			this._document = document;
		}

		/// <inheritdoc/>
		public virtual void Write()
		{
			this._document.UpdateDxfClasses(this.Configuration.ResetDxfClasses);

			this._encoding = this.getListedEncoding(this._document.Header.CodePage);
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
