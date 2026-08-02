using System.Collections.Generic;

namespace ACadSharp.Prototype1b.Segments;

public class SegmentIndex : FileSegment
{
	public Dictionary<int, SegmentIndexEntry> Pointers { get; set; } = new();

	public struct EntryPointer
	{
		public ulong Offset { get; set; }

		public uint Size { get; set; }

		public override string ToString()
		{
			return $"Offset: {Offset} | Size: {Size}";
		}
	}
}
