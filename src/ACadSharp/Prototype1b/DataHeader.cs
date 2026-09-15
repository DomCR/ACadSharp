namespace ACadSharp.Prototype1b
{
	public class DataHeader
    {
        public uint EntrySize { get; set; }
        public uint Unknown1 { get; set; }
        public ulong Handle { get; set; }
        public uint DataOffset { get; set; }

        // Resolved Values
        public uint SchemaIndex { get; set; }
    }
}
