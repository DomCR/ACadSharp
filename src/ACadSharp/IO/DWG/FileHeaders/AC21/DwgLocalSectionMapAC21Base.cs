namespace ACadSharp.IO.DWG.FileHeaders.AC21;

internal abstract class DwgLocalSectionMapAC21Base
{
	public long PageNumber { get; set; }

	public ulong Seeker { get; set; }

	public long Size { get; set; }

	public virtual void Copy(DwgLocalSectionMapAC21Base localSection)
	{
		this.PageNumber = localSection.PageNumber;
		this.Seeker = localSection.Seeker;
		this.Size = localSection.Size;
	}

	public abstract object Clone();
}
