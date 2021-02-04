namespace ACadSharp.IO.DWG
{
	public class DwgLocalSectionMap
	{
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
	}
}