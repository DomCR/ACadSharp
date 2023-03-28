namespace ACadSharp.IO
{
	public abstract class CadWriterBase : ICadWriter
	{
		public event NotificationEventHandler OnNotification;

		/// <inheritdoc/>
		public abstract void Write();

		/// <inheritdoc/>
		public abstract void Dispose();

		protected void validateDocument()
		{
			//TODO: Implement the document validation to check the structure
		}

		protected void triggerNotification(object sender, NotificationEventArgs e)
		{
			this.OnNotification?.Invoke(this, e);
		}
	}
}
