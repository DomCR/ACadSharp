namespace ACadSharp.Prototype1b;

public class FileSegmentHeader
{
	public int DataStorageRevision { get; set; }

	public int IsBlob { get; set; }

	public string Name { get; set; }

	public int ObjectDataAlignmentOffset { get; set; }

	public uint SegmentIndex { get; set; }

	public uint SegmentSize { get; set; }

	public short Signature { get; set; }

	public int SystemDataAlignmentOffset { get; set; }

	public int Unknown2 { get; set; }

	public int Unknown3 { get; set; }

	/// <summary>
	/// The size of the segment header. The <see cref="Name"/> is always 6 characters
	/// long and the AlignmentBytes is always 8 bytes long.
	/// </summary>
	public const uint SIZE = 48;

	public override string ToString()
	{
		return Name;
	}
}