using CSUtilities.IO;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal class DwgFileHeaderWriterAC21 : DwgFileHeaderWriterBase<DwgFileHeaderAC21>
{
	public override int FileHeaderSize { get { return 0x480; } }

	public override int HandleSectionOffset { get; }

	private readonly ICompressor _compressor = new DwgLZ77AC21Compressor();

	public DwgFileHeaderWriterAC21(Stream stream, Encoding encoding, CadDocument model) : base(stream, encoding, model)
	{
	}

	public override void AddSection(string name, MemoryStream stream, bool isCompressed, int decompsize = 29696)
	{
		throw new System.NotImplementedException();
	}

	public override void WriteFile()
	{
		throw new System.NotImplementedException();
	}

	protected DwgSectionDescriptor createDescriptor(string name, ulong compressedSize, ulong decompressedSize, bool isCompressed)
	{
		DwgSectionDescriptor descriptor = new DwgSectionDescriptor();

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

	protected void createLocalSection(DwgSectionDescriptor descriptor, byte[] buffer, int decompressedSize, ulong offset, int totalSize, bool isCompressed)
	{
		return;
	}

	protected void writeFileHeader(MemoryStream stream)
	{
		//0x00	8	CRC

		MemoryStream metadataStream = new MemoryStream();
		StreamIO streamIO = new StreamIO(metadataStream);
		this.writeCompressedMetadata(streamIO);
	}

	private void writeCompressedMetadata(StreamIO writer)
	{
		//0x00	8	Header size (normally 0x70)
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.HeaderSize);
		//0x08	8	File size
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.FileSize);
		//0x10	8	PagesMapCrcCompressed
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.PagesMapCrcCompressed);
		//0x18	8	PagesMapCorrectionFactor
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.PagesMapCorrectionFactor);
		//0x20	8	PagesMapCrcSeed
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.PagesMapCrcSeed);
		//0x28	8	Pages map2offset(relative to data page map 1, add 0x480 to get stream position)
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.Map2Offset);
		//0x30	8	Pages map2Id
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.Map2Id);
		//0x38	8	PagesMapOffset(relative to data page map 1, add 0x480 to get stream position)
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.PagesMapOffset);
		//0x40	8	PagesMapId
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.PagesMapId);
		//0x48	8	Header2offset(relative to page map 1 address, add 0x480 to get stream position)
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.Header2offset);
		//0x50	8	PagesMapSizeCompressed
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.PagesMapSizeCompressed);
		//0x58	8	PagesMapSizeUncompressed
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.PagesMapSizeUncompressed);
		//0x60	8	PagesAmount
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.PagesAmount);
		//0x68	8	PagesMaxId
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.PagesMaxId);
		//0x70	8	Unknown(normally 0x20, 32)
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.Unknow0x20);
		//0x78	8	Unknown(normally 0x40, 64)
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.Unknow0x40);
		//0x80	8	PagesMapCrcUncompressed
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.PagesMapCrcUncompressed);
		//0x88	8	Unknown(normally 0xf800, 63488)
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.Unknown0xF800);
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.Unknown4);
		//0x98	8	Unknown(normally 1)
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.Unknown1);
		//0xA0	8	SectionsAmount(number of sections + 1)
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.SectionsAmount);
		//0xA8	8	SectionsMapCrcUncompressed
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.SectionsMapCrcUncompressed);
		//0xB0	8	SectionsMapSizeCompressed
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.SectionsMapSizeCompressed);
		//0xB8	8	SectionsMap2Id
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.SectionsMap2Id);
		//0xC0	8	SectionsMapId
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.SectionsMapId);
		//0xC8	8	SectionsMapSizeUncompressed
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.SectionsMapSizeUncompressed);
		//0xD0	8	SectionsMapCrcCompressed
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.SectionsMapCrcCompressed);
		//0xD8	8	SectionsMapCorrectionFactor
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.SectionsMapCorrectionFactor);
		//0xE0	8	SectionsMapCrcSeed
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.SectionsMapCrcSeed);
		//0xE8	8	StreamVersion(normally 0x60100)
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.StreamVersion);
		//0xF0	8	CrcSeed
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.CrcSeed);
		//0xF8	8	CrcSeedEncoded
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.CrcSeedEncoded);
		//0x100	8	RandomSeed
		writer.Write<ulong>(this.FileHeader.CompressedMetadata.RandomSeed);
		//0x108	8	Header CRC64
		writer.Write<ulong>(0uL);
	}
}