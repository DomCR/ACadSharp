using ACadSharp.Prototype1b.Segments;
using System.Collections.Generic;

namespace ACadSharp.Prototype1b
{
	public class SchemaSearch : IPrototype1bSegment
    {
        public SegmentHeader Header { get; set; }
        public List<SchemaSearchEntry> Entries { get; set; }
    }
}
