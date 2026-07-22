using ACadSharp.Prototype1b.Segments;

namespace ACadSharp.Prototype1b
{
	public class DataStoragePointers
	{
		// Indices for reading Segments, Schemas and Data
		public SegmentIndex SegmentIndex { get; set; }
		public DataIndex DataIndex { get; set; }
		public SchemaIndex SchemaIndex { get; set; }

		// Empty areas inside the file which are filled with zeros for the most part (could be commented out
		// sections as sometimes it contains invalid segment data that should be ignored)
		public FreeSpace FreeSpace { get; set; }
	}
}
