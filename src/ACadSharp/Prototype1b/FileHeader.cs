namespace ACadSharp.Prototype1b;

public class FileHeader
{
	public int DataIndexSegmentIndex { get; set; }

	public int DataStorageRevision { get; set; }

	public int FileHeaderSize { get; set; }

	public uint FileSignature { get; set; }

	public int FileSize { get; set; }

	public int FreeSpaceEntryCount { get; set; }

	public int FreeSpaceSegmentIndex { get; set; }

	public int PreviousSaveIndex { get; set; }

	public int SchemaIndexSegmentIndex { get; set; }

	public int SearchSegmentIndex { get; set; }

	public int SegmentIndexEntryCount { get; set; }

	public int SegmentIndexOffset { get; set; }

	public int SegmentIndexUnknown { get; set; }

	public int Unknown1 { get; set; } = 2;

	public int Unknown2 { get; set; } = 0;

	public int Unknown5 { get; set; }

	public byte[] UnknownRemaining { get; set; }

	public int Version { get; set; } = 2;
}