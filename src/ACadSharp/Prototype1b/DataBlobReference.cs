using System.Collections.Generic;

namespace ACadSharp.Prototype1b
{
	public class DataBlobReference
    {
        public ulong TotalDataSize { get; set; }
        public uint PageCount { get; set; }
        public uint RecordSize { get; set; }    // DataBlobReference size
        public uint PageSize { get; set; }
        public uint LastPageSize { get; set; }
        public uint Unknown1 { get; set; }
        public uint Unknown2 { get; set; }

		/// Pointer (segment index, size) to blob01 segments containing the data
		public List<(uint, uint)> SegmentPointers { get; set; }
    }
}
