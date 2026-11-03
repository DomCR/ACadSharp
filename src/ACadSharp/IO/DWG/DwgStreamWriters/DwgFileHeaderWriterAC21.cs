using ACadSharp.IO.DWG.FileHeaders;
using ACadSharp.IO.DWG.FileHeaders.AC21;
using CSUtilities.Converters;
using CSUtilities.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal class DwgFileHeaderWriterAC21 : DwgFileHeaderWriterBase<DwgFileHeaderAC21>
{
	public override int FileHeaderSize { get { return _dataStartOffset; } }

	public override int HandleSectionOffset { get; }

	private const int _dataStartOffset = 0x480;

	private const int _encodedHeaderSize = 0x400;

	private const int _metadataSize = 0x110;

	private const int _pageAlignment = 0x20;

	private DwgSectionDescriptorAC21 _appInfoDescriptor;

	private CrcRandomEncoding _crcEncoding = new CrcRandomEncoding();

	private Dictionary<string, DwgSectionDescriptorAC21> _descriptors = new();

	private DwgLocalSectionCompressedDataAC21 _map2Record = new DwgLocalSectionCompressedDataAC21();

	private List<DwgLocalSectionMapAC21Base> _pageRecords = new();

	private DwgLocalSectionCompressedDataAC21 _pagesMapRecord = new DwgLocalSectionCompressedDataAC21();

	private DwgSectionDescriptorAC21 _previewDescriptor;

	private DwgSectionDescriptorAC21 _summaryInfoDescriptor;

	public DwgFileHeaderWriterAC21(Stream stream, Encoding encoding, CadDocument document)
		: base(stream, encoding, document)
	{
		Random random = new Random();
		byte[] seedBytes = new byte[8];
		random.NextBytes(seedBytes);
		this._crcEncoding.Initialize(LittleEndianConverter.Instance.ToUInt64(seedBytes));

		this.FileHeader.CompressedMetadata.RandomSeed = this._crcEncoding.Seed;
		this.FileHeader.CompressedMetadata.CrcSeed = 0;
		this.FileHeader.CompressedMetadata.PagesMapCrcSeed = 0;
		this.FileHeader.CompressedMetadata.SectionsMapCrcSeed = 0;

		this.initialize();
	}

	public override void AddSection(string name, MemoryStream stream, bool isCompressed, int decompsize = 0x7400)
	{
		this._descriptors[name].Stream = stream;
	}

	public override void WriteFile()
	{
		for (int i = 0; i < this.FileHeaderSize; i++)
		{
			this._stream.WriteByte(0);
		}

		int totalPageCount = 0;
		foreach (DwgSectionDescriptorAC21 item in this._descriptors.Values)
		{
			item.Update();
			totalPageCount += (int)item.PageCount;
		}

		byte[] pagesMapBuffer = new byte[DwgChecksumCalculatorAC21.CalculatePageSize((ulong)(totalPageCount + 5) * 16uL)];

		this.reservePage(this._pagesMapRecord, pagesMapBuffer, 0, pagesMapBuffer.Length);
		this.reservePage(this._map2Record, pagesMapBuffer, 0, pagesMapBuffer.Length);

		foreach (var item in this._descriptors.Values)
		{
			this.createDescriptorSection(item);
		}

		this.FileHeader.CompressedMetadata.PagesMaxId = (uint)(this._pageRecords.Count + 2);
		this.FileHeader.CompressedMetadata.PagesMapId = this.FileHeader.CompressedMetadata.PagesMaxId;
		this.FileHeader.CompressedMetadata.SectionsMapId = this.FileHeader.CompressedMetadata.PagesMaxId - 1;

		this.writeDescriptors();

		this.writePagesMap();

		this.writeFileHeader();

		this.writeFileMetaData();

		this._stream.Flush();
	}

	private static ulong alignTo8(ulong value)
	{
		return (value + 7) & ~7uL;
	}

	private static void writeZeroPadding(Stream stream, ulong targetLength)
	{
		while ((ulong)stream.Length < targetLength)
		{
			stream.WriteByte(0);
		}
	}

	private void compressAndEncodePagesMap(DwgLocalSectionCompressedDataAC21 record, MemoryStream data, ulong crcSeed, ulong pageSize, out byte[] encodedData)
	{
		record.PagesMapSizeUncompressed = (ulong)data.Length;
		record.PagesMapCrcUncompressed = CRC.Encode(
			data.GetBuffer(), 0u, (uint)data.Length,
			CRC.UpdateSeed1(crcSeed, (uint)data.Length), true);

		MemoryStream compressed = new MemoryStream();
		new DwgLZ77AC21Compressor().Compress(data.GetBuffer(), 0, (int)data.Length, compressed);
		if (compressed.Length >= data.Length)
		{
			compressed = data;
		}
		record.PagesMapSizeCompressed = (ulong)compressed.Length;
		record.PagesMapCrcCompressed = CRC.Encode(compressed.GetBuffer(), 0u, (uint)compressed.Length,
			CRC.UpdateSeed1(crcSeed, (uint)compressed.Length), true);

		ulong alignedLength = alignTo8((ulong)compressed.Length);
		writeZeroPadding(compressed, alignedLength);
		ulong blockCount = pageSize / 255uL;
		record.PagesMapCorrectionFactor = blockCount * ReedSolomon.Rs255_239.BlockSize / alignedLength;
		for (int j = 1; j < (int)record.PagesMapCorrectionFactor; j++)
		{
			compressed.Write(compressed.GetBuffer(), 0, (int)alignedLength);
		}
		encodedData = new byte[pageSize];
		uint position = 0u;
		uint encodedLength = (uint)(this.encodeInterleaved(encodedData, position, compressed.GetBuffer(), 0u, (int)compressed.Length, ReedSolomon.Rs255_239) * 255);
		position += encodedLength;
		uint totalBlocksLength = (uint)((int)blockCount * 255);
		uint zeroPaddingLength = totalBlocksLength - encodedLength;
		for (int k = 0; k < zeroPaddingLength; k++)
		{
			encodedData[position++] = 0;
		}
		uint randomPaddingLength = (uint)(int)pageSize - totalBlocksLength;
		this.fillRandomPadding(encodedData, position, randomPaddingLength);
		if (position + randomPaddingLength != pageSize)
		{
			throw new InvalidOperationException("Encoded pages map size does not match the page size.");
		}
	}

	private void copyAndEncodeBlocks(byte[] dataBuffer, uint dataOffset, byte[] parityBuffer, uint parityOffset, byte[] source, uint sourceOffset, int length, ReedSolomon reedSolomon)
	{
		for (int i = 0; i < length; i++)
		{
			dataBuffer[dataOffset + i] = source[sourceOffset + i];
		}
		while (length >= reedSolomon.BlockSize)
		{
			this.encodeBlock(parityBuffer, ref parityOffset, source, sourceOffset, reedSolomon);
			length -= (int)reedSolomon.BlockSize;
			sourceOffset += reedSolomon.BlockSize;
		}
		if (length > 0)
		{
			byte[] paddedBlock = new byte[reedSolomon.BlockSize];
			int blockIndex;
			for (blockIndex = 0; blockIndex < length; blockIndex++)
			{
				paddedBlock[blockIndex] = source[sourceOffset++];
			}
			int randomIndex = 0;
			while (blockIndex < reedSolomon.BlockSize)
			{
				paddedBlock[blockIndex++] = this._crcEncoding.RandomBlock[randomIndex++];
			}
			this.encodeBlock(parityBuffer, ref parityOffset, paddedBlock, 0u, reedSolomon);
		}
	}

	private void createDescriptorSection(DwgSectionDescriptorAC21 descriptor)
	{
		descriptor.CompressedSize = (ulong)descriptor.Stream.Length;
		ulong pageCount = (ulong)descriptor.Stream.Length / descriptor.DecompressedSize;
		byte[] buffer = descriptor.Stream.GetBuffer();
		descriptor.Stream.Dispose();
		descriptor.Stream = null;

		ulong offset = 0uL;
		for (int i = 0; i < (int)pageCount; i++)
		{
			this.createLocalSection(descriptor, buffer, offset, descriptor.DecompressedSize);
			offset += descriptor.DecompressedSize;
		}

		ulong remainingSize = descriptor.CompressedSize % descriptor.DecompressedSize;
		if (remainingSize != 0)
		{
			this.createLocalSection(descriptor, buffer, offset, remainingSize);
			pageCount++;
		}
		descriptor.PageCount = pageCount;
	}

	private void createLocalSection(DwgSectionDescriptorAC21 descriptor, byte[] buffer, ulong offset, ulong length)
	{
		DwgLocalSectionMapAC21 localMap = new DwgLocalSectionMapAC21();
		localMap.PageSize = descriptor.DecompressedSize;
		localMap.Offset = offset;
		localMap.DecompressedSize = length;
		localMap.Checksum = DwgChecksumCalculatorAC21.Calculate(0, buffer, (uint)offset, (uint)length);

		ReedSolomon reedSolomon = ReedSolomon.Rs255_251;
		MemoryStream compressed = new MemoryStream();
		if (descriptor.IsCompressed)
		{
			new DwgLZ77AC21Compressor().Compress(buffer, (int)offset, (int)length, compressed);
			if (compressed.Length >= (long)length)
			{
				compressed = new MemoryStream();
				compressed.Write(buffer, (int)offset, (int)length);
			}
		}
		else
		{
			compressed.Write(buffer, (int)offset, (int)length);
		}

		localMap.CompressedSize = (ulong)compressed.Length;
		localMap.CRC = CRC.Encode(
			compressed.GetBuffer(), 0u, (uint)compressed.Length,
			CRC.UpdateSeed1(0, (uint)compressed.Length), true);

		ulong alignedLength = alignTo8((ulong)compressed.Length);
		int blockCount = (int)((alignedLength + reedSolomon.BlockSize - 1) / reedSolomon.BlockSize);
		byte[] encodedData;
		if (descriptor.Encoding == 4)
		{
			encodedData = new byte[blockCount * reedSolomon.CodewordLength];
			writeZeroPadding(compressed, alignedLength);
			this.encodeInterleaved(encodedData, 0u, compressed.GetBuffer(), 0u, (int)compressed.Length, reedSolomon);
		}
		else if (descriptor.Encoding == 1)
		{
			writeZeroPadding(compressed, alignedLength);
			encodedData = new byte[(long)alignedLength + blockCount * (reedSolomon.CodewordLength - reedSolomon.BlockSize)];
			this.copyAndEncodeBlocks(encodedData, 0u, encodedData, (uint)alignedLength, compressed.GetBuffer(), 0u, (int)compressed.Length, reedSolomon);
		}
		else
		{
			throw new NotSupportedException($"Section encoding {descriptor.Encoding} is not supported for section {descriptor.Name}.");
		}

		this.reservePage(localMap, encodedData, 0, encodedData.Length);
		if (localMap.PageNumber > 0)
		{
			descriptor.PageCount++;
		}
		descriptor.LocalSections.Add(localMap);
	}

	private void encodeBlock(byte[] parityBuffer, ref uint parityOffset, byte[] source, uint sourceOffset, ReedSolomon reedSolomon)
	{
		reedSolomon.Encode(source, sourceOffset, parityBuffer, parityOffset);
		parityOffset += reedSolomon.CodewordLength - reedSolomon.BlockSize;
	}

	private void encodeBlockInterleaved(byte[] target, uint targetOffset, byte[] source, uint sourceOffset, int stride, ReedSolomon reedSolomon)
	{
		uint sourceIndex = sourceOffset;
		int remaining = (int)(reedSolomon.BlockSize - 1);
		while (remaining >= 0)
		{
			target[targetOffset] = source[sourceIndex];
			targetOffset += (uint)stride;
			remaining--;
			sourceIndex++;
		}

		byte[] parity = reedSolomon.Encode(source, sourceOffset);
		int parityLength = parity.Length;
		for (int i = 0; i < parityLength; i++)
		{
			target[targetOffset] = parity[i];
			targetOffset += (uint)stride;
		}
	}

	private int encodeInterleaved(byte[] target, uint targetOffset, byte[] source, uint sourceOffset, int length, ReedSolomon reedSolomon)
	{
		int blockCount = (int)((length + reedSolomon.BlockSize - 1) / reedSolomon.BlockSize);
		while (length >= reedSolomon.BlockSize)
		{
			this.encodeBlockInterleaved(target, targetOffset, source, sourceOffset, blockCount, reedSolomon);
			targetOffset++;
			length -= (int)reedSolomon.BlockSize;
			sourceOffset += reedSolomon.BlockSize;
		}
		if (length > 0)
		{
			byte[] paddedBlock = new byte[reedSolomon.BlockSize];
			int blockIndex = 0;
			while (blockIndex < length)
			{
				paddedBlock[blockIndex] = source[sourceOffset];
				blockIndex++;
				sourceOffset++;
			}
			int randomIndex = 0;
			while (blockIndex < reedSolomon.BlockSize)
			{
				paddedBlock[blockIndex++] = this._crcEncoding.RandomBlock[randomIndex++];
			}
			this.encodeBlockInterleaved(target, targetOffset, paddedBlock, 0u, blockCount, reedSolomon);
		}
		return blockCount;
	}

	private void fillRandomPadding(byte[] buffer, uint offset, uint count)
	{
		for (int i = 0; i < count; i++)
		{
			buffer[offset + i] = this._crcEncoding.RandomBlock[i];
		}
	}

	private void initialize()
	{
		DwgSectionDescriptorAC21 appInfoDescriptor = this._appInfoDescriptor = new DwgSectionDescriptorAC21(DwgSectionDefinition.AppInfo);
		appInfoDescriptor.HashCode = 1067451454uL;
		appInfoDescriptor.DecompressedSize = 0x300uL;
		appInfoDescriptor.Encryption = 0uL;
		appInfoDescriptor.Encoding = 1uL;
		this._descriptors.Add(appInfoDescriptor.Name, appInfoDescriptor);

		DwgSectionDescriptorAC21 previewDescriptor = this._previewDescriptor = new DwgSectionDescriptorAC21(DwgSectionDefinition.Preview);
		previewDescriptor.HashCode = 1084884083uL;
		previewDescriptor.DecompressedSize = 1024uL;
		previewDescriptor.Encryption = 0uL;
		previewDescriptor.Encoding = 1uL;
		this._descriptors.Add(previewDescriptor.Name, previewDescriptor);

		DwgSectionDescriptorAC21 summaryInfoDescriptor = this._summaryInfoDescriptor = new DwgSectionDescriptorAC21(DwgSectionDefinition.SummaryInfo);
		summaryInfoDescriptor.HashCode = 1903822351uL;
		summaryInfoDescriptor.DecompressedSize = 128uL;
		summaryInfoDescriptor.Encryption = 0uL;
		summaryInfoDescriptor.Encoding = 1uL;
		this._descriptors.Add(summaryInfoDescriptor.Name, summaryInfoDescriptor);

		DwgSectionDescriptorAC21 revHistoryDescriptor = new DwgSectionDescriptorAC21(DwgSectionDefinition.RevHistory);
		revHistoryDescriptor.HashCode = 1621231027uL;
		revHistoryDescriptor.DecompressedSize = 4096uL;
		revHistoryDescriptor.Encryption = 0uL;
		revHistoryDescriptor.Encoding = 4uL;
		revHistoryDescriptor.IsCompressed = true;
		this._descriptors.Add(revHistoryDescriptor.Name, revHistoryDescriptor);

		DwgSectionDescriptorAC21 objectsDescriptor = new DwgSectionDescriptorAC21(DwgSectionDefinition.AcDbObjects);
		objectsDescriptor.HashCode = 1733035433uL;
		objectsDescriptor.DecompressedSize = 63488uL;
		objectsDescriptor.Encryption = 0uL;
		objectsDescriptor.Encoding = 4uL;
		objectsDescriptor.IsCompressed = true;
		this._descriptors.Add(objectsDescriptor.Name, objectsDescriptor);

		DwgSectionDescriptorAC21 objFreeSpaceDescriptor = new DwgSectionDescriptorAC21(DwgSectionDefinition.ObjFreeSpace);
		objFreeSpaceDescriptor.HashCode = 2011301407uL;
		objFreeSpaceDescriptor.DecompressedSize = 63488uL;
		objFreeSpaceDescriptor.Encryption = 0uL;
		objFreeSpaceDescriptor.Encoding = 4uL;
		objFreeSpaceDescriptor.IsCompressed = true;
		this._descriptors.Add(objFreeSpaceDescriptor.Name, objFreeSpaceDescriptor);

		DwgSectionDescriptorAC21 templateDescriptor = new DwgSectionDescriptorAC21(DwgSectionDefinition.Template);
		templateDescriptor.HashCode = 1242825934uL;
		templateDescriptor.DecompressedSize = 1024uL;
		templateDescriptor.Encryption = 0uL;
		templateDescriptor.Encoding = 4uL;
		templateDescriptor.IsCompressed = true;
		this._descriptors.Add(templateDescriptor.Name, templateDescriptor);

		DwgSectionDescriptorAC21 handlesDescriptor = new DwgSectionDescriptorAC21(DwgSectionDefinition.Handles);
		handlesDescriptor.HashCode = 1064174672uL;
		handlesDescriptor.DecompressedSize = 63488uL;
		handlesDescriptor.Encryption = 0uL;
		handlesDescriptor.Encoding = 4uL;
		handlesDescriptor.IsCompressed = true;
		this._descriptors.Add(handlesDescriptor.Name, handlesDescriptor);

		DwgSectionDescriptorAC21 classesDescriptor = new DwgSectionDescriptorAC21(DwgSectionDefinition.Classes);
		classesDescriptor.HashCode = 1062470751uL;
		classesDescriptor.DecompressedSize = 63488uL;
		classesDescriptor.Encryption = 0uL;
		classesDescriptor.Encoding = 4uL;
		classesDescriptor.IsCompressed = true;
		this._descriptors.Add(classesDescriptor.Name, classesDescriptor);

		DwgSectionDescriptorAC21 auxHeaderDescriptor = new DwgSectionDescriptorAC21(DwgSectionDefinition.AuxHeader);
		auxHeaderDescriptor.HashCode = 1425016074uL;
		auxHeaderDescriptor.DecompressedSize = 2048uL;
		auxHeaderDescriptor.Encryption = 0uL;
		auxHeaderDescriptor.Encoding = 4uL;
		auxHeaderDescriptor.IsCompressed = true;
		this._descriptors.Add(auxHeaderDescriptor.Name, auxHeaderDescriptor);

		DwgSectionDescriptorAC21 headerDescriptor = new DwgSectionDescriptorAC21(DwgSectionDefinition.Header);
		headerDescriptor.HashCode = 850920409uL;
		headerDescriptor.DecompressedSize = 2048uL;
		headerDescriptor.Encryption = 0uL;
		headerDescriptor.Encoding = 4uL;
		headerDescriptor.IsCompressed = true;
		this._descriptors.Add(headerDescriptor.Name, headerDescriptor);

		this.FileHeader.CompressedMetadata.SectionsAmount = (ulong)(this._descriptors.Count + 1);
	}

	private void reservePage(DwgLocalSectionMapAC21Base record, byte[] buffer, int offset, int length)
	{
		if (this._stream.Position % _pageAlignment != 0)
		{
			throw new InvalidOperationException($"Page position must be aligned to {_pageAlignment} bytes.");
		}

		long position = this._stream.Position;
		record.PageNumber = this._pageRecords.Count + 1;
		record.Seeker = (ulong)(position - _dataStartOffset);

		this._stream.Write(buffer, offset, length);

		int alignedLength = (length + _pageAlignment - 1) & ~(_pageAlignment - 1);
		this.writeRandomPadding(this._stream, (uint)(alignedLength - length));

		record.Size = this._stream.Position - position;
		this._pageRecords.Add(record);
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

	private void writeDescriptor(DwgSectionDescriptorAC21 descriptor, StreamIO writer)
	{
		//0x00	8	Data size
		writer.Write<ulong>(descriptor.CompressedSize);
		//0x08	8	Max size
		writer.Write<ulong>(descriptor.DecompressedSize);
		//0x10	8	Encryption
		writer.Write<ulong>(descriptor.Encryption);
		//0x18	8	HashCode
		writer.Write<ulong>(descriptor.HashCode ?? 0);

		ulong nameLength = (ulong)descriptor.Name.Length;
		if (nameLength != 0)
		{
			nameLength = nameLength + 1 << 1;
		}
		//0x20	8	SectionNameLength
		writer.Write<ulong>(nameLength);
		//0x28	8	Unknown
		writer.Write<ulong>(0UL);
		//0x30	8	Encoding
		writer.Write<ulong>(descriptor.Encoding ?? 0);
		//0x38	8	NumPages
		writer.Write<ulong>(descriptor.PageCount);

		if (nameLength != 0)
		{
			//Section name (Unicode)
			byte[] bytes = System.Text.Encoding.Unicode.GetBytes(descriptor.Name);
			writer.Stream.Write(bytes, 0, bytes.Length);
			writer.Write<byte>(0);
			writer.Write<byte>(0);
		}

		foreach (DwgLocalSectionMapAC21 page in descriptor.LocalSections)
		{
			//8	Page data offset
			writer.Write<ulong>(page.Offset);
			//8	Page Size
			writer.Write<ulong>(page.PageSize);
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

	private void writeDescriptors()
	{
		MemoryStream descriptorsStream = new MemoryStream();
		var writer = new StreamIO(descriptorsStream);
		writer.Encoding = this._encoding;

		foreach (var item in this._descriptors.Values)
		{
			if (item.CompressedSize != 0)
			{
				this.writeDescriptor(item, writer);
			}
		}

		DwgSectionDescriptorAC21 emptyDescriptor = new DwgSectionDescriptorAC21();
		emptyDescriptor.HashCode = 0uL;
		emptyDescriptor.DecompressedSize = 63488uL;
		emptyDescriptor.Encryption = 0uL;
		emptyDescriptor.Encoding = 4uL;
		this.writeDescriptor(emptyDescriptor, writer);

		DwgLocalSectionCompressedDataAC21 sectionsMapRecord = new DwgLocalSectionCompressedDataAC21();
		this.compressAndEncodePagesMap(pageSize: DwgChecksumCalculatorAC21.CalculatePageSize((ulong)descriptorsStream.Length), record: sectionsMapRecord, data: descriptorsStream,
			crcSeed: this.FileHeader.CompressedMetadata.SectionsMapCrcSeed, encodedData: out var encodedData);

		DwgLocalSectionCompressedDataAC21 sectionsMap2Record = (DwgLocalSectionCompressedDataAC21)sectionsMapRecord.Clone();
		this.reservePage(sectionsMapRecord, encodedData, 0, encodedData.Length);
		this.reservePage(sectionsMap2Record, encodedData, 0, encodedData.Length);

		this.FileHeader.CompressedMetadata.SectionsMapCorrectionFactor = sectionsMapRecord.PagesMapCorrectionFactor;
		this.FileHeader.CompressedMetadata.SectionsMapSizeCompressed = sectionsMapRecord.PagesMapSizeCompressed;
		this.FileHeader.CompressedMetadata.SectionsMapSizeUncompressed = sectionsMapRecord.PagesMapSizeUncompressed;
		this.FileHeader.CompressedMetadata.SectionsMapCrcCompressed = sectionsMapRecord.PagesMapCrcCompressed;
		this.FileHeader.CompressedMetadata.SectionsMapCrcUncompressed = sectionsMapRecord.PagesMapCrcUncompressed;
		this.FileHeader.CompressedMetadata.SectionsMapId = (ulong)sectionsMapRecord.PageNumber;
		this.FileHeader.CompressedMetadata.SectionsMap2Id = (ulong)sectionsMap2Record.PageNumber;
	}

	private void writeFileHeader()
	{
		this._stream.Position = this._stream.Length;
		this.FileHeader.CompressedMetadata.FileSize = (ulong)(this._stream.Length + _encodedHeaderSize);
		this.FileHeader.CompressedMetadata.Header2offset = (ulong)(this._stream.Position - _dataStartOffset);

		this.FileHeader.CompressedMetadata.SectionsMapCrcSeed = this._crcEncoding.EncodeValue((uint)this.FileHeader.CompressedMetadata.SectionsMapCrcSeed);
		this.FileHeader.CompressedMetadata.PagesMapCrcSeed = this._crcEncoding.EncodeValue((uint)this.FileHeader.CompressedMetadata.PagesMapCrcSeed);

		DwgCheckDataAC21 checkData = new DwgCheckDataAC21(this.FileHeader.CompressedMetadata.CrcSeed, this._crcEncoding);

		this.FileHeader.CompressedMetadata.CrcSeedEncoded = this._crcEncoding.EncodeValue((uint)(this.FileHeader.CompressedMetadata.CrcSeed & 0xFFFF));

		MemoryStream metadataStream = new MemoryStream();
		StreamIO metadataIO = new StreamIO(metadataStream);
		metadataIO.Encoding = this._encoding;
		this.writeCompressedMetadata(metadataIO);

		ulong CRC64 = CRC.Encode(
			metadataStream.GetBuffer(), 0u, (uint)metadataStream.Length,
			CRC.UpdateSeed2(0uL, (uint)metadataStream.Length));
		metadataStream.Position = 0x108L;

		//0x108	8	Header CRC64
		metadataIO.Write<ulong>(CRC64);

		MemoryStream compressed = new MemoryStream();
		new DwgLZ77AC21Compressor().Compress(metadataStream.GetBuffer(), 0, (int)metadataStream.Length, compressed);
		if (compressed.Length >= metadataStream.Length)
		{
			compressed = metadataStream;
		}

		ulong compressedCrc = CRC.Encode(
			compressed.GetBuffer(), 0u, (uint)compressed.Length,
			CRC.UpdateSeed2(0uL, (uint)compressed.Length));

		MemoryStream crcMemoryStream = new MemoryStream(12);
		StreamIO crcStreamIO = new StreamIO(crcMemoryStream);
		crcStreamIO.Encoding = this._encoding;

		ulong seed = this._crcEncoding.NextUInt64();
		crcStreamIO.Write<ulong>(seed);
		crcStreamIO.Write<ulong>(DwgCheckDataAC21.Encode(seed, seed));
		ulong crc = CRC.Encode(
			crcMemoryStream.GetBuffer(), 0u, (uint)crcMemoryStream.Length,
			CRC.UpdateSeed1(0uL, (uint)crcMemoryStream.Length));

		MemoryStream headerStream = new MemoryStream(1024);
		StreamIO headerStreamIO = new StreamIO(headerStream);
		headerStreamIO.Encoding = this._encoding;

		//0x00	8	CRC
		headerStreamIO.Write<ulong>(crc);
		//0x08	8	Unknown key
		headerStreamIO.Write<ulong>(seed);
		//0x10	8	Compressed Data CRC
		headerStreamIO.Write<ulong>(compressedCrc);

		if (compressed.Length < _metadataSize)
		{
			//0x18	4	ComprLen & 0x1C	4	Length2
			headerStreamIO.Write<long>(compressed.Length);
		}
		else if (compressed.Length == _metadataSize)
		{
			//0x18	4	ComprLen (negative = stored uncompressed)
			headerStreamIO.Write<long>(-_metadataSize);
		}
		else
		{
			throw new InvalidOperationException($"Compressed metadata size {compressed.Length} exceeds {_metadataSize}.");
		}

		//Position 32 = 0x20
		headerStream.Write(
			compressed.GetBuffer(), 0, (int)compressed.Length);

		uint encodedSize = ReedSolomon.Rs255_239.BlockSize * 3;
		uint headerLength = (uint)headerStreamIO.Stream.Length;
		uint alignedHeaderLength = (uint)alignTo8(headerLength);

		this.writeRandomPadding(headerStream, alignedHeaderLength - headerLength);
		uint repeatCount = encodedSize / alignedHeaderLength;
		for (int i = 1; i < repeatCount; i++)
		{
			headerStream.Write(headerStream.GetBuffer(), 0, (int)alignedHeaderLength);
		}

		this.writeRandomPadding(headerStream, encodedSize - (uint)(int)headerStream.Length);
		byte[] encodedHeader = new byte[_encodedHeaderSize];
		this.encodeInterleaved(encodedHeader, 0u, headerStream.GetBuffer(), 0u, (int)encodedSize, ReedSolomon.Rs255_239);
		this.fillRandomPadding(encodedHeader, 765u, 259u);
		MemoryStream encodedHeaderStream = new MemoryStream(encodedHeader, true);
		encodedHeaderStream.Position = encodedHeaderStream.Length - 40;
		checkData.Write(encodedHeaderStream);
		if (encodedHeaderStream.Length != _encodedHeaderSize)
		{
			throw new InvalidOperationException($"Encoded file header must be {_encodedHeaderSize} bytes.");
		}

		this._stream.Write(encodedHeader, 0, encodedHeader.Length);
		this._stream.Position = 0x80;
		this._stream.Write(encodedHeader, 0, encodedHeader.Length);
	}

	private void writeFileMetaData()
	{
		StreamIO writer = new StreamIO(this._stream);
		writer.Encoding = this._encoding;

		////0x00	6	“ACXXXX” version string
		this._stream.Position = 0L;
		this._stream.Write(Encoding.ASCII.GetBytes(this._document.Header.VersionString), 0, 6);

		//5 bytes of 0x00
		this._stream.Write(new byte[5], 0, 5);

		//0x0B	1	Maintenance release version
		this._stream.WriteByte((byte)this._document.Header.MaintenanceVersion);

		//0x0C	1	Byte 0x00, 0x01, or 0x03
		this._stream.WriteByte(3);

		//0x0D	4	Preview address(long), points to the image page + page header size(0x20).
		writer.Write<uint>((uint)((int)this._previewDescriptor.LocalSections[0].Seeker + _dataStartOffset));

		//0x11	1	Dwg version (Acad version that writes the file)
		this._stream.WriteByte(33);
		//0x12	1	Application maintenance release version(Acad maintenance version that writes the file)
		this._stream.WriteByte(0);

		//0x13	2	Codepage
		writer.Write<ushort>(getFileCodePage());
		//0x15	3	3 0x00 bytes
		this._stream.Write(new byte[3], 0, 3);

		//TODO: Write SecurityType
		//0x18	4	SecurityType (long), see R2004 meta data, the definition is the same, paragraph 4.1.
		writer.Write<int>(0);
		//0x1C	4	Unknown long
		writer.Write<int>(0);

		//0x20	4	Summary info Address in stream
		writer.Write<uint>((uint)((int)this._summaryInfoDescriptor.LocalSections[0].Seeker + _dataStartOffset));

		//0x24	4	VBA Project Addr(0 if not present)
		writer.Write<uint>(0u);

		//0x28	4	0x00000080
		writer.Write<int>(128);

		//0x2C	4	App info Address in stream
		writer.Write<uint>((uint)((int)this._appInfoDescriptor.LocalSections[0].Seeker + _dataStartOffset));

		//0x30	0x80	0x00 bytes
		for (int i = 0; i < 80; i++)
		{
			this._stream.WriteByte(0);
		}
	}

	private void writePagesMap()
	{
		this._pagesMapRecord.Seeker = 0uL;
		ulong pageSize = DwgChecksumCalculatorAC21.CalculatePageSize((ulong)(this._pageRecords.Count + 5) * 16uL);
		long nextPageId = this._pageRecords.Count + 1 + 2;
		if (pageSize <= 1024)
		{
			this._pageRecords[0].PageNumber = nextPageId++;
			this._pageRecords[1].PageNumber = nextPageId;
			this.FileHeader.CompressedMetadata.PagesMapId = (ulong)this._pageRecords[0].PageNumber;
			this.FileHeader.CompressedMetadata.PagesMapOffset = this._pageRecords[0].Seeker;
			this.FileHeader.CompressedMetadata.Map2Id = (ulong)this._pageRecords[1].PageNumber;
			this.FileHeader.CompressedMetadata.Map2Offset = this._pageRecords[1].Seeker;
		}
		else
		{
			this._pageRecords[0].PageNumber = -nextPageId++;
			this._pageRecords[1].PageNumber = -nextPageId++;
			uint streamOffset = (uint)(this._stream.Position - _dataStartOffset);
			DwgLocalSectionCompressedDataAC21 pagesMapPage = new DwgLocalSectionCompressedDataAC21();
			pagesMapPage.PageNumber = nextPageId++;
			pagesMapPage.Seeker = streamOffset - _dataStartOffset;
			this._pageRecords.Add(pagesMapPage);
			DwgLocalSectionCompressedDataAC21 pagesMap2Page = new DwgLocalSectionCompressedDataAC21();
			pagesMap2Page.PageNumber = nextPageId;
			pagesMap2Page.Seeker = streamOffset + pageSize - _dataStartOffset;
			this._pageRecords.Add(pagesMap2Page);
		}
		this.FileHeader.CompressedMetadata.PagesMaxId = 0uL;
		this.FileHeader.CompressedMetadata.PagesAmount = (uint)this._pageRecords.Count;
		this.FileHeader.CompressedMetadata.PagesMapOffset = this._pagesMapRecord.Seeker;

		MemoryStream pagesMapStream = new MemoryStream();
		StreamIO writer = new StreamIO(pagesMapStream);
		writer.Encoding = this._encoding;

		foreach (DwgLocalSectionMapAC21Base item in this._pageRecords)
		{
			if (item != null)
			{
				writer.Write<long>(item.Size);
				writer.Write<long>(item.PageNumber);
				if (item.PageNumber > (long)this.FileHeader.CompressedMetadata.PagesMaxId)
				{
					this.FileHeader.CompressedMetadata.PagesMaxId = (ulong)item.PageNumber;
				}
			}
		}

		this.compressAndEncodePagesMap(this._pagesMapRecord, pagesMapStream, this.FileHeader.CompressedMetadata.PagesMapCrcSeed, pageSize, out var encodedData);

		this._pagesMapRecord.Size = encodedData.Length;
		long map2PageNumber = this._map2Record.PageNumber;
		this._map2Record.Copy(this._pagesMapRecord);
		this._map2Record.PageNumber = map2PageNumber;
		this._map2Record.Seeker = 1024uL;

		this._stream.Position = (long)(this.FileHeader.CompressedMetadata.PagesMapOffset + _dataStartOffset);
		this._stream.Write(encodedData, 0, encodedData.Length);
		this._stream.Position = (long)(this.FileHeader.CompressedMetadata.Map2Offset + _dataStartOffset);
		this._stream.Write(encodedData, 0, encodedData.Length);

		this.FileHeader.CompressedMetadata.PagesMapCorrectionFactor = this._pagesMapRecord.PagesMapCorrectionFactor;
		this.FileHeader.CompressedMetadata.PagesMapSizeCompressed = this._pagesMapRecord.PagesMapSizeCompressed;
		this.FileHeader.CompressedMetadata.PagesMapSizeUncompressed = this._pagesMapRecord.PagesMapSizeUncompressed;
		this.FileHeader.CompressedMetadata.PagesMapCrcCompressed = this._pagesMapRecord.PagesMapCrcCompressed;
		this.FileHeader.CompressedMetadata.PagesMapCrcUncompressed = this._pagesMapRecord.PagesMapCrcUncompressed;
	}

	private void writeRandomPadding(Stream stream, uint length)
	{
		for (int i = 0; i < length; i++)
		{
			stream.WriteByte(this._crcEncoding.RandomBlock[i]);
		}
	}
}