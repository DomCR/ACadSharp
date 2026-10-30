namespace ACadSharp.Prototype1b.Segments
{
	public class Blob01 : IPrototype1bSegment
    {
        public SegmentHeader Header { get; set; }
        public ulong TotalDataSize { get; set; }
        public ulong PageStartOffset { get; set; }
        public uint PageIndex { get; set; }
        public uint PageCount { get; set; }
        public ulong PageDataSize { get; set; }
        public byte[] Data { get; set; }
    }
}
