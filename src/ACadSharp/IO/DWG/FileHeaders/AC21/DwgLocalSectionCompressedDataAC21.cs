namespace ACadSharp.IO.DWG.FileHeaders.AC21;

internal class DwgLocalSectionCompressedDataAC21 : DwgLocalSectionMapAC21Base
{
	public ulong PagesMapCorrectionFactor { get; set; }

	public ulong PagesMapCrcCompressed { get; set; }

	public ulong PagesMapCrcUncompressed { get; set; }

	public ulong PagesMapSizeCompressed { get; set; }

	public ulong PagesMapSizeUncompressed { get; set; }

	public override object Clone()
	{
		DwgLocalSectionCompressedDataAC21 obj = new DwgLocalSectionCompressedDataAC21();
		obj.Copy(this);
		return obj;
	}

	public void Copy(DwgLocalSectionCompressedDataAC21 data)
	{
		base.Copy(data);
		this.PagesMapSizeUncompressed = data.PagesMapSizeUncompressed;
		this.PagesMapSizeCompressed = data.PagesMapSizeCompressed;
		this.PagesMapCrcUncompressed = data.PagesMapCrcUncompressed;
		this.PagesMapCrcCompressed = data.PagesMapCrcCompressed;
		this.PagesMapCorrectionFactor = data.PagesMapCorrectionFactor;
	}
}