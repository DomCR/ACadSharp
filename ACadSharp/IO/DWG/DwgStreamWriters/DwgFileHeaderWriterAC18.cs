﻿using CSUtilities.Converters;
using CSUtilities.IO;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters
{
	internal class DwgFileHeaderWriterAC18 : IDwgFileHeaderWriter
	{
		public ACadVersion _version;

		private Encoding _encoding;

		private Stream _stream;

		private DwgFileHeaderAC18 _fileHeader = new DwgFileHeaderAC18();

		private List<DwgLocalSectionMap> _localSectionsMaps = new List<DwgLocalSectionMap>();

		private CadDocument _document;

		private Dictionary<string, DwgSectionDescriptor> _descriptors { get { return this._fileHeader.Descriptors; } }

		public DwgFileHeaderWriterAC18(Stream stream, CadDocument model)
		{
			if (!stream.CanSeek || !stream.CanWrite)
			{
				throw new ArgumentException();
			}

			this._document = model;
			this._stream = stream;
			this._version = model.Header.Version;
			this._encoding = TextEncoding.Windows1252();
		}

		public void Init()
		{
			// File header info
			for (int i = 0; i < 0x100; i++)
			{
				this._stream.WriteByte(0);
			}

			if (this._version < ACadVersion.AC1032)
			{
				this._fileHeader.AddSection(DwgSectionDefinition.FileDepList);
			}

			this._fileHeader.AddSection(DwgSectionDefinition.AppInfo);
			this._fileHeader.AddSection(DwgSectionDefinition.Preview);
			this._fileHeader.AddSection(DwgSectionDefinition.SummaryInfo);
			this._fileHeader.AddSection(DwgSectionDefinition.RevHistory);
			this._fileHeader.AddSection(DwgSectionDefinition.AcDbObjects);
			this._fileHeader.AddSection(DwgSectionDefinition.ObjFreeSpace);
			this._fileHeader.AddSection(DwgSectionDefinition.Template);
			this._fileHeader.AddSection(DwgSectionDefinition.Handles);
			this._fileHeader.AddSection(DwgSectionDefinition.Classes);
			this._fileHeader.AddSection(DwgSectionDefinition.AuxHeader);
			this._fileHeader.AddSection(DwgSectionDefinition.Header);
		}

		public void WriteFile()
		{
			this._fileHeader.SectionArrayPageSize = (uint)(this._localSectionsMaps.Count + 2);
			this._fileHeader.GapArraySize = 0u;
			this._fileHeader.SectionPageMapId = this._fileHeader.SectionArrayPageSize;
			this._fileHeader.SectionMapId = this._fileHeader.SectionArrayPageSize - 1;

			this.writeDescriptors();

			this.WriteRecords();

			this.WriteFileMetaData();
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

			this.compressSection(section, stream);

			DwgLocalSectionMap last = this._localSectionsMaps[this._localSectionsMaps.Count - 1];
			this._fileHeader.LastPageId = last.PageNumber;
			this._fileHeader.LastSectionAddr = (ulong)(last.Seeker + size - 256);
			this._fileHeader.GapAmount = 0u;
			this._fileHeader.SectionAmount = (uint)(this._localSectionsMaps.Count - 1);
			this._fileHeader.PageMapAddress = (ulong)section.Seeker;
		}

		public void WriteFileMetaData()
		{
			StreamIO writer = new StreamIO(this._stream);

			this._fileHeader.SecondHeaderAddr = (ulong)this._stream.Position;

			MemoryStream fileHeaderStream = new MemoryStream();

			this.writeFileHeader(fileHeaderStream);

			this._stream.Write(fileHeaderStream.GetBuffer(), 0, (int)fileHeaderStream.Length);

			////0x00	6	“ACXXXX” version string
			this._stream.Position = 0L;
			this._stream.Write(Encoding.ASCII.GetBytes(this._document.Header.VersionString), 0, 6);

			//5 bytes of 0x00 
			this._stream.Write(new byte[5], 0, 5);

			if (this._document.Header.Version == ACadVersion.AC1018
				&& this._document.Header.MaintenanceVersion == 1)
			{
				throw new Exception();
			}
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
			writer.Write((uint)((int)this._descriptors[DwgSectionDefinition.SummaryInfo].LocalSections[0].Seeker + 32));

			//0x24	4	VBA Project Addr(0 if not present)
			writer.Write(0u);

			//0x28	4	0x00000080
			writer.Write<int>(0x00000080);

			//0x2C	4	App info Address in stream
			writer.Write((uint)((int)this._descriptors[DwgSectionDefinition.AppInfo].LocalSections[0].Seeker + 32));

			//0x30	0x80	0x00 bytes
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
			swriter.Write(0u);

			uint seed = crcStream.Seed;
			crcStream.Position = position;
			//0x68	4	CRC32(long).See paragraph 2.14.2 for the 32 - bit CRC calculation, 
			//			the seed is zero. Note that the CRC 
			//			calculation is done including the 4 CRC bytes that are 
			//			initially zero! So the CRC calculation takes into account 
			//			all of the 0x6c bytes of the data in this table.
			swriter.Write(seed);

			crcStream.Flush();

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
			section.PageNumber = this._localSectionsMaps.Count + 1;
			this._localSectionsMaps.Add(section);
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
			StreamIO writer = new StreamIO(stream);

			//0x00	4	Section page type:
			//Section page map: 0x41630e3b
			//Section map: 0x4163003b
			writer.Write(section.SectionMap);
			//0x04	4	Decompressed size of the data that follows
			writer.Write(section.Decompressed);
			//0x08	4	Compressed size of the data that follows(CompDataSize)
			writer.Write(section.CompDataSize);
			//0x0C	4	Compression type(0x02)
			writer.Write(section.Compression);
			//0x10	4	Section page checksum
			writer.Write(section.CheckSum);
		}

		public void CreateSection(string name, MemoryStream stream, bool isCompressed, int decompsize = 0x7400)
		{
			DwgSectionDescriptor descriptor = this._descriptors[name];
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
					0x4163043B,
					(int)descriptor.DecompressedSize,
					buffer,
					offset,
					(int)descriptor.DecompressedSize,
					isCompressed);
				offset += (ulong)descriptor.DecompressedSize;
			}

			int spearBytes = (int)(stream.Length % (int)descriptor.DecompressedSize);
			if (spearBytes > 0 && !checkEmptyBytes(buffer, offset, (ulong)spearBytes))
			{
				this.craeteLocalSection(descriptor, 0x4163043B, (int)descriptor.DecompressedSize, buffer, offset, spearBytes, isCompressed);
				nlocalSections++;
			}
		}

		public void CreateSection(DwgSectionDescriptor descriptor, MemoryStream stream, bool isCompressed)
		{
			descriptor.CompressedSize = (ulong)stream.Length;
			descriptor.CompressedCode = ((!isCompressed) ? 1 : 2);

			int nlocalSections = (int)(stream.Length / (int)descriptor.DecompressedSize);

			byte[] buffer = stream.GetBuffer();
			ulong offset = 0uL;
			for (int i = 0; i < nlocalSections; i++)
			{
				this.craeteLocalSection(
					descriptor,
					0x4163043B,
					(int)descriptor.DecompressedSize,
					buffer,
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
					0x4163043B,
					(int)descriptor.DecompressedSize,
					buffer,
					offset,
					spearBytes,
					isCompressed);
			}
		}

		public static bool checkEmptyBytes(byte[] buffer, ulong offset, ulong spearBytes)
		{
			bool result = true;
			ulong num = offset + spearBytes;

			for (ulong i = offset; i < num; i++)
			{
				if (buffer[i] != 0)
				{
					result = false;
					break;
				}
			}

			return result;
		}

		private void craeteLocalSection(DwgSectionDescriptor descriptor, int pageSize, int decompressedSize, byte[] buffer, ulong offset, int totalSize, bool isCompressed)
		{
			MemoryStream mainStream = new MemoryStream();

			if (isCompressed)
			{
				MemoryStream holder = new MemoryStream(decompressedSize);
				holder.Write(buffer, (int)offset, totalSize);
				int diff = decompressedSize - totalSize;
				for (int i = 0; i < diff; i++)
				{
					holder.WriteByte(0);
				}

				new DwgLZ77AC18Decompressor().Compress(holder.GetBuffer(), 0, decompressedSize, mainStream);
			}
			else
			{
				mainStream.Write(buffer, (int)offset, totalSize);
				int diff = decompressedSize - totalSize;
				for (int j = 0; j < diff; j++)
				{
					mainStream.WriteByte(0);
				}
			}

			this.writeMagicNumber();

			//Save position for the local section
			long position = this._stream.Position;

			DwgLocalSectionMap localMap = new DwgLocalSectionMap();
			localMap.PageNumber = this._localSectionsMaps.Count + 1;
			localMap.Offset = offset;
			localMap.Seeker = position;
			localMap.ODA = DwgCheckSumCalculator.Calculate(0u, mainStream.GetBuffer(), 0, (int)mainStream.Length);

			int compressDiff = DwgCheckSumCalculator.CompressionCalculator((int)mainStream.Length);
			localMap.CompressedSize = (ulong)mainStream.Length;
			localMap.DecompressedSize = (ulong)totalSize;
			localMap.PageSize = (long)localMap.CompressedSize + 32 + compressDiff;
			localMap.Checksum = 0u;

			MemoryStream checkSumStream = new MemoryStream(32);
			this.writeDataSection(checkSumStream, descriptor, localMap, pageSize);
			localMap.Checksum = DwgCheckSumCalculator.Calculate(localMap.ODA, checkSumStream.GetBuffer(), 0, (int)checkSumStream.Length);
			checkSumStream.SetLength(0L);
			checkSumStream.Position = 0L;

			this.writeDataSection(checkSumStream, descriptor, localMap, pageSize);

			this.applyMask(checkSumStream.GetBuffer(), 0, (int)checkSumStream.Length);

			this._stream.Write(checkSumStream.GetBuffer(), 0, (int)checkSumStream.Length);
			this._stream.Write(mainStream.GetBuffer(), 0, (int)mainStream.Length);

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

		private void applyMask(byte[] buffer, int offset, int length)
		{
			byte[] bytes = LittleEndianConverter.Instance.GetBytes(0x4164536B ^ (int)this._stream.Position);
			int diff = offset + length;
			while (offset < diff)
			{
				for (int i = 0; i < 4; i++)
				{
					buffer[offset + i] ^= bytes[i];
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