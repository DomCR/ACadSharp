using CSUtilities.Converters;
using CSUtilities.IO;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgFileHeaderWriterAC18 : DwgFileHeaderWriterBase
	{
		public override int HandleSectionOffset { get { return 0; } }

		protected override int _fileHeaderSize { get { return 0x100; } }

		protected new DwgFileHeaderAC18 _fileHeader { get; } = new DwgFileHeaderAC18();

		protected virtual ICompressor compressor { get; } = new DwgLZ77AC18Compressor();

		private List<DwgLocalSectionMap> _localSectionsMaps = new List<DwgLocalSectionMap>();

		private Dictionary<string, DwgSectionDescriptor> _descriptors { get { return this._fileHeader.Descriptors; } }

		public DwgFileHeaderWriterAC18(Stream stream, CadDocument model) : base(stream, model)
		{
			// File header info
			for (int i = 0; i < this._fileHeaderSize; i++)
			{
				this._stream.WriteByte(0);
			}
		}

		public override void WriteFile()
		{
			this._fileHeader.SectionArrayPageSize = (uint)(this._localSectionsMaps.Count + 2);
			this._fileHeader.SectionPageMapId = this._fileHeader.SectionArrayPageSize;
			this._fileHeader.SectionMapId = this._fileHeader.SectionArrayPageSize - 1;

			this.writeDescriptors();

			this.writeRecords();

			this.writeFileMetaData();
		}

		public override void AddSection(string name, MemoryStream stream, bool isCompressed, int decompsize = 0x7400)
		{
			DwgSectionDescriptor descriptor = new DwgSectionDescriptor(name);
			this._fileHeader.AddSection(descriptor);
			descriptor.DecompressedSize = (ulong)decompsize;

			descriptor.CompressedSize = (ulong)stream.Length;
			descriptor.CompressedCode = ((!isCompressed) ? 1 : 2);

			int nlocalSections = (int)(stream.Length / (int)descriptor.DecompressedSize);

			byte[] buffer = stream.GetBuffer();
			ulong offset = 0uL;
			for (int i = 0; i < nlocalSections; i++)
			{
				this.craeteLocalSection(
					descriptor,
					buffer,
					(int)descriptor.DecompressedSize,
					offset,
					(int)descriptor.DecompressedSize,
					isCompressed);
				offset += (ulong)descriptor.DecompressedSize;
			}

			int spearBytes = (int)(stream.Length % (int)descriptor.DecompressedSize);
			if (spearBytes > 0 && !checkEmptyBytes(buffer, offset, (ulong)spearBytes))
			{
				this.craeteLocalSection(
					descriptor,
					buffer,
					(int)descriptor.DecompressedSize,
					offset,
					spearBytes,
					isCompressed);
			}
		}

		protected virtual void craeteLocalSection(DwgSectionDescriptor descriptor, byte[] buffer, int decompressedSize, ulong offset, int totalSize, bool isCompressed)
		{
			MemoryStream descriptorStream = this.applyCompression(buffer, decompressedSize, offset, totalSize, isCompressed);

			this.writeMagicNumber();

			//Save position for the local section
			long position = this._stream.Position;

			DwgLocalSectionMap localMap = new DwgLocalSectionMap();
			localMap.Offset = offset;
			localMap.Seeker = position;
			localMap.PageNumber = this._localSectionsMaps.Count + 1;
			localMap.ODA = DwgCheckSumCalculator.Calculate(0u, descriptorStream.GetBuffer(), 0, (int)descriptorStream.Length);

			int compressDiff = DwgCheckSumCalculator.CompressionCalculator((int)descriptorStream.Length);
			localMap.CompressedSize = (ulong)descriptorStream.Length;
			localMap.DecompressedSize = (ulong)totalSize;
			localMap.PageSize = (long)localMap.CompressedSize + 32 + compressDiff;
			localMap.Checksum = 0u;

			MemoryStream checkSumStream = new MemoryStream(32);
			this.writeDataSection(checkSumStream, descriptor, localMap, (int)descriptor.PageType);
			localMap.Checksum = DwgCheckSumCalculator.Calculate(localMap.ODA, checkSumStream.GetBuffer(), 0, (int)checkSumStream.Length);
			checkSumStream.SetLength(0L);
			checkSumStream.Position = 0L;

			this.writeDataSection(checkSumStream, descriptor, localMap, (int)descriptor.PageType);

			this.applyMask(checkSumStream.GetBuffer(), 0, (int)checkSumStream.Length);

			this._stream.Write(checkSumStream.GetBuffer(), 0, (int)checkSumStream.Length);
			this._stream.Write(descriptorStream.GetBuffer(), 0, (int)descriptorStream.Length);

			if (isCompressed)
			{
				this._stream.Write(DwgCheckSumCalculator.MagicSequence, 0, compressDiff);
			}
			else if (compressDiff != 0)
			{
				throw new Exception();
			}

			if (localMap.PageNumber > 0)
			{
				descriptor.PageCount++;
			}

			localMap.Size = this._stream.Position - position;
			descriptor.LocalSections.Add(localMap);
			this._localSectionsMaps.Add(localMap);
		}

		protected MemoryStream applyCompression(byte[] buffer, int decompressedSize, ulong offset, int totalSize, bool isCompressed)
		{
			MemoryStream stream = new MemoryStream();
			if (isCompressed)
			{
				MemoryStream holder = new MemoryStream(decompressedSize);
				holder.Write(buffer, (int)offset, totalSize);
				int diff = decompressedSize - totalSize;
				for (int i = 0; i < diff; i++)
				{
					holder.WriteByte(0);
				}

				this.compressor.Compress(holder.GetBuffer(), 0, decompressedSize, stream);
			}
			else
			{
				stream.Write(buffer, (int)offset, totalSize);
				int diff = decompressedSize - totalSize;
				for (int j = 0; j < diff; j++)
				{
					stream.WriteByte(0);
				}
			}

			return stream;
		}

		private void writeDescriptors()
		{
			MemoryStream stream = new MemoryStream();
			var swriter = DwgStreamWriterBase.GetStreamHandler(_version, stream, _encoding);

			//0x00	4	Number of section descriptions(NumDescriptions)
			swriter.WriteInt(this._descriptors.Count);

			//0x04	4	0x02 (long)
			swriter.WriteInt(2);
			//0x08	4	0x00007400 (long)
			swriter.WriteInt(0x7400);
			//0x0C	4	0x00 (long)
			swriter.WriteInt(0);

			//0x10	4	Unknown (long), ODA writes NumDescriptions here.
			swriter.WriteInt(this._descriptors.Count);
			foreach (var descriptors in this._descriptors.Values)
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
				//0x14	4	Compressed(1 = no, 2 = yes, normally 2)
				swriter.WriteInt(descriptors.CompressedCode);
				//0x18	4	Section Id(starts at 0). The first section(empty section) is numbered 0, consecutive sections are numbered descending from(the number of sections – 1) down to 1.
				swriter.WriteInt(descriptors.SectionId);
				//0x1C	4	Encrypted(0 = no, 1 = yes, 2 = unknown)
				swriter.WriteInt(descriptors.Encrypted);

				//0x20	64	Section Name(string)
				byte[] nameArr = new byte[64];
				if (!string.IsNullOrEmpty(descriptors.Name))
				{
					byte[] bytes = TextEncoding.Windows1252().GetBytes(descriptors.Name);
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
			DwgLocalSectionMap sectionHolder = this.setSeeker(0x4163003B, stream);
			int count = DwgCheckSumCalculator.CompressionCalculator((int)(this._stream.Position - sectionHolder.Seeker));
			// Fill the gap
			this._stream.Write(DwgCheckSumCalculator.MagicSequence, 0, count);
			sectionHolder.Size = this._stream.Position - sectionHolder.Seeker;

			this.addSection(sectionHolder);
		}

		public void writeRecords()
		{
			this.writeMagicNumber();

			//Section page map: 0x41630e3b
			DwgLocalSectionMap section = new DwgLocalSectionMap
			{
				SectionMap = 0x41630E3B
			};

			this.addSection(section);

			int counter = this._localSectionsMaps.Count * 8;
			section.Seeker = this._stream.Position;
			int size = counter + DwgCheckSumCalculator.CompressionCalculator(counter);
			section.Size = size;

			MemoryStream stream = new MemoryStream();
			StreamIO writer = new StreamIO(stream);

			foreach (DwgLocalSectionMap item in this._localSectionsMaps)
			{
				if (item != null)
				{
					//0x00	4	Section page number, starts at 1, page numbers are unique per file.
					writer.Write(item.PageNumber);
					//0x04	4	Section size
					writer.Write((int)item.Size);
				}
			}

			this.compressChecksum(section, stream);

			DwgLocalSectionMap last = this._localSectionsMaps[this._localSectionsMaps.Count - 1];
			this._fileHeader.GapAmount = 0u;
			this._fileHeader.LastPageId = last.PageNumber;
			this._fileHeader.LastSectionAddr = (ulong)(last.Seeker + size - 256);
			this._fileHeader.SectionAmount = (uint)(this._localSectionsMaps.Count - 1);
			this._fileHeader.PageMapAddress = (ulong)section.Seeker;
		}

		protected void writeFileMetaData()
		{
			StreamIO writer = new StreamIO(this._stream);

			this._fileHeader.SecondHeaderAddr = (ulong)this._stream.Position;

			MemoryStream stream = new MemoryStream();
			this.writeFileHeader(stream);

			this._stream.Write(stream.GetBuffer(), 0, (int)stream.Length);

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
			writer.Write((uint)((int)this._descriptors[DwgSectionDefinition.Preview].LocalSections[0].Seeker + 0x20));

			//0x11	1	Dwg version (Acad version that writes the file)
			this._stream.WriteByte((byte)33);
			//0x12	1	Application maintenance release version(Acad maintenance version that writes the file)
			this._stream.WriteByte((byte)_document.Header.MaintenanceVersion);

			//TODO: Write CodePage
			//0x13	2	Codepage
			writer.Write<ushort>(30);
			//0x15	3	3 0x00 bytes
			this._stream.Write(new byte[3], 0, 3);

			//TODO: Write SecurityType
			//0x18	4	SecurityType (long), see R2004 meta data, the definition is the same, paragraph 4.1.
			writer.Write((int)0);
			//0x1C	4	Unknown long
			writer.Write(0);

			//0x20	4	Summary info Address in stream
			writer.Write((uint)((int)this._descriptors[DwgSectionDefinition.SummaryInfo].LocalSections[0].Seeker + 0x20));

			//0x24	4	VBA Project Addr(0 if not present)
			writer.Write(0u);

			//0x28	4	0x00000080
			writer.Write<int>(0x00000080);

			//0x2C	4	App info Address in stream
			writer.Write((uint)((int)this._descriptors[DwgSectionDefinition.AppInfo].LocalSections[0].Seeker + 0x20));

			//0x30	0x80	0x00 bytes
			byte[] array = new byte[80];

			this._stream.Write(array, 0, 80);
			this._stream.Write(stream.GetBuffer(), 0, (int)stream.Length);
			this._stream.Write(DwgCheckSumCalculator.MagicSequence, 236, 20);
		}

		private void writeFileHeader(MemoryStream stream)
		{
			CRC32StreamHandler crcStream = new CRC32StreamHandler(stream, 0u);
			StreamIO swriter = new StreamIO(crcStream);

			//0x00	12	“AcFssFcAJMB” file ID string
			crcStream.Write(TextEncoding.Windows1252().GetBytes("AcFssFcAJMB"), 0, 11);
			crcStream.WriteByte(0);

			//0x0C	4	0x00(long)
			swriter.Write<int>(0);
			//0x10	4	0x6c(long)
			swriter.Write<int>(0x6C);
			//0x14	4	0x04(long)
			swriter.Write<int>(0x04);
			//0x18	4	Root tree node gap
			swriter.Write<int>(this._fileHeader.RootTreeNodeGap);
			//0x1C	4	Lowermost left tree node gap
			swriter.Write<int>(this._fileHeader.LeftGap);
			//0x20	4	Lowermost right tree node gap
			swriter.Write<int>(this._fileHeader.RigthGap);
			//0x24	4	Unknown long(ODA writes 1)	
			swriter.Write<int>(1);
			//0x28	4	Last section page Id
			swriter.Write<int>(this._fileHeader.LastPageId);

			//0x2C	8	Last section page end address
			swriter.Write<ulong>(this._fileHeader.LastSectionAddr);
			//0x34	8	Second header data address pointing to the repeated header data at the end of the file
			swriter.Write<ulong>(this._fileHeader.SecondHeaderAddr);

			//0x3C	4	Gap amount
			swriter.Write<uint>(this._fileHeader.GapAmount);
			//0x40	4	Section page amount
			swriter.Write<uint>(this._fileHeader.SectionAmount);

			//0x44	4	0x20(long)
			swriter.Write<int>(0x20);
			//0x48	4	0x80(long)
			swriter.Write<int>(0x80);
			//0x4C	4	0x40(long)
			swriter.Write<int>(0x40);

			//0x50	4	Section Page Map Id
			swriter.Write(this._fileHeader.SectionPageMapId);
			//0x54	8	Section Page Map address(add 0x100 to this value)
			swriter.Write<ulong>(this._fileHeader.PageMapAddress - 256);
			//0x5C	4	Section Map Id
			swriter.Write<uint>(this._fileHeader.SectionMapId);
			//0x60	4	Section page array size
			swriter.Write<uint>(this._fileHeader.SectionArrayPageSize);
			//0x64	4	Gap array size
			swriter.Write<uint>(this._fileHeader.GapArraySize);

			long position = crcStream.Position;
			swriter.Write<uint>(0u);

			uint seed = crcStream.Seed;
			crcStream.Position = position;
			//0x68	4	CRC32(long).See paragraph 2.14.2 for the 32 - bit CRC calculation, 
			//			the seed is zero. Note that the CRC 
			//			calculation is done including the 4 CRC bytes that are 
			//			initially zero! So the CRC calculation takes into account 
			//			all of the 0x6c bytes of the data in this table.
			swriter.Write<uint>(seed);

			crcStream.Flush();

			this.applyMagicSequence(stream);
		}

		private void addSection(DwgLocalSectionMap section)
		{
			section.PageNumber = this._localSectionsMaps.Count + 1;
			this._localSectionsMaps.Add(section);
		}

		private DwgLocalSectionMap setSeeker(int map, MemoryStream stream)
		{
			DwgLocalSectionMap holder = new DwgLocalSectionMap
			{
				SectionMap = map
			};

			this.writeMagicNumber();

			holder.Seeker = this._stream.Position;

			this.compressChecksum(holder, stream);

			return holder;
		}

		private void compressChecksum(DwgLocalSectionMap section, MemoryStream stream)
		{
			//Compress the local map section and write the checksum once is done
			section.DecompressedSize = (ulong)stream.Length;

			MemoryStream main = new MemoryStream();
			this.compressor.Compress(stream.GetBuffer(), 0, (int)stream.Length, main);

			section.CompressedSize = (ulong)main.Length;

			MemoryStream checkSumHolder = new MemoryStream();
			this.writePageHeaderData(section, checkSumHolder);
			section.Checksum = DwgCheckSumCalculator.Calculate(0u, checkSumHolder.GetBuffer(), 0, (int)checkSumHolder.Length);
			section.Checksum = DwgCheckSumCalculator.Calculate((uint)section.Checksum, main.GetBuffer(), 0, (int)main.Length);

			this.writePageHeaderData(section, this._stream);
			this._stream.Write(main.GetBuffer(), 0, (int)main.Length);
		}

		private void writePageHeaderData(DwgLocalSectionMap section, Stream stream)
		{
			StreamIO writer = new StreamIO(stream);

			//0x00	4	Section page type:
			//Section page map: 0x41630e3b
			//Section map: 0x4163003b
			writer.Write<int>(section.SectionMap);
			//0x04	4	Decompressed size of the data that follows
			writer.Write<int>((int)section.DecompressedSize);
			//0x08	4	Compressed size of the data that follows(CompDataSize)
			writer.Write<int>((int)section.CompressedSize);
			//0x0C	4	Compression type(0x02)
			writer.Write<int>(section.Compression);
			//0x10	4	Section page checksum
			writer.Write<uint>((uint)section.Checksum);
		}

		private void writeDataSection(Stream stream, DwgSectionDescriptor descriptor, DwgLocalSectionMap map, int size)
		{
			StreamIO writer = new StreamIO(stream);

			//0x00	4	Section page type, since it’s always a data section: 0x4163043b
			writer.Write(size);
			//0x04	4	Section number
			writer.Write(descriptor.SectionId);
			//0x08	4	Data size (compressed)
			writer.Write((int)map.CompressedSize);
			//0x0C	4	Page Size (decompressed)
			writer.Write((int)map.PageSize);
			//0x10	4	Start Offset (in the decompressed buffer)
			writer.Write((long)map.Offset);
			//0x18	4	Data Checksum (section page checksum calculated from compressed data bytes, with seed 0)
			writer.Write((uint)map.Checksum);
			//0x1C	4	Unknown (ODA writes a 0)
			writer.Write(map.ODA);
		}
	}
}
