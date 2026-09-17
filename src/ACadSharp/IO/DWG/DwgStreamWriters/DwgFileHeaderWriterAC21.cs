using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal class DwgFileHeaderWriterAC21 : DwgFileHeaderWriterAC18
{
	public override int FileHeaderSize { get { return 0x480; } }

	protected override ICompressor compressor => new DwgLZ77AC21Compressor();

	public DwgFileHeaderWriterAC21(Stream stream, Encoding encoding, CadDocument model) : base(stream, encoding, model)
	{
	}

	protected override void craeteLocalSection(DwgSectionDescriptor descriptor, byte[] buffer, int decompressedSize, ulong offset, int totalSize, bool isCompressed)
	{
		// [PATCH] Same as AC18: offset is the page's Start Offset inside the whole decompressed
		// section buffer; buffer is the totalSize-byte tail chunk just read, always starting at 0.
		MemoryStream descriptorStream = this.applyCompression(buffer, decompressedSize, 0, totalSize, isCompressed);

		this.writeMagicNumber();

		//Implementation for the LZ77 compressor for AC1021
		//modify the localsection writer to match this specific version
	}
}