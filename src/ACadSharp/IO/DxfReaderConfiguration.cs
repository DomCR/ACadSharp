namespace ACadSharp.IO
{
	/// <summary>
	/// Configuration for reading DXF files.
	/// </summary>
	public class DxfReaderConfiguration : CadReaderConfiguration
	{
		/// <summary>
		/// Clears the cache after the reading
		/// </summary>
		public bool ClearCache { get; set; } = true;

		/// <summary>
		/// Create the defaults at the end of the reading operation.
		/// </summary>
		public bool CreateDefaults { get; set; } = false;
	}
}
