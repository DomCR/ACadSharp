using CSUtilities.IO;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgFileHeaderWriterAC21 : DwgFileHeaderWriterAC18
	{
		protected override ICompressor compressor => new DwgLZ77AC21Compressor();

		protected new DwgFileHeaderAC21 fileHeader { get; } = new DwgFileHeaderAC21();

		protected override int fileHeaderSize { get { return 0x480; } }

		public DwgFileHeaderWriterAC21(Stream stream, Encoding encoding, CadDocument model) : base(stream, encoding, model)
		{
		}

		protected override void writeFileHeader(MemoryStream stream)
		{
			//0x00	8	CRC

			System.IO.MemoryStream metadataStream = new System.IO.MemoryStream();
			StreamIO streamIO = new StreamIO(metadataStream);
			this.writeCompressedMetadata(streamIO);
		}

		private void writeCompressedMetadata(StreamIO writer)
		{
			//0x00	8	Header size (normally 0x70)
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.HeaderSize);
			//0x08	8	File size
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.FileSize);
			//0x10	8	PagesMapCrcCompressed
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.PagesMapCrcCompressed);
			//0x18	8	PagesMapCorrectionFactor
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.PagesMapCorrectionFactor);
			//0x20	8	PagesMapCrcSeed
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.PagesMapCrcSeed);
			//0x28	8	Pages map2offset(relative to data page map 1, add 0x480 to get stream position)
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.Map2Offset);
			//0x30	8	Pages map2Id
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.Map2Id);
			//0x38	8	PagesMapOffset(relative to data page map 1, add 0x480 to get stream position)
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.PagesMapOffset);
			//0x40	8	PagesMapId
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.PagesMapId);
			//0x48	8	Header2offset(relative to page map 1 address, add 0x480 to get stream position)
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.Header2offset);
			//0x50	8	PagesMapSizeCompressed
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.PagesMapSizeCompressed);
			//0x58	8	PagesMapSizeUncompressed
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.PagesMapSizeUncompressed);
			//0x60	8	PagesAmount
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.PagesAmount);
			//0x68	8	PagesMaxId
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.PagesMaxId);
			//0x70	8	Unknown(normally 0x20, 32)
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.Unknow0x20);
			//0x78	8	Unknown(normally 0x40, 64)
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.Unknow0x40);
			//0x80	8	PagesMapCrcUncompressed
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.PagesMapCrcUncompressed);
			//0x88	8	Unknown(normally 0xf800, 63488)
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.Unknown0xF800);
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.Unknown4);
			//0x98	8	Unknown(normally 1)
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.Unknown1);
			//0xA0	8	SectionsAmount(number of sections + 1)
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.SectionsAmount);
			//0xA8	8	SectionsMapCrcUncompressed
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.SectionsMapCrcUncompressed);
			//0xB0	8	SectionsMapSizeCompressed
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.SectionsMapSizeCompressed);
			//0xB8	8	SectionsMap2Id
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.SectionsMap2Id);
			//0xC0	8	SectionsMapId
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.SectionsMapId);
			//0xC8	8	SectionsMapSizeUncompressed
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.SectionsMapSizeUncompressed);
			//0xD0	8	SectionsMapCrcCompressed
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.SectionsMapCrcCompressed);
			//0xD8	8	SectionsMapCorrectionFactor
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.SectionsMapCorrectionFactor);
			//0xE0	8	SectionsMapCrcSeed
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.SectionsMapCrcSeed);
			//0xE8	8	StreamVersion(normally 0x60100)
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.StreamVersion);
			//0xF0	8	CrcSeed
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.CrcSeed);
			//0xF8	8	CrcSeedEncoded
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.CrcSeedEncoded);
			//0x100	8	RandomSeed
			writer.Write<ulong>(this.fileHeader.CompressedMetadata.RandomSeed);
			//0x108	8	Header CRC64
			writer.Write<ulong>(0uL);
		}

		protected override void createLocalSection(DwgSectionDescriptor descriptor, byte[] buffer, int decompressedSize, ulong offset, int totalSize, bool isCompressed)
		{
			base.createLocalSection(descriptor, buffer, decompressedSize, offset, totalSize, isCompressed);
			return;

			MemoryStream descriptorStream = this.applyCompression(buffer, decompressedSize, offset, totalSize, isCompressed);

			this.writeMagicNumber();

			//Implementation for the LZ77 compressor for AC1021
			//modify the local section writer to match this specific version
		}

		protected override DwgSectionDescriptor createDescriptor(string name, ulong compressedSize, ulong decompressedSize, bool isCompressed)
		{
			DwgSectionDescriptor descriptor = base.createDescriptor(name, compressedSize, decompressedSize, isCompressed);

			switch (name)
			{
				case DwgSectionDefinition.Security:
					descriptor.HashCode = 0x4a0204ea;
					descriptor.Encrypted = 0;
					descriptor.Encoding = 1;
					break;
				case DwgSectionDefinition.FileDepList:
					descriptor.HashCode = 0x6c4205ca;
					descriptor.Encrypted = 2;
					descriptor.Encoding = 1;
					break;
				case DwgSectionDefinition.VBAProject:
					descriptor.HashCode = 0x586e0544;
					descriptor.Encrypted = 2;
					descriptor.Encoding = 1;
					break;
				case DwgSectionDefinition.AppInfo:
					descriptor.HashCode = 0x3fa0043e;
					descriptor.Encrypted = 0;
					descriptor.Encoding = 1;
					break;
				case DwgSectionDefinition.Preview:
					descriptor.HashCode = 0x40aa0473;
					descriptor.Encrypted = 0;
					descriptor.Encoding = 1;
					break;
				case DwgSectionDefinition.SummaryInfo:
					descriptor.HashCode = 0x717a060f;
					descriptor.Encrypted = 0;
					descriptor.Encoding = 1;
					break;
				case DwgSectionDefinition.RevHistory:
					descriptor.HashCode = 0x60a205b3;
					descriptor.Encrypted = 0;
					descriptor.Encoding = 4;
					break;
				case DwgSectionDefinition.AcDbObjects:
					descriptor.HashCode = 0x674c05a9;
					descriptor.Encrypted = 0;
					descriptor.Encoding = 4;
					break;
				case DwgSectionDefinition.ObjFreeSpace:
					descriptor.HashCode = 0x77e2061f;
					descriptor.Encrypted = 0;
					descriptor.Encoding = 4;
					break;
				case DwgSectionDefinition.Template:
					descriptor.HashCode = 0x4a1404ce;
					descriptor.Encrypted = 0;
					descriptor.Encoding = 4;
					break;
				case DwgSectionDefinition.Handles:
					descriptor.HashCode = 0x3f6e0450;
					descriptor.Encrypted = 0;
					descriptor.Encoding = 4;
					break;
				case DwgSectionDefinition.Classes:
					descriptor.HashCode = 0x3f54045f;
					descriptor.Encrypted = 0;
					descriptor.Encoding = 4;
					break;
				case DwgSectionDefinition.AuxHeader:
					descriptor.HashCode = 0x54f0050a;
					descriptor.Encrypted = 0;
					descriptor.Encoding = 4;
					break;
				case DwgSectionDefinition.Header:
					descriptor.HashCode = 0x32b803d9;
					descriptor.Encrypted = 0;
					descriptor.Encoding = 4;
					break;
				default:
					throw new System.NotImplementedException(name);
			}

			//Remove this line
			descriptor.CompressedCode = 2;

			return descriptor;
		}
	}
}