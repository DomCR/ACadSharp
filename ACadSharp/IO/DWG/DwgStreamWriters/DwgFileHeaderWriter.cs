using CSUtilities.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters
{
	internal abstract class DwgFileHeaderWriter
	{
	}

	internal class DwgFileHeaderWriterAC18
	{
		public Stream Stream;

		private ACadVersion _version;

		private List<DwgLocalSectionMap> _localSectionsMaps = new List<DwgLocalSectionMap>();

		DwgFileHeaderAC18 _fileHeader;

		public DwgFileHeaderWriterAC18(Stream stream, ACadVersion version)
		{
			this.Stream = stream;
			this._version = version;
		}

		public void CreateSection(string name, MemoryStream stream, bool isCompressed)
		{
			DwgSectionDescriptor descriptor = new DwgSectionDescriptor(name);
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
				offset += (ulong)descriptor.DecompressedSize;
			}

			//Check if there are spear bytes or the section is just too small to divide
			ulong spearBytes = (ulong)(stream.Length % (long)descriptor.DecompressedSize);
			if (spearBytes > 0)
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

			this._fileHeader.AddSection(name);
		}

		private void craeteLocalSection(DwgSectionDescriptor descriptor, long pageSize, int decompressedSize, byte[] buffer, ulong offset, ulong totalSize, bool isCompressed)
		{
			DwgLocalSectionMap localMap = new DwgLocalSectionMap();
			MemoryStream mainStream = new System.IO.MemoryStream();

			mainStream.Write(buffer, (int)offset, (int)totalSize);

			int diff = decompressedSize - (int)totalSize;
			for (int j = 0; j < diff; j++)
			{
				mainStream.WriteByte(0);
			}

			if (isCompressed)
			{
				System.IO.MemoryStream holder = new System.IO.MemoryStream(decompressedSize);
				holder.Write(mainStream.GetBuffer(), (int)offset, (int)totalSize);

				throw new NotImplementedException();
				//Dwg2004LZ77.Encode(holder.GetBuffer(), 0, decompressedSize, mainStream);
			}

			//Save position for the local section
			long position = this.Stream.Position;
			int compressDiff = DwgCheckSumCalculator.CompressionCalculator((int)mainStream.Length);

			localMap.PageNumber = this._localSectionsMaps.Count + 1;
			localMap.Offset = offset;
			localMap.Seeker = position;
			localMap.ODA = DwgCheckSumCalculator.Calculate(0u, mainStream.GetBuffer(), 0, (int)mainStream.Length);
			localMap.CompressedSize = (ulong)mainStream.Length;
			localMap.DecompressedSize = totalSize;
			localMap.PageSize = (long)localMap.CompressedSize + 32 + compressDiff;
			localMap.Checksum = 0u;

			System.IO.MemoryStream checkSumStream = new System.IO.MemoryStream(32);
			this.writeHeaderDataSection(checkSumStream, descriptor, localMap, (int)pageSize);
			localMap.Checksum = DwgCheckSumCalculator.Calculate(localMap.ODA, checkSumStream.GetBuffer(), 0, (int)checkSumStream.Length);
			checkSumStream.SetLength(0L);
			checkSumStream.Position = 0L;

			this.writeHeaderDataSection(checkSumStream, descriptor, localMap, (int)pageSize);

			this.applyMask(checkSumStream.GetBuffer(), 0, (int)checkSumStream.Length);

			this.Stream.Write(checkSumStream.GetBuffer(), 0, (int)checkSumStream.Length);
			this.Stream.Write(mainStream.GetBuffer(), 0, (int)mainStream.Length);

			if (isCompressed)
			{
				this.Stream.Write(DwgCheckSumCalculator.MagicSequence, 0, compressDiff);
			}
			else if (compressDiff != 0)
			{
				throw new System.Exception();
			}

			if (localMap.PageNumber > 0)
			{
				descriptor.PageCount++;
			}

			descriptor.LocalSections.Add(localMap);
			this._localSectionsMaps.Add(localMap);
		}

		private void writeHeaderDataSection(Stream stream, DwgSectionDescriptor descriptor, DwgLocalSectionMap map, int size)
		{
			var writer = DwgStreamWriterBase.GetStreamHandler(_version, stream, Encoding.Default);
			//0x00	4	Section page type, since it’s always a data section: 0x4163043b
			writer.WriteInt(size);
			//0x04	4	Section number
			writer.WriteInt(descriptor.SectionId);
			//0x08	4	Data size (compressed)
			writer.WriteInt((int)map.CompressedSize);
			//0x0C	4	Page Size (decompressed)
			writer.WriteInt((int)map.PageSize);
			//0x10	4	Start Offset (in the decompressed buffer)
			writer.WriteRawLong((long)map.Offset);
			//0x18	4	Data Checksum (section page checksum calculated from compressed data bytes, with seed 0)
			writer.WriteInt((int)map.Checksum);
			//0x1C	4	Unknown (ODA writes a 0)
			writer.WriteInt(0);
		}

		private void applyMask(byte[] buffer, int offset, int length)
		{
			byte[] bytes = LittleEndianConverter.Instance.GetBytes(0x4164536B ^ (int)this.Stream.Position);
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
	}
}
