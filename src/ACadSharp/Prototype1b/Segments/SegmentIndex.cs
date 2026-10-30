using System.Collections.Generic;

namespace ACadSharp.Prototype1b.Segments
{
	public class SegmentIndex : IPrototype1bSegment
    {
        public SegmentHeader Header { get; set; }
        public Dictionary<int, SegmentIndexEntry> Pointers { get; set; }
    }
}
