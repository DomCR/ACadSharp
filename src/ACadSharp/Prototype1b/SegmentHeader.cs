namespace ACadSharp.Prototype1b
{
	public class SegmentHeader
    {
        /// <summary>
        /// The size of the segment header. The <see cref="Name"/> is always 6 characters
        /// long and the <see cref="AlignmentBytes"/> is always 8 bytes long.
        /// </summary>
        public const uint SIZE = 48;

        public short Signature { get; set; }
        public string Name { get; set; }
        public uint SegmentIndex { get; set; }
        public int IsBlob { get; set; }
        public uint SegmentSize { get; set; }
        public int Unknown1 { get; set; }
        public int DataStorageRevision { get; set; }
        public int Unknown2 { get; set; }
        public int SystemDataAlignmentOffset { get; set; }
        public int ObjectDataAlignmentOffset { get; set; }
        public byte[] AlignmentBytes { get; set; }
    }
}
