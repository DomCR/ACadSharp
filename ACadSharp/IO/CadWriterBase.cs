namespace ACadSharp.IO
{
	public abstract class CadWriterBase : ICadWriter
	{
		public event NotificationEventHandler OnNotification;

		/// <inheritdoc/>
		public abstract void Dispose();

		protected void triggerNotification(object sender, NotificationEventArgs e)
		{
			this.OnNotification?.Invoke(this, e);
		}
	}
}
