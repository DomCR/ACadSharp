using ACadSharp.Classes;

namespace ACadSharp.IO
{
	public abstract class CadWriterBase : ICadWriter
	{
		public event NotificationEventHandler OnNotification;

		protected CadDocument _document;

		/// <inheritdoc/>
		public abstract void Write();

		/// <inheritdoc/>
		public abstract void Dispose();

		public CadWriterBase(CadDocument document)
		{
			this._document = document;
		}

		protected void validateDocument()
		{
			DxfClassCollection.UpdateDxfClasses(_document);
		}

		protected void triggerNotification(object sender, NotificationEventArgs e)
		{
			this.OnNotification?.Invoke(this, e);
		}
	}
}
