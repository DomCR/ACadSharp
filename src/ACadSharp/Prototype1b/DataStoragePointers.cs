using ACadSharp.Prototype1b.Segments;
using System;

namespace ACadSharp.Prototype1b
{
	[Obsolete("Redundant class")]
	public class DataStoragePointers
	{
		public DataIndex DataIndex { get; set; }

		/// <summary>
		/// Empty areas inside the file which are filled with zeros for the most part
		/// </summary>
		public FreeSpace FreeSpace { get; set; }

		public SchemaIndexFileSegment SchemaIndex { get; set; }

		public SegmentIndex SegmentIndex { get; set; }

	}
}