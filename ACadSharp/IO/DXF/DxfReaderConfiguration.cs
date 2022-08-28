namespace ACadSharp.IO
{
	public class DxfReaderConfiguration
	{
		/// <summary>
		/// Clears the cache after the reading
		/// </summary>
		public bool ClearChache { get; set; } = true;

		/// <summary>
		/// The reader will try to continue when an exception is found, unless this setting is true
		/// </summary>
		public bool StopAtExceptions { get; set; } = false;

	}
}
