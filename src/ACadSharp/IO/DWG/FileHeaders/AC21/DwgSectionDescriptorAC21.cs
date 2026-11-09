using System.IO;

namespace ACadSharp.IO.DWG.FileHeaders.AC21;

internal class DwgSectionDescriptorAC21
{
	public ulong CompressedSize { get; set; }

	public ulong DecompressedSize { get; set; }

	public ulong? Encoding { get; set; }

	public ulong Encryption { get; set; }

	public ulong? HashCode { get; set; }

	public bool IsCompressed { get; set; }

	public System.Collections.Generic.List<DwgLocalSectionMapAC21> LocalSections { get; } = new();

	public string Name { get; set; } = string.Empty;

	public ulong PageCount { get; set; }

	public MemoryStream Stream { get; set; }

	public DwgSectionDescriptorAC21()
	{
	}

	public DwgSectionDescriptorAC21(string name)
	{
		this.Name = name;
		this.Stream = new MemoryStream();
	}

	public void Update()
	{
		this.PageCount = (ulong)(this.Stream.Length + (long)this.DecompressedSize - 1) / this.DecompressedSize;
	}
}