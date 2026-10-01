using ACadSharp.Prototype1b.Segments;

namespace ACadSharp.Prototype1b
{
	public class PreviousSave : IPrototype1bSegment
    {
        public FileSegmentHeader Header { get; set; }
        public FileHeader FileHeader { get; set; }
    }
}
