using ACadSharp.Prototype1b.Segments;

namespace ACadSharp.Prototype1b
{
	public class SchemaIndex : IPrototype1bSegment
    {
        public SegmentHeader Header { get; set; }
        public uint Unknown1 { get; set; }			// 0; 1431655765		--> 0; 0x5555 0x5555
        public long UnknownMagic { get; set; }		// 0x0af10c
        public SchemaPropertyPointer[] SchemaPointer { get; set; }
        public SchemaPropertyPointer[] UnknownPropertyEntryPointer { get; set; }
        public SchemaPropertyPointer[] SchemaUnknownPropertyPointer { get; set; }
        public uint UnknownIndex1 { get; set; }		// 5; 4
        public string[] SchemaNames { get; set; }
    }
}
