using ACadSharp.Prototype1b.Segments;
using System;
using System.Collections.Generic;

namespace ACadSharp.Prototype1b;

public class DataIndex : FileSegment
{
	public List<Entry> Entries { get; } = new List<Entry>();

	[Obsolete]
	public Dictionary<uint, DataIndexEntry> Entries_Old { get; set; }

	public int Unknown1 { get; set; }

	public struct Entry
	{
		public uint SegmentIndex { get; set; }

		public uint LocalOffset { get; set; }

		public uint SchemaIndex { get; set; }

		public override string ToString()
		{
			return $"SegmentIndex: {SegmentIndex} | SchemaIndex: {SchemaIndex} | Offset: {LocalOffset}";
		}
	}
}