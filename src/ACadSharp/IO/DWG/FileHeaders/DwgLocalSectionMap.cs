namespace ACadSharp.IO.DWG.FileHeaders;

internal class DwgLocalSectionMap
{
	public ulong Checksum { get; set; }

	public ulong CompressedSize { get; set; }

	public int Compression { get; set; } = 2;

	public ulong CRC { get; set; }

	public ulong DecompressedSize { get; set; }

	public bool IsEmpty { get; set; }

	public uint ODA { get; set; }

	public ulong Offset { get; set; }

	public int PageNumber { get; set; }

	public long PageSize { get; set; }

	public int SectionMap { get; set; }

	public long Seeker { get; set; }

	public long Size { get; set; }

	public DwgLocalSectionMap()
	{ }

	public DwgLocalSectionMap(int value)
	{
		this.SectionMap = value;
	}
}