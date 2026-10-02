using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal class DataIndexSegment : FileSegment
{
	public List<Entry> Entries { get; set; } = new();

	public class Entry
	{
		public uint SchemaIndex { get; set; }

		public uint LocalOffset { get; set; }

		public uint SegmentIndex { get; set; }
	}
}