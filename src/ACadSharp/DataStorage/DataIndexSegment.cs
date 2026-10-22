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

		public override string ToString()
		{
			return $"SchemaIndex {this.SchemaIndex} | SegmentIndex {this.SegmentIndex} | LocalOffset: {this.LocalOffset}";
		}
	}

	public List<Entry> GetEntriesBySchemaIndex(uint segmentIndex)
	{
		List<Entry> result = new();
		foreach (Entry entry in this.Entries)
		{
			if (entry.SegmentIndex == segmentIndex)
			{
				result.Add(entry);
			}
		}
		return result;
	}
}