using System.Collections.Generic;

namespace ACadSharp.Prototype1b.Segments
{
	public class SchemaData : IPrototype1bSegment
    {
        public SegmentHeader Header { get; set; }
        public List<SchemaUnknownProperty> SchemaUnknownProperties { get; set; }
        public List<Schema> Values { get; set; }
    }
}
