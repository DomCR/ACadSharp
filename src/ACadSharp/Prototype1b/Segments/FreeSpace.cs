namespace ACadSharp.Prototype1b.Segments
{
	public class FreeSpace : IPrototype1bSegment
    {
        public SegmentHeader Header { get; set; }
        public ulong Unknown { get; set; }
        public FreeSpaceArea[] FreeSpaces { get; set; }
    }
}
