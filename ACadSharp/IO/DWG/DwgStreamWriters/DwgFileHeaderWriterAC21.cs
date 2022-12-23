using System.IO;

namespace ACadSharp.IO.DWG
{
	internal class DwgFileHeaderWriterAC21 : DwgFileHeaderWriterAC18
	{
		protected override int _fileHeaderSize { get { return 0x480; } }

		protected override ICompressor compressor => new DwgLZ77AC21Compressor();

		public DwgFileHeaderWriterAC21(Stream stream, CadDocument model) : base(stream, model)
		{
		}

		protected override void craeteLocalSection(DwgSectionDescriptor descriptor, byte[] buffer, int decompressedSize, ulong offset, int totalSize, bool isCompressed)
		{
			MemoryStream descriptorStream = this.applyCompression(buffer, decompressedSize, offset, totalSize, isCompressed);

			this.writeMagicNumber();

			//Implementation for the LZ77 compressor for AC1021
			//modify the localsection writer to match this specific version
		}
	}
}
