using ACadSharp.Prototype1b.Segments;
using System.Collections.Generic;

namespace ACadSharp.Prototype1b
{
	public class DataIndex : IPrototype1bSegment
    {
        public SegmentHeader Header { get; set; }
        public int Unknown1 { get; set; }
        public Dictionary<uint, DataIndexEntry> Entries { get; set; }
    }
}
