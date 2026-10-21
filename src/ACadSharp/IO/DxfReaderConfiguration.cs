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

		/// <summary>
		/// [PATCH] Intern (share by reference) the string values of XData records (group codes
		/// 1000/1001) while reading. Strings are immutable, so sharing is semantically invisible.
		/// For drawings where XData values repeat heavily (GIS attribute exports: the same keys /
		/// coded values / layer names appear millions of times), this collapses ~100M string
		/// objects into a few million shared instances, cutting XData memory by several GB.
		/// Default true. Call <see cref="DxfXDataInterning.Clear"/> after reading to release
		/// the intern table itself.
		/// </summary>
		public bool InternXDataStrings { get; set; } = true;

		/// <summary>
		/// [PATCH] Trim the working set (light gen0+gen1 GC + EmptyWorkingSet) every N entities
		/// while reading the ENTITIES / BLOCKS sections. Reading a huge DXF allocates a lot of
		/// transient garbage (templates, maps, parsed strings); without periodic trims the
		/// working set grows far beyond the live object graph (measured: ~24GB WS vs ~12.5GB
		/// live for a 1.9M-entity file with XData). A light trim every 100K entities keeps the
		/// working set close to the live size at a modest time cost. 0 disables periodic trims
		/// (upstream behavior).
		/// </summary>
		public int GCEveryNEntities { get; set; } = 100000;
	}
}
