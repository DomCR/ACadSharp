using System.Collections.Generic;

namespace ACadSharp.Prototype1b.Segments
{
	public class DataField : IPrototype1bSegment
	{
		public List<DataEntry> Entries { get; set; }

		public SegmentHeader Header { get; set; }
	}
}