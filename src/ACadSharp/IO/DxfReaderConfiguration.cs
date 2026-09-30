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

		/// <summary>
		/// [PATCH] Whether to parse application-defined (XData) group codes (1000-1199) into
		/// <see cref="ACadSharp.XData.ExtendedData"/> records. Default true (upstream behavior).
		/// Set false to skip XData entirely: the reader still advances past the group codes
		/// (keeping the stream in sync) but does not build any records, which drastically cuts
		/// memory for drawings with heavy XData (e.g. millions of attribute records).
		/// </summary>
		public bool ReadXData { get; set; } = true;
	}
}
