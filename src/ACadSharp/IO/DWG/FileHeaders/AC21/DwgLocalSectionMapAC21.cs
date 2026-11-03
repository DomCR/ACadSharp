namespace ACadSharp.IO.DWG.FileHeaders.AC21;

internal class DwgLocalSectionMapAC21 : DwgLocalSectionMapAC21Base
{
	public ulong Checksum { get; set; }

	public ulong CompressedSize { get; set; }

	public ulong CRC { get; set; }

	public ulong DecompressedSize { get; set; }

	public ulong Offset { get; set; }

	public ulong PageSize { get; set; }

	public override object Clone()
	{
		DwgLocalSectionMapAC21 clone = new DwgLocalSectionMapAC21();
		clone.CopyFrom(this);
		return clone;
	}

	public void CopyFrom(DwgLocalSectionMapAC21 localSection)
	{
		base.Copy(localSection);
		this.PageSize = localSection.PageSize;
		this.Offset = localSection.Offset;
		this.DecompressedSize = localSection.DecompressedSize;
		this.CompressedSize = localSection.CompressedSize;
		this.Checksum = localSection.Checksum;
		this.CRC = localSection.CRC;
	}
}
