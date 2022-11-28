using CSUtilities.Converters;
using CSUtilities.IO;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters
{
	internal class DwgFileHeaderWriterAC18
	{
		private Stream _stream;

		private CadDocument _document;

		private ACadVersion _version;

		private List<DwgLocalSectionMap> _localSectionMaps = new List<DwgLocalSectionMap>();

		private DwgFileHeaderAC18 _fileHeader;

		private Encoding _encoding;

		public DwgFileHeaderWriterAC18(CadDocument document, Stream stream, ACadVersion version)
		{
			this._document = document;
			this._stream = stream;
			this._version = version;
			this._encoding = TextEncoding.Windows1252();
			this._fileHeader = new DwgFileHeaderAC18();

			// File header info
			for (int i = 0; i < 0x100; i++)
			{
				this._stream.WriteByte(0);
			}
		}

		public void CreateSection(string name, MemoryStream stream, bool isCompressed, ulong decompsize = 0x7400)
		{
			DwgSectionDescriptor descriptor = new DwgSectionDescriptor(name);
			descriptor.DecompressedSize = decompsize;

			descriptor.CompressedSize = (ulong)stream.Length;
			descriptor.CompressedCode = isCompressed ? 2 : 1;

			int nlocalSections = (int)(stream.Length / (long)descriptor.DecompressedSize);

			byte[] buffer = stream.GetBuffer();
			ulong offset = 0uL;
			for (int i = 0; i < nlocalSections; i++)
			{
				this.craeteLocalSection(
					descriptor,
					descriptor.PageType,
					(int)descriptor.DecompressedSize,
					buffer,
					offset,
					descriptor.DecompressedSize,
					isCompressed);

				offset += descriptor.DecompressedSize;
			}

			//Check if there are spear bytes or the section is just too small to divide
			ulong spearBytes = (ulong)(stream.Length % (long)descriptor.DecompressedSize);
			if (spearBytes > 0 && !checkEmptyBytes(buffer, offset, spearBytes))
			{
				this.craeteLocalSection(
					descriptor,
					descriptor.PageType,
					(int)descriptor.DecompressedSize,
					buffer,
					offset,
					spearBytes,
					isCompressed);
			}

			this._fileHeader.AddSection(descriptor);
		}

		private static bool checkEmptyBytes(byte[] buffer, ulong offset, ulong spearBytes)
		{
			for (ulong i = offset; i < offset + spearBytes; i++)
			{
				if (buffer[i] != 0)
				{
					return false;
				}
			}

			return true;
		}

		public void WriteDescriptors()
		{
			this._localSectionMaps.Add(null);

			this._fileHeader.SectionArrayPageSize = (uint)(this._localSectionMaps.Count + 2);
			this._fileHeader.GapArraySize = 0u;
			this._fileHeader.SectionPageMapId = this._fileHeader.SectionArrayPageSize;
			this._fileHeader.SectionMapId = this._fileHeader.SectionArrayPageSize - 1;

			MemoryStream stream = new MemoryStream();
			//DwgStreamIO swriter = DwgStreamIO.CraeteStream(stream, this._version, this.Encoding);
			var swriter = DwgStreamWriterBase.GetStreamHandler(_version, stream, _encoding);

			//0x00	4	Number of section descriptions(NumDescriptions)
			swriter.WriteInt(this._fileHeader.Descriptors.Count);
			//0x04	4	0x02 (long)
			swriter.WriteInt(2);
			//0x08	4	0x00007400 (long)
			swriter.WriteInt(0x7400);
			swriter.WriteInt(0);
			swriter.WriteInt(this._fileHeader.Descriptors.Count);
			foreach (DwgSectionDescriptor descriptors in this._fileHeader.Descriptors.Values)
			{
				//0x00	8	Size of section(OdUInt64)
				swriter.WriteBytes(LittleEndianConverter.Instance.GetBytes(descriptors.CompressedSize));
				/*0x08	4	Page count(PageCount). Note that there can be more pages than PageCount,
							as PageCount is just the number of pages written to file.
							If a page contains zeroes only, that page is not written to file.
							These “zero pages” can be detected by checking if the page’s start 
							offset is bigger than it should be based on the sum of previously read pages 
							decompressed size(including zero pages).After reading all pages, if the total 
							decompressed size of the pages is not equal to the section’s size, add more zero 
							pages to the section until this condition is met.
				*/
				swriter.WriteInt(descriptors.PageCount);
				//0x0C	4	Max Decompressed Size of a section page of this type(normally 0x7400)
				swriter.WriteInt((int)descriptors.DecompressedSize);
				//0x10	4	Unknown(long)
				swriter.WriteInt(1);
				swriter.WriteInt(descriptors.CompressedCode);
				swriter.WriteInt(descriptors.SectionId);
				swriter.WriteInt(descriptors.Encrypted);

				//0x20	64	Section Name(string)
				byte[] nameArr = new byte[64];
				if (!string.IsNullOrEmpty(descriptors.Name))
				{
					byte[] bytes = Encoding.ASCII.GetBytes(descriptors.Name);
					int length = Math.Min(bytes.Length, nameArr.Length);
					for (int i = 0; i < length; i++)
					{
						nameArr[i] = bytes[i];
					}
				}
				stream.Write(nameArr, 0, nameArr.Length);

				foreach (DwgLocalSectionMap localMap in descriptors.LocalSections)
				{
					if (localMap.PageNumber > 0)
					{
						//0x00	4	Page number(index into SectionPageMap), starts at 1
						swriter.WriteInt(localMap.PageNumber);
						//0x04	4	Data size for this page(compressed size).
						swriter.WriteInt((int)localMap.CompressedSize);
						//0x08	8	Start offset for this page(OdUInt64).If this start offset is smaller than the sum of the decompressed size of all previous pages, then this page is to be preceded by zero pages until this condition is met.
						swriter.WriteBytes(LittleEndianConverter.Instance.GetBytes(localMap.Offset));
					}
				}
			}

			//Section map: 0x4163003b
			LocalSectionHolderAC18 sectionHolder = this.compressName(0x4163003B, stream);
			int count = DwgCheckSumCalculator.CompressionCalculator((int)(this._stream.Position - sectionHolder.Seeker));
			this._stream.Write(DwgCheckSumCalculator.MagicSequence, 0, count);
			sectionHolder.Size = this._stream.Position - sectionHolder.Seeker;

			this.addSection(sectionHolder);
		}

		public void WriteRecords()
		{
			this.writeMagicNumber();
			//Section page map: 0x41630e3b
			LocalSectionHolderAC18 section = new LocalSectionHolderAC18(0x41630E3B);
			this.addSection(section);

			int counter = this._localSectionMaps.Count * 8;
			section.Seeker = this._stream.Position;
			int size = counter + DwgCheckSumCalculator.CompressionCalculator(counter);
			section.Size = size;

			MemoryStream stream = new MemoryStream();
			StreamIO writer = new StreamIO(stream);

			foreach (DwgLocalSectionMap item in this._localSectionMaps)
			{
				if (item != null)
				{
					//0x00	4	Section page number, starts at 1, page numbers are unique per file.
					writer.Write(item.PageNumber);
					//0x04	4	Section size
					writer.Write((int)item.Size);
				}
			}

			this.compressSection(section, stream);

			DwgLocalSectionMap last = this._localSectionMaps[this._localSectionMaps.Count - 1];
			this._fileHeader.LastPageId = last.PageNumber;
			this._fileHeader.LastSectionAddr = (ulong)(last.Seeker + size - 256);
			this._fileHeader.GapAmount = 0u;
			this._fileHeader.SectionAmount = (uint)(this._localSectionMaps.Count - 1);
			this._fileHeader.PageMapAddress = (ulong)section.Seeker;
		}

		public void WriteFileMetaData()
		{
			var writer = DwgStreamWriterBase.GetStreamHandler(_version, this._stream, _encoding);

			this._fileHeader.SecondHeaderAddr = (ulong)this._stream.Position;

			MemoryStream fileHeaderStream = new MemoryStream();

			this.writeFileHeader(fileHeaderStream);

			this._stream.Write(fileHeaderStream.GetBuffer(), 0, (int)fileHeaderStream.Length);

			//0x00	6	“ACXXXX” version string
			this._stream.Position = 0L;
			this._stream.Write(Encoding.ASCII.GetBytes(this._document.Header.VersionString), 0, 6);

			//5 bytes of 0x00 
			this._stream.Write(new byte[5], 0, 5);

			//0x0B	1	Maintenance release version
			this._stream.WriteByte((byte)this._document.Header.MaintenanceVersion);

			//0x0C	1	Byte 0x00, 0x01, or 0x03
			this._stream.WriteByte(3);

			//0x0D	4	Preview address(long), points to the image page + page header size(0x20).
			writer.WriteInt((int)this._fileHeader.Descriptors[DwgSectionDefinition.Preview].LocalSections[0].Seeker + 0x20);

			//0x11	1	Dwg version (Acad version that writes the file)
			this._stream.WriteByte((byte)this._version);
			//0x12	1	Application maintenance release version(Acad maintenance version that writes the file)
			this._stream.WriteByte(0);

			//0x13	2	Codepage
			writer.WriteRawShort(30);

			this._stream.Write(new byte[3], 0, 3);

			//0x18	4	SecurityType (long), see R2004 meta data, the definition is the same, paragraph 4.1.
			writer.WriteInt(0);
			//0x1C	4	Unknown long
			writer.WriteInt(0);
			//0x20	4	Summary info Address in stream
			writer.WriteInt((int)this._fileHeader.Descriptors[DwgSectionDefinition.SummaryInfo].LocalSections[0].Seeker + 32);

			//0x24	4	VBA Project Addr(0 if not present)
			writer.WriteInt(0);

			//0x28	4	0x00000080
			writer.WriteInt(128);

			if (this._fileHeader.Descriptors.ContainsKey(DwgSectionDefinition.AppInfo))
			{
				writer.WriteInt((int)this._fileHeader.Descriptors[DwgSectionDefinition.AppInfo].LocalSections[0].Seeker + 32);
			}
			else
			{
				writer.WriteInt(0);
			}

			byte[] array = new byte[80];

			if (this._version > ACadVersion.AC1027)
			{
				//Observed while debugging the files
				array[1] = 13;
				array[4] = 2;
				array[8] = 51;
				array[12] = 4;
				array[16] = 4;
			}

			this._stream.Write(array, 0, 80);
			this._stream.Write(fileHeaderStream.GetBuffer(), 0, (int)fileHeaderStream.Length);
			this._stream.Write(DwgCheckSumCalculator.MagicSequence, 236, 20);
		}

		private void writeFileHeader(MemoryStream stream)
		{
			CRC32StreamHandler crc = new CRC32StreamHandler(stream, 0u);
			StreamIO stram = new StreamIO(crc);

			//0x00	12	“AcFssFcAJMB” file ID string
			crc.Write(TextEncoding.GetListedEncoding(CodePage.Windows1252).GetBytes("AcFssFcAJMB\0"), 0, 12);
			//0x0C	4	0x00(long)
			stram.Write(0, LittleEndianConverter.Instance);
			//0x10	4	0x6c(long)
			stram.Write(0x6c, LittleEndianConverter.Instance);
			//0x14	4	0x04(long)
			stram.Write(0x04, LittleEndianConverter.Instance);

			stram.Write(_fileHeader.RootTreeNodeGap, LittleEndianConverter.Instance);
			stram.Write(_fileHeader.LeftGap, LittleEndianConverter.Instance);
			stram.Write(_fileHeader.RigthGap, LittleEndianConverter.Instance);
			stram.Write(1, LittleEndianConverter.Instance);
			stram.Write(_fileHeader.LastPageId, LittleEndianConverter.Instance);

			//0x2C	8	Last section page end address
			stram.Write<ulong>(_fileHeader.LastSectionAddr, LittleEndianConverter.Instance);
			stram.Write<ulong>(_fileHeader.SecondHeaderAddr, LittleEndianConverter.Instance);
			stram.Write(_fileHeader.GapAmount, LittleEndianConverter.Instance);
			stram.Write(_fileHeader.SectionAmount, LittleEndianConverter.Instance);

			stram.Write(0x20, LittleEndianConverter.Instance);
			stram.Write(0x80, LittleEndianConverter.Instance);
			stram.Write(0x40, LittleEndianConverter.Instance);

			stram.Write(_fileHeader.SectionPageMapId, LittleEndianConverter.Instance);
			stram.Write<ulong>(_fileHeader.PageMapAddress - 256, LittleEndianConverter.Instance);
			stram.Write(_fileHeader.SectionMapId, LittleEndianConverter.Instance);
			stram.Write(_fileHeader.SectionArrayPageSize, LittleEndianConverter.Instance);
			stram.Write(_fileHeader.GapArraySize, LittleEndianConverter.Instance);

			long position = crc.Position;
			stram.Write(0u);

			uint seed = crc.Seed;
			crc.Position = position;
			stram.Write(seed);

			crc.Flush();

			this.applyMagicSequence(stream);
		}

		private void applyMagicSequence(MemoryStream stream)
		{
			byte[] buffer = stream.GetBuffer();
			for (int i = 0; i < (int)stream.Length; i++)
			{
				buffer[i] ^= DwgCheckSumCalculator.MagicSequence[i];
			}
		}

		private void addSection(LocalSectionHolderAC18 section)
		{
			section.PageNumber = this._localSectionMaps.Count + 1;
			this._localSectionMaps.Add(section);
		}

		private LocalSectionHolderAC18 compressName(int map, MemoryStream stream)
		{
			LocalSectionHolderAC18 holder = new LocalSectionHolderAC18(map);

			this.writeMagicNumber();

			holder.Seeker = this._stream.Position;

			this.compressSection(holder, stream);

			return holder;
		}

		private void compressSection(LocalSectionHolderAC18 section, MemoryStream stream)
		{
			section.Decompressed = (int)stream.Length;
			DwgLZ77AC18Decompressor compressor = new DwgLZ77AC18Decompressor();

			MemoryStream main = new MemoryStream();

			compressor.Compress(stream.GetBuffer(), 0, (int)stream.Length, main);

			section.CompDataSize = (int)main.Length;

			MemoryStream checkSumHolder = new MemoryStream();
			this.writePageHeaderData(section, checkSumHolder);
			section.CheckSum = DwgCheckSumCalculator.Calculate(0u, checkSumHolder.GetBuffer(), 0, (int)checkSumHolder.Length);
			section.CheckSum = DwgCheckSumCalculator.Calculate(section.CheckSum, main.GetBuffer(), 0, (int)main.Length);
			this.writePageHeaderData(section, this._stream);

			this._stream.Write(main.GetBuffer(), 0, (int)main.Length);
			this.writePageHeaderData(new LocalSectionHolderAC18(section.SectionMap), this._stream);
		}

		private void writePageHeaderData(LocalSectionHolderAC18 section, Stream stream)
		{
			var writer = DwgStreamWriterBase.GetStreamHandler(_version, stream, _encoding);

			//0x00	4	Section page type:
			//Section page map: 0x41630e3b
			//Section map: 0x4163003b
			writer.WriteInt(section.SectionMap);
			//0x04	4	Decompressed size of the data that follows
			writer.WriteInt(section.Decompressed);
			//0x08	4	Compressed size of the data that follows(CompDataSize)
			writer.WriteInt(section.CompDataSize);
			//0x0C	4	Compression type(0x02)
			writer.WriteInt(section.Compression);
			//0x10	4	Section page checksum
			writer.WriteBytes(LittleEndianConverter.Instance.GetBytes(section.CheckSum));
		}

		private void craeteLocalSection(DwgSectionDescriptor descriptor, long pageSize, int decompressedSize, byte[] buffer, ulong offset, ulong totalSize, bool isCompressed)
		{
			DwgLocalSectionMap localMap = new DwgLocalSectionMap();
			MemoryStream mainStream = new MemoryStream();
			int diff = decompressedSize - (int)totalSize;

			if (isCompressed)
			{
				MemoryStream holder = new MemoryStream(decompressedSize);
				holder.Write(buffer, (int)offset, (int)totalSize);
				for (int i = 0; i < diff; i++)
				{
					holder.WriteByte(0);
				}

				DwgLZ77AC18Decompressor compressor = new DwgLZ77AC18Decompressor();
				compressor.Compress(holder.GetBuffer(), 0, decompressedSize, mainStream);
			}
			else
			{
				mainStream.Write(buffer, (int)offset, (int)totalSize);
				for (int j = 0; j < diff; j++)
				{
					mainStream.WriteByte(0);
				}
			}

			this.writeMagicNumber();

			//Save position for the local section
			long position = this._stream.Position;

			localMap.PageNumber = this._localSectionMaps.Count + 1;
			localMap.Offset = offset;
			localMap.Seeker = position;
			//Just 0
			localMap.ODA = DwgCheckSumCalculator.Calculate(0u, mainStream.GetBuffer(), 0, (int)mainStream.Length);

			int compressDiff = DwgCheckSumCalculator.CompressionCalculator((int)mainStream.Length);
			localMap.CompressedSize = (ulong)mainStream.Length;
			localMap.DecompressedSize = totalSize;
			localMap.PageSize = (long)localMap.CompressedSize + 32 + compressDiff;
			localMap.Checksum = 0u;

			MemoryStream checkSumStream = new MemoryStream(32);
			this.writeDataSection(checkSumStream, descriptor, localMap, (int)pageSize);
			localMap.Checksum = DwgCheckSumCalculator.Calculate(localMap.ODA, checkSumStream.GetBuffer(), 0, (int)checkSumStream.Length);
			checkSumStream.SetLength(0L);
			checkSumStream.Position = 0L;

			this.writeDataSection(checkSumStream, descriptor, localMap, (int)pageSize);

			this.applyMask(checkSumStream.GetBuffer(), 0, (int)checkSumStream.Length);

			this._stream.Write(checkSumStream.GetBuffer(), 0, (int)checkSumStream.Length);
			this._stream.Write(mainStream.GetBuffer(), 0, (int)mainStream.Length);

			if (isCompressed)
			{
				this._stream.Write(DwgCheckSumCalculator.MagicSequence, 0, compressDiff);
			}
			else if (compressDiff != 0)
			{
				throw new System.Exception();
			}

			if (localMap.PageNumber > 0)
			{
				descriptor.PageCount++;
			}

			localMap.Size = this._stream.Position - position;
			descriptor.LocalSections.Add(localMap);
			this._localSectionMaps.Add(localMap);
		}

		private void writeDataSection(Stream stream, DwgSectionDescriptor descriptor, DwgLocalSectionMap map, int size)
		{
			var writer = DwgStreamWriterBase.GetStreamHandler(_version, stream, _encoding);
			//0x00	4	Section page type, since it’s always a data section: 0x4163043b
			writer.WriteInt(size);
			//0x04	4	Section number
			writer.WriteInt(descriptor.SectionId);
			//0x08	4	Data size (compressed)
			writer.WriteInt((int)map.CompressedSize);
			//0x0C	4	Page Size (decompressed)
			writer.WriteInt((int)map.PageSize);
			//0x10	4	Start Offset (in the decompressed buffer)
			writer.WriteBytes(LittleEndianConverter.Instance.GetBytes(map.Offset));
			//0x18	4	Data Checksum (section page checksum calculated from compressed data bytes, with seed 0)
			writer.WriteInt((int)map.Checksum);
			//0x1C	4	Unknown (ODA writes a 0)
			writer.WriteInt(0);
		}

		private void applyMask(byte[] buffer, int offset, int length)
		{
			byte[] secMask = LittleEndianConverter.Instance.GetBytes(0x4164536B ^ (int)this._stream.Position);
			int diff = offset + length;
			while (offset < diff)
			{
				for (int i = 0; i < 4; i++)
				{
					buffer[offset + i] ^= secMask[i];
				}

				offset += 4;
			}
		}

		private void writeMagicNumber()
		{
			for (int i = 0; i < (int)(this._stream.Position % 0x20); i++)
			{
				this._stream.WriteByte(DwgCheckSumCalculator.MagicSequence[i]);
			}
		}

		[Obsolete("Use the LocalSectionMap fields")]
		internal class LocalSectionHolderAC18 : DwgLocalSectionMap
		{
			public int SectionMap { get; set; }

			public int Decompressed { get; set; }

			public int CompDataSize { get; set; }

			public int Compression { get; set; } = 2;

			public uint CheckSum { get; set; }

			public LocalSectionHolderAC18(int value)
			{
				this.SectionMap = value;
			}
		}
	}
}
