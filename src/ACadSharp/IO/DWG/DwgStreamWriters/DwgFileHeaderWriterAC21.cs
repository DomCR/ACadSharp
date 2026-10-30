using CSUtilities.IO;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal class DwgFileHeaderWriterAC21 : DwgFileHeaderWriterBase<DwgFileHeaderAC21>
{
	public override int FileHeaderSize { get { return 0x480; } }

	public override int HandleSectionOffset { get { return 0; } }

	private const int HeaderBlockSize = 239;

	private const int HeaderFactor = 3;

	private const int PageBlockSize = 251;

	private const int SystemBlockSize = 239;

	private readonly ICompressor _compressor = new DwgLZ77AC21Compressor();

	private Dictionary<string, DwgSectionDescriptor> _descriptors { get { return this.FileHeader.Descriptors; } }

	private readonly List<DwgLocalSectionMap> _pageRecords = new List<DwgLocalSectionMap>();

	public DwgFileHeaderWriterAC21(Stream stream, Encoding encoding, CadDocument model) : base(stream, encoding, model)
	{
		// File header info
		for (int i = 0; i < this.FileHeaderSize; i++)
		{
			this._stream.WriteByte(0);
		}
	}

	public override void AddSection(string name, MemoryStream stream, bool isCompressed, int decompsize = 0x7400)
	{
		DwgSectionDescriptor descriptor = this.createDescriptor(name, (ulong)stream.Length, (ulong)decompsize, isCompressed);
		this.FileHeader.AddSection(descriptor);

		byte[] buffer = stream.GetBuffer();
		ulong offset = 0uL;
		int nlocalSections = (int)(stream.Length / decompsize);

		for (int i = 0; i < nlocalSections; i++)
		{
			this.createLocalSection(descriptor, buffer, decompsize, offset, decompsize, isCompressed);
			offset += (ulong)decompsize;
		}

		int spearBytes = (int)(stream.Length % decompsize);
		if (spearBytes > 0 && !this.checkEmptyBytes(buffer, offset, (ulong)spearBytes))
		{
			this.createLocalSection(descriptor, buffer, decompsize, offset, spearBytes, isCompressed);
		}
	}

	public override void WriteFile()
	{
		this.writeSectionsMap();

		this.writePagesMap();

		this.writeFileMetaData();
	}

	protected DwgSectionDescriptor createDescriptor(string name, ulong compressedSize, ulong decompressedSize, bool isCompressed)
	{
		DwgSectionDescriptor descriptor = new DwgSectionDescriptor(name);
		descriptor.CompressedSize = compressedSize;
		descriptor.DecompressedSize = decompressedSize;

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

		descriptor.CompressedCode = isCompressed ? 2 : 1;

		return descriptor;
	}

	protected void createLocalSection(DwgSectionDescriptor descriptor, byte[] buffer, int decompressedSize, ulong offset, int totalSize, bool isCompressed)
	{
		byte[] data;
		int dataLength;

		if (isCompressed)
		{
			MemoryStream compressed = new MemoryStream();
			this._compressor.Compress(buffer, (int)offset, totalSize, compressed);
			data = compressed.GetBuffer();
			dataLength = (int)compressed.Length;
		}
		else
		{
			data = new byte[totalSize];
			System.Array.Copy(buffer, (int)offset, data, 0, totalSize);
			dataLength = totalSize;
		}

		DwgLocalSectionMap localMap = new DwgLocalSectionMap();
		localMap.Offset = offset;
		localMap.CompressedSize = (ulong)dataLength;
		localMap.DecompressedSize = (ulong)totalSize;

		if (descriptor.Encoding == 4)
		{
			//Encoded page, use reed solomon
			int factor = (int)((align8((ulong)dataLength) + PageBlockSize - 1) / PageBlockSize);
			data = reedSolomonEncoding(data, dataLength, factor, PageBlockSize);
			dataLength = data.Length;
		}

		this.writePage(localMap, data, dataLength);

		descriptor.PageCount++;
		descriptor.LocalSections.Add(localMap);
	}

	protected void writeFileHeader(MemoryStream stream)
	{
		MemoryStream metadataStream = new MemoryStream();
		StreamIO streamIO = new StreamIO(metadataStream);
		this.writeCompressedMetadata(streamIO);

		MemoryStream compressed = new MemoryStream();
		this._compressor.Compress(metadataStream.GetBuffer(), 0, (int)metadataStream.Length, compressed);

		byte[] decoded = new byte[HeaderFactor * HeaderBlockSize];
		StreamIO writer = new StreamIO(new MemoryStream(decoded));

		//0x00	8	CRC
		writer.Write<long>(0L);
		//0x08	8	Unknown key
		writer.Write<long>(0L);
		//0x10	8	Compressed Data CRC
		writer.Write<long>(0L);
		//0x18	4	ComprLen
		writer.Write<int>((int)compressed.Length);
		//0x1C	4	Length2
		writer.Write<int>(0);

		System.Array.Copy(compressed.GetBuffer(), 0, decoded, 0x20, (int)compressed.Length);

		byte[] encoded = reedSolomonEncoding(decoded, decoded.Length, HeaderFactor, HeaderBlockSize);

		stream.Write(encoded, 0, encoded.Length);
		//Fill the rest of the 0x400 bytes, the last 0x28 bytes are the check data
		stream.Write(new byte[0x400 - encoded.Length], 0, 0x400 - encoded.Length);
	}

	private static ulong align8(ulong value)
	{
		return (value + 7UL) & 0xFFFFFFFFFFFFFFF8UL;
	}

	private static readonly byte[] _gfExp = new byte[512];

	private static readonly byte[] _gfLog = new byte[256];

	static DwgFileHeaderWriterAC21()
	{
		//GF(256) with primitive polynomial x^8 + x^4 + x^3 + x^2 + 1 (0x11D)
		int x = 1;
		for (int i = 0; i < 255; i++)
		{
			_gfExp[i] = (byte)x;
			_gfLog[x] = (byte)i;
			x <<= 1;
			if ((x & 0x100) != 0)
			{
				x ^= 0x11D;
			}
		}

		for (int i = 255; i < 512; i++)
		{
			_gfExp[i] = _gfExp[i - 255];
		}
	}

	private static byte gfMultiply(byte a, byte b)
	{
		if (a == 0 || b == 0)
			return 0;

		return _gfExp[_gfLog[a] + _gfLog[b]];
	}

	private static byte[] rsGenerator(int nsym)
	{
		//g(x) = (x - a^1)(x - a^2)...(x - a^nsym), highest degree first
		byte[] g = new byte[] { 1 };
		for (int i = 1; i <= nsym; i++)
		{
			byte[] next = new byte[g.Length + 1];
			for (int j = 0; j < g.Length; j++)
			{
				next[j] ^= g[j];
				next[j + 1] ^= gfMultiply(g[j], _gfExp[i]);
			}
			g = next;
		}

		return g;
	}

	/// <summary>
	/// Reed-Solomon (255, blockSize) systematic encoding, each codeword is interleaved
	/// in the output as the inverse of the decoding used by the reader.
	/// </summary>
	private static byte[] reedSolomonEncoding(byte[] data, int length, int factor, int blockSize)
	{
		int nsym = byte.MaxValue - blockSize;
		byte[] generator = rsGenerator(nsym);
		byte[] encoded = new byte[factor * byte.MaxValue];
		byte[] parity = new byte[nsym];

		int index = 0;
		for (int i = 0; i < factor; i++)
		{
			System.Array.Clear(parity, 0, nsym);

			int cindex = i;
			for (int j = 0; j < blockSize; j++)
			{
				byte value = index < length ? data[index] : (byte)0;
				encoded[cindex] = value;
				index++;
				cindex += factor;

				//LFSR division by the generator polynomial
				byte feedback = (byte)(value ^ parity[0]);
				for (int k = 0; k < nsym - 1; k++)
				{
					parity[k] = (byte)(parity[k + 1] ^ gfMultiply(feedback, generator[k + 1]));
				}
				parity[nsym - 1] = gfMultiply(feedback, generator[nsym]);
			}

			for (int k = 0; k < nsym; k++)
			{
				encoded[cindex] = parity[k];
				cindex += factor;
			}
		}

		return encoded;
	}

	private void writePage(DwgLocalSectionMap map, byte[] data, int length)
	{
		map.PageNumber = this._pageRecords.Count + 1;
		map.Seeker = this._stream.Position - this.FileHeaderSize;
		map.Size = length;

		this._stream.Write(data, 0, length);

		this._pageRecords.Add(map);
	}

	private DwgLocalSectionMap writeSystemPage(byte[] data, int length, out ulong compressedSize)
	{
		MemoryStream compressed = new MemoryStream();
		this._compressor.Compress(data, 0, length, compressed);
		compressedSize = (ulong)compressed.Length;

		int factor = (int)((align8(compressedSize) + SystemBlockSize - 1) / SystemBlockSize);
		byte[] encoded = reedSolomonEncoding(compressed.GetBuffer(), (int)compressed.Length, factor, SystemBlockSize);

		DwgLocalSectionMap map = new DwgLocalSectionMap();
		map.CompressedSize = compressedSize;
		map.DecompressedSize = (ulong)length;
		this.writePage(map, encoded, encoded.Length);

		return map;
	}

	private void writeSectionsMap()
	{
		MemoryStream stream = new MemoryStream();
		StreamIO writer = new StreamIO(stream);

		foreach (DwgSectionDescriptor descriptor in this._descriptors.Values)
		{
			byte[] name = Encoding.Unicode.GetBytes(descriptor.Name + "\0");

			//0x00	8	Data size
			writer.Write<ulong>(descriptor.CompressedSize);
			//0x08	8	Max size
			writer.Write<ulong>(descriptor.DecompressedSize);
			//0x10	8	Encryption
			writer.Write<ulong>((ulong)descriptor.Encrypted);
			//0x18	8	HashCode
			writer.Write<ulong>(descriptor.HashCode ?? 0);
			//0x20	8	SectionNameLength
			writer.Write<ulong>((ulong)name.Length);
			//0x28	8	Unknown
			writer.Write<ulong>(0UL);
			//0x30	8	Encoding
			writer.Write<ulong>(descriptor.Encoding ?? 1);
			//0x38	8	NumPages
			writer.Write<ulong>((ulong)descriptor.PageCount);
			//Section name (Unicode)
			stream.Write(name, 0, name.Length);

			foreach (DwgLocalSectionMap page in descriptor.LocalSections)
			{
				//8	Page data offset
				writer.Write<ulong>(page.Offset);
				//8	Page Size
				writer.Write<long>(page.Size);
				//8	Page Id
				writer.Write<long>(page.PageNumber);
				//8	Page Uncompressed Size
				writer.Write<ulong>(page.DecompressedSize);
				//8	Page Compressed Size
				writer.Write<ulong>(page.CompressedSize);
				//8	Page Checksum
				writer.Write<ulong>(page.Checksum);
				//8	Page CRC
				writer.Write<ulong>(page.CRC);
			}
		}

		DwgLocalSectionMap map = this.writeSystemPage(stream.GetBuffer(), (int)stream.Length, out ulong compressedSize);

		Dwg21CompressedMetadata metadata = this.FileHeader.CompressedMetadata;
		metadata.SectionsAmount = (ulong)this._descriptors.Count + 1;
		metadata.SectionsMapId = (ulong)map.PageNumber;
		metadata.SectionsMap2Id = (ulong)map.PageNumber;
		metadata.SectionsMapSizeCompressed = compressedSize;
		metadata.SectionsMapSizeUncompressed = (ulong)stream.Length;
		metadata.SectionsMapCorrectionFactor = 1;
	}

	private void writePagesMap()
	{
		int pageId = this._pageRecords.Count + 1;

		//The pages map contains its own record, the size has to be known before writing it
		int length = (pageId) * 16;
		MemoryStream compressed = new MemoryStream();
		this._compressor.Compress(new byte[length], 0, length, compressed);
		int factor = (int)((align8((ulong)compressed.Length) + SystemBlockSize - 1) / SystemBlockSize);
		long pagesMapSize = factor * byte.MaxValue;

		MemoryStream stream = new MemoryStream();
		StreamIO writer = new StreamIO(stream);
		foreach (DwgLocalSectionMap record in this._pageRecords)
		{
			writer.Write<long>(record.Size);
			writer.Write<long>(record.PageNumber);
		}
		writer.Write<long>(pagesMapSize);
		writer.Write<long>(pageId);

		DwgLocalSectionMap map = this.writeSystemPage(stream.GetBuffer(), (int)stream.Length, out ulong compressedSize);

		Dwg21CompressedMetadata metadata = this.FileHeader.CompressedMetadata;
		metadata.PagesMapId = (ulong)map.PageNumber;
		metadata.PagesMapOffset = (ulong)map.Seeker;
		metadata.Map2Id = (ulong)map.PageNumber;
		metadata.Map2Offset = (ulong)map.Seeker;
		metadata.PagesMapSizeCompressed = compressedSize;
		metadata.PagesMapSizeUncompressed = (ulong)stream.Length;
		metadata.PagesMapCorrectionFactor = 1;
		metadata.PagesAmount = (ulong)this._pageRecords.Count;
		metadata.PagesMaxId = (ulong)this._pageRecords.Count;
	}

	private void writeFileMetaData()
	{
		StreamIO writer = new StreamIO(this._stream);

		//Repeated header at the end of the file
		this.FileHeader.CompressedMetadata.Header2offset = (ulong)(this._stream.Position - this.FileHeaderSize);
		this.FileHeader.CompressedMetadata.FileSize = (ulong)(this._stream.Position + 0x400);

		MemoryStream header = new MemoryStream();
		this.writeFileHeader(header);
		this._stream.Write(header.GetBuffer(), 0, (int)header.Length);

		this._stream.Position = 0L;

		//0x00	6	“ACXXXX” version string
		this._stream.Write(Encoding.ASCII.GetBytes(this._document.Header.VersionString), 0, 6);
		//5 bytes of 0x00
		this._stream.Write(new byte[5], 0, 5);
		//0x0B	1	Maintenance release version
		this._stream.WriteByte((byte)this._document.Header.MaintenanceVersion);
		//0x0C	1	Byte 0x00, 0x01, or 0x03
		this._stream.WriteByte(3);
		//0x0D	4	Preview address(long)
		writer.Write(this.getSectionAddress(DwgSectionDefinition.Preview));
		//0x11	1	Dwg version (Acad version that writes the file)
		this._stream.WriteByte(33);
		//0x12	1	Application maintenance release version
		this._stream.WriteByte((byte)this._document.Header.MaintenanceVersion);
		//0x13	2	Codepage
		writer.Write(this.getFileCodePage());
		//0x15	3	3 0x00 bytes
		this._stream.Write(new byte[3], 0, 3);
		//0x18	4	SecurityType
		writer.Write(0);
		//0x1C	4	Unknown long
		writer.Write(0);
		//0x20	4	Summary info Address in stream
		writer.Write(this.getSectionAddress(DwgSectionDefinition.SummaryInfo));
		//0x24	4	VBA Project Addr(0 if not present)
		writer.Write(0u);
		//0x28	4	0x00000080
		writer.Write(0x00000080);
		//0x2C	4	App info Address in stream
		writer.Write(this.getSectionAddress(DwgSectionDefinition.AppInfo));
		//0x30	0x50	0x00 bytes
		this._stream.Write(new byte[0x50], 0, 0x50);

		//0x80	0x400	Encoded file header
		this._stream.Write(header.GetBuffer(), 0, (int)header.Length);

		this._stream.Position = this._stream.Length;
	}

	private uint getSectionAddress(string name)
	{
		if (this._descriptors.TryGetValue(name, out DwgSectionDescriptor descriptor)
			&& descriptor.LocalSections.Count > 0)
		{
			return (uint)(descriptor.LocalSections[0].Seeker + this.FileHeaderSize);
		}

		return 0u;
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