using ACadSharp.Prototype1b.Segments;
using System.Collections.Generic;

namespace ACadSharp.Prototype1b
{
	public class DataField : IPrototype1bSegment
    {
        public SegmentHeader Header { get; set; }
        public List<DataEntry> Entries { get; set; }
    }
}
