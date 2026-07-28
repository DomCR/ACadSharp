namespace ACadSharp.Prototype1b
{
	public class FileHeader
    {
        public uint FileSignature { get; set; }
        public int FileHeaderSize { get; set; }
        public int UnknownFlag { get; set; }			// 0; 1
        public int Unknown1 { get; set; }				// 8; 2
        public int Version { get; set; }
        public int Unknown2 { get; set; }				// 0
        public int DataStorageRevision { get; set; }
        public int SegmentIndexOffset { get; set; }				// segidx --> this._reader.Position of beginning
        public int SegmentIndexUnknown { get; set; }	// 0
        public int SegmentIndexEntryCount { get; set; }			// lastId / count of segidx entries
        public int SchemaIndexSegmentIndex { get; set; }		// schidx
        public int DataIndexSegmentIndex { get; set; }			// datidx
        public int SearchSegmentIndex { get; set; }				// search
        public int PreviousSaveIndex { get; set; }				// prvsav
        public int FileSize { get; set; }
        public int Unknown3 { get; set; }				// 0
        public int FreeSpaceSegmentIndex { get; set; }			// freesp
        public int FreeSpaceEntryCount { get; set; }
        public int Unknown5 { get; set; }				// 0

        public byte[] UnknownRemaining { get; set; }	// Zeros
    }
}
