using System.Collections.Generic;

namespace ACadSharp.Prototype1b
{
	public class DataIndexEntry
    {
        public uint DataSegmentIndex { get; set; }
        public List<DataIndexEntryPointer> Pointers { get; set; }
    }
}
