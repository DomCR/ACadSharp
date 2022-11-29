namespace ACadSharp.IO.DWG
{
	public class DwgLocalSectionMap
	{
		public int Compression { get; set; } = 2;

		public bool IsEmpty { get; internal set; }

		public ulong Offset { get; internal set; }

		public ulong CompressedSize { get; internal set; }

		public int PageNumber { get; internal set; }

		public ulong DecompressedSize { get; internal set; }

		public long Seeker { get; internal set; }

		public long Size { get; internal set; }

		public ulong Checksum { get; internal set; }

		public ulong CRC { get; internal set; }

		public long PageSize { get; internal set; }

		public uint ODA { get; internal set; }

		public int SectionMap { get; set; }

		public DwgLocalSectionMap() { }

		public DwgLocalSectionMap(int value)
		{
			this.SectionMap = value;
		}
	}
}