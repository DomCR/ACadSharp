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
