using ACadSharp.Exceptions;
using CSUtilities.Converters;
using CSUtilities.IO;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACadSharp.IO.DWG
{
	internal class DwgFileHeaderReader : DwgSectionIO
	{
		private const int _metaDataSize = 0x7A;

		private const int _encriptedDataSize = 0x6C;

		public override string SectionName { get { return string.Empty; } }

		private StreamIO _fileStream;

		public DwgFileHeaderReader(Stream stream) : base(ACadVersion.Unknown)
		{
			this._fileStream = new StreamIO(stream);
		}

		public DwgFileHeader Read()
		{
			this._fileStream.Position = 0;

			//0x00	6	“ACXXXX” version string
			byte[] buffer = new byte[6];
			this._fileStream.Stream.Read(buffer, 0, buffer.Length);
			this.updateFileVersion(buffer);

			DwgFileHeader fileHeader = DwgFileHeader.CreateFileHeader(this._version);

			//Get the stream reader
			IDwgStreamReader sreader = DwgStreamReaderBase.GetStreamHandler(this._version, _fileStream.Stream);

			//Read the file header
			switch (fileHeader.AcadVersion)
			{
				case ACadVersion.Unknown:
					throw new CadNotSupportedException();
				case ACadVersion.MC0_0:
				case ACadVersion.AC1_2:
				case ACadVersion.AC1_4:
				case ACadVersion.AC1_50:
				case ACadVersion.AC2_10:
				case ACadVersion.AC1002:
				case ACadVersion.AC1003:
				case ACadVersion.AC1004:
				case ACadVersion.AC1006:
				case ACadVersion.AC1009:
					throw new CadNotSupportedException(this._version);
				case ACadVersion.AC1012:
				case ACadVersion.AC1014:
				case ACadVersion.AC1015:
					this.readFileHeaderAC15(fileHeader as DwgFileHeaderAC15, sreader);
					break;
				case ACadVersion.AC1018:
					this.readFileHeaderAC18(fileHeader as DwgFileHeaderAC18, sreader);
					break;
				case ACadVersion.AC1021:
					this.readFileHeaderAC21(fileHeader as DwgFileHeaderAC21, sreader);
					break;
				case ACadVersion.AC1024:
				case ACadVersion.AC1027:
				case ACadVersion.AC1032:
					//Check if it works...
					this.readFileHeaderAC18(fileHeader as DwgFileHeaderAC18, sreader);
					break;
			}

			return fileHeader;
		}

		public async Task<DwgFileHeader> ReadAsync(CancellationToken cancellationToken = default)
		{
			this._fileStream.Position = 0;

			//0x00	6	“ACXXXX” version string
			byte[] buffer = new byte[6];
			await this._fileStream.Stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
			this.updateFileVersion(buffer);

			DwgFileHeader fileHeader = DwgFileHeader.CreateFileHeader(this._version);

			//Read the file header
			switch (fileHeader.AcadVersion)
			{
				case ACadVersion.Unknown:
					throw new CadNotSupportedException();
				case ACadVersion.MC0_0:
				case ACadVersion.AC1_2:
				case ACadVersion.AC1_4:
				case ACadVersion.AC1_50:
				case ACadVersion.AC2_10:
				case ACadVersion.AC1002:
				case ACadVersion.AC1003:
				case ACadVersion.AC1004:
				case ACadVersion.AC1006:
				case ACadVersion.AC1009:
					throw new CadNotSupportedException(this._version);
				case ACadVersion.AC1012:
				case ACadVersion.AC1014:
				case ACadVersion.AC1015:
					IDwgStreamReader sreader = DwgStreamReaderBase.GetStreamHandler(_version, await getHeaderAC15Stream(cancellationToken));
					this.readFileHeaderAC15(fileHeader as DwgFileHeaderAC15, sreader);
					break;
				case ACadVersion.AC1018:
					await this.readFileHeaderAC18Async(fileHeader as DwgFileHeaderAC18, cancellationToken);
					break;
				case ACadVersion.AC1021:
					await this.readFileHeaderAC21Async(fileHeader as DwgFileHeaderAC21, cancellationToken);
					break;
				case ACadVersion.AC1024:
				case ACadVersion.AC1027:
				case ACadVersion.AC1032:
					await this.readFileHeaderAC18Async(fileHeader as DwgFileHeaderAC18, cancellationToken);
					break;
			}

			return fileHeader;
		}

		private void readFileHeaderAC15(DwgFileHeaderAC15 fileheader, IDwgStreamReader sreader)
		{
			//The next 7 starting at offset 0x06 are to be six bytes of 0 
			//(in R14, 5 0’s and the ACADMAINTVER variable) and a byte of 1.
			sreader.ReadBytes(7);
			//At 0x0D is a seeker (4 byte long absolute address) for the beginning sentinel of the image data.
			fileheader.PreviewAddress = sreader.ReadInt();

			//Undocumented Bytes at 0x11 and 0x12
			sreader.ReadBytes(2);

			//Bytes at 0x13 and 0x14 are a raw short indicating the value of the code page for this drawing file.
			fileheader.DrawingCodePage = CadUtils.GetCodePage(sreader.ReadShort());

			//At 0x15 is a long that tells how many sets of recno/seeker/length records follow.
			int nRecords = (int)sreader.ReadRawLong();
			for (int i = 0; i < nRecords; ++i)
			{
				//Record number (raw byte) | Seeker (raw long) | Size (raw long)
				DwgSectionLocatorRecord record = new DwgSectionLocatorRecord();
				record.Number = sreader.ReadByte();
				record.Seeker = sreader.ReadRawLong();
				record.Size = sreader.ReadRawLong();

				fileheader.Records.Add(record.Number.Value, record);
			}

			//RS : CRC for BOF to this point.
			sreader.ReadCRC();

			var sn = sreader.ReadSentinel();
			if (!CheckSentinel(sn, DwgFileHeaderAC15.EndSentinel))
			{
				this.notify($"Invalid section sentinel found in FileHeader", NotificationType.Warning);
			}
		}

		private void readFileHeaderAC18(DwgFileHeaderAC18 fileheader, IDwgStreamReader sreader)
		{
			this.readFileMetaData(fileheader, sreader);

			//0x80	0x6C	Encrypted Data
			//Metadata:
			//The encrypted data at 0x80 can be decrypted by exclusive or’ing the 0x6c bytes of data 
			//from the file with the following magic number sequence:

			//29 23 BE 84 E1 6C D6 AE 52 90 49 F1 F1 BB E9 EB
			//B3 A6 DB 3C 87 0C 3E 99 24 5E 0D 1C 06 B7 47 DE
			//B3 12 4D C8 43 BB 8B A6 1F 03 5A 7D 09 38 25 1F
			//5D D4 CB FC 96 F5 45 3B 13 0D 89 0A 1C DB AE 32
			//20 9A 50 EE 40 78 36 FD 12 49 32 F6 9E 7D 49 DC
			//AD 4F 14 F2 44 40 66 D0 6B C4 30 B7

			StreamIO headerStream = new StreamIO(new CRC32StreamHandler(sreader.ReadBytes(0x6C), 0U)); //108
			headerStream.Encoding = TextEncoding.GetListedEncoding(CodePage.Windows1252);

			#region Read header encrypted data

			//0x00	12	“AcFssFcAJMB” file ID string
			string fileId = headerStream.ReadString(12);
			if (fileId != "AcFssFcAJMB\0")
			{
				this.notify($"File validation failed, id should be : AcFssFcAJMB\0, but is : {fileId}", NotificationType.Warning);
			}

			//0x0C	4	0x00(long)
			headerStream.ReadInt<LittleEndianConverter>();
			//0x10	4	0x6c(long)
			headerStream.ReadInt<LittleEndianConverter>();
			//0x14	4	0x04(long)
			headerStream.ReadInt<LittleEndianConverter>();
			//0x18	4	Root tree node gap
			fileheader.RootTreeNodeGap = headerStream.ReadInt<LittleEndianConverter>();
			//0x1C	4	Lowermost left tree node gap
			fileheader.LeftGap = headerStream.ReadInt<LittleEndianConverter>();
			//0x20	4	Lowermost right tree node gap
			fileheader.RigthGap = headerStream.ReadInt<LittleEndianConverter>();
			//0x24	4	Unknown long(ODA writes 1)	
			headerStream.ReadInt<LittleEndianConverter>();
			//0x28	4	Last section page Id
			fileheader.LastPageId = headerStream.ReadInt<LittleEndianConverter>();

			//0x2C	8	Last section page end address
			fileheader.LastSectionAddr = headerStream.ReadULong<LittleEndianConverter>();
			//0x34	8	Second header data address pointing to the repeated header data at the end of the file
			fileheader.SecondHeaderAddr = headerStream.ReadULong<LittleEndianConverter>();

			//0x3C	4	Gap amount
			fileheader.GapAmount = headerStream.ReadUInt<LittleEndianConverter>();
			//0x40	4	Section page amount
			fileheader.SectionAmount = headerStream.ReadUInt<LittleEndianConverter>();
			//0x44	4	0x20(long)
			headerStream.ReadInt<LittleEndianConverter>();
			//0x48	4	0x80(long)
			headerStream.ReadInt<LittleEndianConverter>();
			//0x4C	4	0x40(long)
			headerStream.ReadInt<LittleEndianConverter>();
			//0x50	4	Section Page Map Id
			fileheader.SectionPageMapId = headerStream.ReadUInt<LittleEndianConverter>();
			//0x54	8	Section Page Map address(add 0x100 to this value)
			fileheader.PageMapAddress = headerStream.ReadULong<LittleEndianConverter>() + 256UL;
			//0x5C	4	Section Map Id
			fileheader.SectionMapId = headerStream.ReadUInt<LittleEndianConverter>();
			//0x60	4	Section page array size
			fileheader.SectionArrayPageSize = headerStream.ReadUInt<LittleEndianConverter>();
			//0x64	4	Gap array size
			fileheader.GapArraySize = headerStream.ReadUInt<LittleEndianConverter>();
			//0x68	4	CRC32(long).See paragraph 2.14.2 for the 32 - bit CRC calculation, 
			//			the seed is zero. Note that the CRC 
			//			calculation is done including the 4 CRC bytes that are 
			//			initially zero! So the CRC calculation takes into account 
			//			all of the 0x6c bytes of the data in this table.
			fileheader.CRCSeed = headerStream.ReadUInt();
			#endregion

			#region Read page map of the file
			sreader.Position = (long)fileheader.PageMapAddress;

			//Get the page size
			this.getPageHeaderData(sreader, out _, out long decompressedSize, out _, out _, out _);
			//Get the descompressed stream to read the records
			StreamIO decompressed = new StreamIO(DwgLZ77AC18Decompressor.Decompress(sreader.Stream, decompressedSize));

			//Section size
			int total = 0x100;
			while (decompressed.Position < decompressed.Length)
			{
				DwgSectionLocatorRecord record = new DwgSectionLocatorRecord();
				//0x00	4	Section page number, starts at 1, page numbers are unique per file.
				record.Number = decompressed.ReadInt();
				//0x04	4	Section size
				record.Size = decompressed.ReadInt();

				if (record.Number >= 0)
				{
					record.Seeker = total;
					fileheader.Records.Add(record.Number.Value, record);
				}
				else
				{
					//If the section number is negative, this represents a gap in the sections (unused data). 
					//For a negative section number, the following data will be present after the section size:

					//0x00	4	Parent
					decompressed.ReadInt();
					//0x04	4	Left
					decompressed.ReadInt();
					//0x08	4	Right
					decompressed.ReadInt();
					//0x0C	4	0x00
					decompressed.ReadInt();
				}

				total += (int)record.Size;
			}
			#endregion

			#region Read the data section map
			//Set the positon of the map
			sreader.Position = fileheader.Records[(int)fileheader.SectionMapId].Seeker;
			//84244
			//Get the page size
			this.getPageHeaderData(sreader, out _, out decompressedSize, out _, out _, out _);
			//1556
			StreamIO decompressedStream = new StreamIO(DwgLZ77AC18Decompressor.Decompress(sreader.Stream, decompressedSize));
			decompressedStream.Encoding = TextEncoding.GetListedEncoding(CodePage.Windows1252);

			//0x00	4	Number of section descriptions(NumDescriptions)
			int ndescriptions = decompressedStream.ReadInt<LittleEndianConverter>();
			//0x04	4	0x02 (long)
			decompressedStream.ReadInt<LittleEndianConverter>();
			//0x08	4	0x00007400 (long)
			decompressedStream.ReadInt<LittleEndianConverter>();
			//0x0C	4	0x00 (long)
			decompressedStream.ReadInt<LittleEndianConverter>();
			//0x10	4	Unknown (long), ODA writes NumDescriptions here.
			decompressedStream.ReadInt<LittleEndianConverter>();

			for (int i = 0; i < ndescriptions; ++i)
			{
				DwgSectionDescriptor descriptor = new DwgSectionDescriptor();
				//0x00	8	Size of section(OdUInt64)
				descriptor.CompressedSize = decompressedStream.ReadULong();
				/*0x08	4	Page count(PageCount). Note that there can be more pages than PageCount,
							as PageCount is just the number of pages written to file.
							If a page contains zeroes only, that page is not written to file.
							These “zero pages” can be detected by checking if the page’s start 
							offset is bigger than it should be based on the sum of previously read pages 
							decompressed size(including zero pages).After reading all pages, if the total 
							decompressed size of the pages is not equal to the section’s size, add more zero 
							pages to the section until this condition is met.
				*/
				descriptor.PageCount = decompressedStream.ReadInt<LittleEndianConverter>();
				//0x0C	4	Max Decompressed Size of a section page of this type(normally 0x7400)
				descriptor.DecompressedSize = (ulong)decompressedStream.ReadInt<LittleEndianConverter>();
				//0x10	4	Unknown(long)
				decompressedStream.ReadInt<LittleEndianConverter>();
				//0x14	4	Compressed(1 = no, 2 = yes, normally 2)
				descriptor.CompressedCode = decompressedStream.ReadInt<LittleEndianConverter>();
				//0x18	4	Section Id(starts at 0). The first section(empty section) is numbered 0, consecutive sections are numbered descending from(the number of sections – 1) down to 1.
				descriptor.SectionId = decompressedStream.ReadInt<LittleEndianConverter>();
				//0x1C	4	Encrypted(0 = no, 1 = yes, 2 = unknown)
				descriptor.Encrypted = decompressedStream.ReadInt<LittleEndianConverter>();
				//0x20	64	Section Name(string)
				descriptor.Name = decompressedStream.ReadString(64).Split('\0')[0];

				//Following this, the following (local) section page map data will be present
				for (int j = 0; j < descriptor.PageCount; ++j)
				{
					DwgLocalSectionMap localmap = new DwgLocalSectionMap();
					//0x00	4	Page number(index into SectionPageMap), starts at 1
					localmap.PageNumber = decompressedStream.ReadInt<LittleEndianConverter>();
					//0x04	4	Data size for this page(compressed size).
					localmap.CompressedSize = (ulong)decompressedStream.ReadInt<LittleEndianConverter>();
					//0x08	8	Start offset for this page(OdUInt64).If this start offset is smaller than the sum of the decompressed size of all previous pages, then this page is to be preceded by zero pages until this condition is met.
					localmap.Offset = decompressedStream.ReadULong();

					//same decompressed size and seeker (temporal values)
					localmap.DecompressedSize = descriptor.DecompressedSize;
					localmap.Seeker = fileheader.Records[localmap.PageNumber].Seeker;

					//Maximum section page size appears to be 0x7400 bytes in the normal case.
					//If a logical section of the file (the database objects, for example) exceeds this size, then it is broken up into pages of size 0x7400.

					descriptor.LocalSections.Add(localmap);
				}

				//Get the final size for the local section
				uint sizeLeft = (uint)(descriptor.CompressedSize % descriptor.DecompressedSize);
				if (sizeLeft > 0U && descriptor.LocalSections.Count > 0)
					descriptor.LocalSections[descriptor.LocalSections.Count - 1].DecompressedSize = sizeLeft;

				fileheader.Descriptors.Add(descriptor.Name, descriptor);
			}
			#endregion
		}

		private async Task readFileHeaderAC18Async(DwgFileHeaderAC18 fileheader, CancellationToken cancellationToken = default)
		{
			this.readFileMetaData(fileheader, await this.getStreamChunkAsync(_metaDataSize, cancellationToken));

			this.readEncriptedData(fileheader, await this.getStreamChunkAsync(_encriptedDataSize, cancellationToken));

			_fileStream.Position = (long)fileheader.PageMapAddress;

			//Get the page size
			this.getPageHeaderData(await this.getStreamChunkAsync(0x14, cancellationToken)
				, out _, out long decompressedSize, out long compressedSize, out _, out _);

			//Get the descompressed stream to read the records
			IDwgStreamReader compressed = await this.getStreamChunkAsync((int)compressedSize, cancellationToken);

			this.readRecords(fileheader, compressed.Stream, decompressedSize);

			this._fileStream.Position = fileheader.Records[(int)fileheader.SectionMapId].Seeker;
			//Get the page size
			this.getPageHeaderData(await this.getStreamChunkAsync(0x14, cancellationToken)
				, out _, out decompressedSize, out compressedSize, out _, out _);

			compressed = await this.getStreamChunkAsync((int)compressedSize, cancellationToken);

			this.readDescriptors(fileheader, compressed.Stream, decompressedSize);
		}

		private void readRecords(DwgFileHeaderAC18 fileheader, Stream compressed, long decompressedSize)
		{
			StreamIO decompressed = new StreamIO(DwgLZ77AC18Decompressor.Decompress(compressed, decompressedSize));

			//Section size
			int total = 0x100;
			while (decompressed.Position < decompressed.Length)
			{
				DwgSectionLocatorRecord record = new DwgSectionLocatorRecord();
				//0x00	4	Section page number, starts at 1, page numbers are unique per file.
				record.Number = decompressed.ReadInt();
				//0x04	4	Section size
				record.Size = decompressed.ReadInt();

				if (record.Number >= 0)
				{
					record.Seeker = total;
					fileheader.Records.Add(record.Number.Value, record);
				}
				else
				{
					//If the section number is negative, this represents a gap in the sections (unused data). 
					//For a negative section number, the following data will be present after the section size:

					//0x00	4	Parent
					decompressed.ReadInt();
					//0x04	4	Left
					decompressed.ReadInt();
					//0x08	4	Right
					decompressed.ReadInt();
					//0x0C	4	0x00
					decompressed.ReadInt();
				}

				total += (int)record.Size;
			}
		}

		private void readDescriptors(DwgFileHeaderAC18 fileheader, Stream compressed, long decompressedSize)
		{
			StreamIO decompressed = new StreamIO(DwgLZ77AC18Decompressor.Decompress(compressed, decompressedSize));
			decompressed.Encoding = TextEncoding.GetListedEncoding(CodePage.Windows1252);

			//0x00	4	Number of section descriptions(NumDescriptions)
			int ndescriptions = decompressed.ReadInt<LittleEndianConverter>();
			//0x04	4	0x02 (long)
			decompressed.ReadInt<LittleEndianConverter>();
			//0x08	4	0x00007400 (long)
			decompressed.ReadInt<LittleEndianConverter>();
			//0x0C	4	0x00 (long)
			decompressed.ReadInt<LittleEndianConverter>();
			//0x10	4	Unknown (long), ODA writes NumDescriptions here.
			decompressed.ReadInt<LittleEndianConverter>();

			for (int i = 0; i < ndescriptions; ++i)
			{
				DwgSectionDescriptor descriptor = new DwgSectionDescriptor();
				//0x00	8	Size of section(OdUInt64)
				descriptor.CompressedSize = decompressed.ReadULong();
				/*0x08	4	Page count(PageCount). Note that there can be more pages than PageCount,
							as PageCount is just the number of pages written to file.
							If a page contains zeroes only, that page is not written to file.
							These “zero pages” can be detected by checking if the page’s start 
							offset is bigger than it should be based on the sum of previously read pages 
							decompressed size(including zero pages).After reading all pages, if the total 
							decompressed size of the pages is not equal to the section’s size, add more zero 
							pages to the section until this condition is met.
				*/
				descriptor.PageCount = decompressed.ReadInt<LittleEndianConverter>();
				//0x0C	4	Max Decompressed Size of a section page of this type(normally 0x7400)
				descriptor.DecompressedSize = (ulong)decompressed.ReadInt<LittleEndianConverter>();
				//0x10	4	Unknown(long)
				decompressed.ReadInt<LittleEndianConverter>();
				//0x14	4	Compressed(1 = no, 2 = yes, normally 2)
				descriptor.CompressedCode = decompressed.ReadInt<LittleEndianConverter>();
				//0x18	4	Section Id(starts at 0). The first section(empty section) is numbered 0, consecutive sections are numbered descending from(the number of sections – 1) down to 1.
				descriptor.SectionId = decompressed.ReadInt<LittleEndianConverter>();
				//0x1C	4	Encrypted(0 = no, 1 = yes, 2 = unknown)
				descriptor.Encrypted = decompressed.ReadInt<LittleEndianConverter>();
				//0x20	64	Section Name(string)
				descriptor.Name = decompressed.ReadString(64).Split('\0')[0];

				//Following this, the following (local) section page map data will be present
				for (int j = 0; j < descriptor.PageCount; ++j)
				{
					DwgLocalSectionMap localmap = new DwgLocalSectionMap();
					//0x00	4	Page number(index into SectionPageMap), starts at 1
					localmap.PageNumber = decompressed.ReadInt<LittleEndianConverter>();
					//0x04	4	Data size for this page(compressed size).
					localmap.CompressedSize = (ulong)decompressed.ReadInt<LittleEndianConverter>();
					//0x08	8	Start offset for this page(OdUInt64).If this start offset is smaller than the sum of the decompressed size of all previous pages, then this page is to be preceded by zero pages until this condition is met.
					localmap.Offset = decompressed.ReadULong();

					//same decompressed size and seeker (temporal values)
					localmap.DecompressedSize = descriptor.DecompressedSize;
					localmap.Seeker = fileheader.Records[localmap.PageNumber].Seeker;

					//Maximum section page size appears to be 0x7400 bytes in the normal case.
					//If a logical section of the file (the database objects, for example) exceeds this size, then it is broken up into pages of size 0x7400.

					descriptor.LocalSections.Add(localmap);
				}

				//Get the final size for the local section
				uint sizeLeft = (uint)(descriptor.CompressedSize % descriptor.DecompressedSize);
				if (sizeLeft > 0U && descriptor.LocalSections.Count > 0)
					descriptor.LocalSections[descriptor.LocalSections.Count - 1].DecompressedSize = sizeLeft;

				fileheader.Descriptors.Add(descriptor.Name, descriptor);
			}
		}

		private void readFileHeaderAC21(DwgFileHeaderAC21 fileheader, IDwgStreamReader sreader)
		{
			this.readFileMetaData(fileheader, sreader);

			//The last 0x28 bytes of this section consists of check data, 
			//containing 5 Int64 values representing CRC’s and related numbers 
			//(starting from 0x3D8 until the end). The first 0x3D8 bytes 
			//should be decoded using Reed-Solomon (255, 239) decoding, with a factor of 3.
			byte[] compressedData = sreader.ReadBytes(0x400);
			byte[] decodedData = new byte[3 * 239]; //factor * blockSize
			this.reedSolomonDecoding(compressedData, decodedData, 3, 239);

			//0x00	8	CRC
			long crc = LittleEndianConverter.Instance.ToInt64(decodedData, 0);
			//0x08	8	Unknown key
			long unknownKey = LittleEndianConverter.Instance.ToInt64(decodedData, 8);
			//0x10	8	Compressed Data CRC
			long compressedDataCRC = LittleEndianConverter.Instance.ToInt64(decodedData, 16);
			//0x18	4	ComprLen
			int comprLen = LittleEndianConverter.Instance.ToInt32(decodedData, 24);
			//0x1C	4	Length2
			int length2 = LittleEndianConverter.Instance.ToInt32(decodedData, 28);

			//The decompressed size is a fixed 0x110.
			byte[] buffer = new byte[0x110];
			//If ComprLen is negative, then Data is not compressed (and data length is ComprLen).
			if (comprLen < 0)
			{
				//buffer = decodedData
				throw new NotImplementedException();
			}
			//If ComprLen is positive, the ComprLen bytes of data are compressed
			else
			{
				DwgLZ77AC21Decompressor.Decompress(decodedData, 32U, (uint)comprLen, buffer);
			}

			//Get the descompressed stream to read the records
			StreamIO decompressed = new StreamIO(buffer);

			//Read the compressed data
			fileheader.CompressedMetadata = new Dwg21CompressedMetadata()
			{
				//0x00	8	Header size (normally 0x70)
				HeaderSize = decompressed.ReadULong(),
				//0x08	8	File size
				FileSize = decompressed.ReadULong(),
				//0x10	8	PagesMapCrcCompressed
				PagesMapCrcCompressed = decompressed.ReadULong(),
				//0x18	8	PagesMapCorrectionFactor
				PagesMapCorrectionFactor = decompressed.ReadULong(),
				//0x20	8	PagesMapCrcSeed
				PagesMapCrcSeed = decompressed.ReadULong(),
				//0x28	8	Pages map2offset(relative to data page map 1, add 0x480 to get stream position)
				Map2Offset = decompressed.ReadULong(),
				//0x30	8	Pages map2Id
				Map2Id = decompressed.ReadULong(),
				//0x38	8	PagesMapOffset(relative to data page map 1, add 0x480 to get stream position)
				PagesMapOffset = decompressed.ReadULong(),
				//0x40	8	PagesMapId
				PagesMapId = decompressed.ReadULong(),
				//0x48	8	Header2offset(relative to page map 1 address, add 0x480 to get stream position)
				Header2offset = decompressed.ReadULong(),
				//0x50	8	PagesMapSizeCompressed
				PagesMapSizeCompressed = decompressed.ReadULong(),
				//0x58	8	PagesMapSizeUncompressed
				PagesMapSizeUncompressed = decompressed.ReadULong(),
				//0x60	8	PagesAmount
				PagesAmount = decompressed.ReadULong(),
				//0x68	8	PagesMaxId
				PagesMaxId = decompressed.ReadULong(),
				//0x70	8	Unknown(normally 0x20, 32)
				Unknow0x20 = decompressed.ReadULong(),
				//0x78	8	Unknown(normally 0x40, 64)
				Unknow0x40 = decompressed.ReadULong(),
				//0x80	8	PagesMapCrcUncompressed
				PagesMapCrcUncompressed = decompressed.ReadULong(),
				//0x88	8	Unknown(normally 0xf800, 63488)
				Unknown0xF800 = decompressed.ReadULong(),
				//0x90	8	Unknown(normally 4)
				Unknown4 = decompressed.ReadULong(),
				//0x98	8	Unknown(normally 1)
				Unknown1 = decompressed.ReadULong(),
				//0xA0	8	SectionsAmount(number of sections + 1)
				SectionsAmount = decompressed.ReadULong(),
				//0xA8	8	SectionsMapCrcUncompressed
				SectionsMapCrcUncompressed = decompressed.ReadULong(),
				//0xB0	8	SectionsMapSizeCompressed
				SectionsMapSizeCompressed = decompressed.ReadULong(),
				//0xB8	8	SectionsMap2Id
				SectionsMap2Id = decompressed.ReadULong(),
				//0xC0	8	SectionsMapId
				SectionsMapId = decompressed.ReadULong(),
				//0xC8	8	SectionsMapSizeUncompressed
				SectionsMapSizeUncompressed = decompressed.ReadULong(),
				//0xD0	8	SectionsMapCrcCompressed
				SectionsMapCrcCompressed = decompressed.ReadULong(),
				//0xD8	8	SectionsMapCorrectionFactor
				SectionsMapCorrectionFactor = decompressed.ReadULong(),
				//0xE0	8	SectionsMapCrcSeed
				SectionsMapCrcSeed = decompressed.ReadULong(),
				//0xE8	8	StreamVersion(normally 0x60100)
				StreamVersion = decompressed.ReadULong(),
				//0xF0	8	CrcSeed
				CrcSeed = decompressed.ReadULong(),
				//0xF8	8	CrcSeedEncoded
				CrcSeedEncoded = decompressed.ReadULong(),
				//0x100	8	RandomSeed
				RandomSeed = decompressed.ReadULong(),
				//0x108	8	Header CRC64
				HeaderCRC64 = decompressed.ReadULong()
			};

			//Prepare the page data stream to read
			byte[] arr = this.getPageBuffer(
				fileheader.CompressedMetadata.PagesMapOffset,
				fileheader.CompressedMetadata.PagesMapSizeCompressed,
				fileheader.CompressedMetadata.PagesMapSizeUncompressed,
				fileheader.CompressedMetadata.PagesMapCorrectionFactor,
				0xEF, sreader.Stream);

			//Read the page data
			StreamIO pageDataStream = new StreamIO(arr);

			long offset = 0;
			while (pageDataStream.Position < pageDataStream.Length)
			{
				long size = pageDataStream.ReadLong();
				long id = Math.Abs(pageDataStream.ReadLong());
				fileheader.Records.Add((int)id, new DwgSectionLocatorRecord((int)id, (int)offset, (int)size));

				//Add the size to the current offset
				offset += size;
			}

			//Prepare the section map data stream to read
			arr = this.getPageBuffer(
				(ulong)fileheader.Records[(int)fileheader.CompressedMetadata.SectionsMapId].Seeker,
				fileheader.CompressedMetadata.SectionsMapSizeCompressed,
				fileheader.CompressedMetadata.SectionsMapSizeUncompressed,
				fileheader.CompressedMetadata.SectionsMapCorrectionFactor,
				239, sreader.Stream);

			//Section map stream
			StreamIO sectionMapStream = new StreamIO(arr);

			while (sectionMapStream.Position < sectionMapStream.Length)
			{
				DwgSectionDescriptor section = new DwgSectionDescriptor();
				//0x00	8	Data size
				section.CompressedSize = sectionMapStream.ReadULong<LittleEndianConverter>();
				//0x08	8	Max size
				section.DecompressedSize = sectionMapStream.ReadULong<LittleEndianConverter>();
				//0x10	8	Encryption
				section.Encrypted = (int)sectionMapStream.ReadULong<LittleEndianConverter>();
				//0x18	8	HashCode
				section.HashCode = sectionMapStream.ReadULong<LittleEndianConverter>();
				//0x20	8	SectionNameLength
				int sectionNameLength = (int)sectionMapStream.ReadLong<LittleEndianConverter>();
				//0x28	8	Unknown
				sectionMapStream.ReadULong<LittleEndianConverter>();
				//0x30	8	Encoding
				section.Encoding = sectionMapStream.ReadULong<LittleEndianConverter>();
				//0x38	8	NumPages.This is the number of pages present 
				//			in the file for the section, but this does not include 
				//			pages that contain zeroes only.A page that contains zeroes 
				//			only is not written to file.If a page’s data offset is 
				//			smaller than the sum of the decompressed size of all previous 
				//			pages, then it is to be preceded by a zero page with a size 
				//			that is equal to the difference between these two numbers.
				section.PageCount = (int)sectionMapStream.ReadULong<LittleEndianConverter>();

				//Read the name
				if (sectionNameLength > 0)
				{
					section.Name = sectionMapStream.ReadString(sectionNameLength, Encoding.Unicode);
					//Remove the empty characters
					section.Name = section.Name.Replace("\0", "");
				}

				ulong currentOffset = 0;
				for (int i = 0; i < section.PageCount; ++i)
				{
					DwgLocalSectionMap page = new DwgLocalSectionMap();
					//8	Page data offset.If a page’s data offset is 
					//	smaller than the sum of the decompressed size
					//	of all previous pages, then it is to be preceded 
					//	by a zero page with a size that is equal to the 
					//	difference between these two numbers.
					page.Offset = sectionMapStream.ReadULong<LittleEndianConverter>();
					//8	Page Size
					page.Size = sectionMapStream.ReadLong<LittleEndianConverter>();
					//8	Page Id
					page.PageNumber = (int)sectionMapStream.ReadLong<LittleEndianConverter>();
					//8	Page Uncompressed Size
					page.DecompressedSize = sectionMapStream.ReadULong<LittleEndianConverter>();
					//8	Page Compressed Size
					page.CompressedSize = sectionMapStream.ReadULong<LittleEndianConverter>();
					//8	Page Checksum
					page.Checksum = sectionMapStream.ReadULong<LittleEndianConverter>();
					//8	Page CRC
					page.CRC = sectionMapStream.ReadULong<LittleEndianConverter>();

					//Add the page to the section
					section.LocalSections.Add(page);
					//Move the offset
					currentOffset = page.Offset + page.DecompressedSize;
				}
				if (sectionNameLength > 0)
					fileheader.Descriptors.Add(section.Name, section);
			}
		}

		private async Task readFileHeaderAC21Async(DwgFileHeaderAC21 fileheader, CancellationToken cancellationToken = default)
		{
			this.readFileMetaData(fileheader, await this.getStreamChunkAsync(_metaDataSize, cancellationToken));

			//The last 0x28 bytes of this section consists of check data, 
			//containing 5 Int64 values representing CRC’s and related numbers 
			//(starting from 0x3D8 until the end). The first 0x3D8 bytes 
			//should be decoded using Reed-Solomon (255, 239) decoding, with a factor of 3.
			byte[] compressedData = await _fileStream.ReadBytesAsync(0x400);
			byte[] decodedData = new byte[3 * 239]; //factor * blockSize
			this.reedSolomonDecoding(compressedData, decodedData, 3, 239);

			//0x00	8	CRC
			long crc = LittleEndianConverter.Instance.ToInt64(decodedData, 0);
			//0x08	8	Unknown key
			long unknownKey = LittleEndianConverter.Instance.ToInt64(decodedData, 8);
			//0x10	8	Compressed Data CRC
			long compressedDataCRC = LittleEndianConverter.Instance.ToInt64(decodedData, 16);
			//0x18	4	ComprLen
			int comprLen = LittleEndianConverter.Instance.ToInt32(decodedData, 24);
			//0x1C	4	Length2
			int length2 = LittleEndianConverter.Instance.ToInt32(decodedData, 28);

			//The decompressed size is a fixed 0x110.
			byte[] buffer = new byte[0x110];
			//If ComprLen is negative, then Data is not compressed (and data length is ComprLen).
			if (comprLen < 0)
			{
				//buffer = decodedData
				throw new NotImplementedException();
			}
			//If ComprLen is positive, the ComprLen bytes of data are compressed
			else
			{
				DwgLZ77AC21Decompressor.Decompress(decodedData, 32U, (uint)comprLen, buffer);
			}

			//Get the descompressed stream to read the records
			StreamIO decompressed = new StreamIO(buffer);

			//Read the compressed data
			fileheader.CompressedMetadata = new Dwg21CompressedMetadata()
			{
				//0x00	8	Header size (normally 0x70)
				HeaderSize = decompressed.ReadULong(),
				//0x08	8	File size
				FileSize = decompressed.ReadULong(),
				//0x10	8	PagesMapCrcCompressed
				PagesMapCrcCompressed = decompressed.ReadULong(),
				//0x18	8	PagesMapCorrectionFactor
				PagesMapCorrectionFactor = decompressed.ReadULong(),
				//0x20	8	PagesMapCrcSeed
				PagesMapCrcSeed = decompressed.ReadULong(),
				//0x28	8	Pages map2offset(relative to data page map 1, add 0x480 to get stream position)
				Map2Offset = decompressed.ReadULong(),
				//0x30	8	Pages map2Id
				Map2Id = decompressed.ReadULong(),
				//0x38	8	PagesMapOffset(relative to data page map 1, add 0x480 to get stream position)
				PagesMapOffset = decompressed.ReadULong(),
				//0x40	8	PagesMapId
				PagesMapId = decompressed.ReadULong(),
				//0x48	8	Header2offset(relative to page map 1 address, add 0x480 to get stream position)
				Header2offset = decompressed.ReadULong(),
				//0x50	8	PagesMapSizeCompressed
				PagesMapSizeCompressed = decompressed.ReadULong(),
				//0x58	8	PagesMapSizeUncompressed
				PagesMapSizeUncompressed = decompressed.ReadULong(),
				//0x60	8	PagesAmount
				PagesAmount = decompressed.ReadULong(),
				//0x68	8	PagesMaxId
				PagesMaxId = decompressed.ReadULong(),
				//0x70	8	Unknown(normally 0x20, 32)
				Unknow0x20 = decompressed.ReadULong(),
				//0x78	8	Unknown(normally 0x40, 64)
				Unknow0x40 = decompressed.ReadULong(),
				//0x80	8	PagesMapCrcUncompressed
				PagesMapCrcUncompressed = decompressed.ReadULong(),
				//0x88	8	Unknown(normally 0xf800, 63488)
				Unknown0xF800 = decompressed.ReadULong(),
				//0x90	8	Unknown(normally 4)
				Unknown4 = decompressed.ReadULong(),
				//0x98	8	Unknown(normally 1)
				Unknown1 = decompressed.ReadULong(),
				//0xA0	8	SectionsAmount(number of sections + 1)
				SectionsAmount = decompressed.ReadULong(),
				//0xA8	8	SectionsMapCrcUncompressed
				SectionsMapCrcUncompressed = decompressed.ReadULong(),
				//0xB0	8	SectionsMapSizeCompressed
				SectionsMapSizeCompressed = decompressed.ReadULong(),
				//0xB8	8	SectionsMap2Id
				SectionsMap2Id = decompressed.ReadULong(),
				//0xC0	8	SectionsMapId
				SectionsMapId = decompressed.ReadULong(),
				//0xC8	8	SectionsMapSizeUncompressed
				SectionsMapSizeUncompressed = decompressed.ReadULong(),
				//0xD0	8	SectionsMapCrcCompressed
				SectionsMapCrcCompressed = decompressed.ReadULong(),
				//0xD8	8	SectionsMapCorrectionFactor
				SectionsMapCorrectionFactor = decompressed.ReadULong(),
				//0xE0	8	SectionsMapCrcSeed
				SectionsMapCrcSeed = decompressed.ReadULong(),
				//0xE8	8	StreamVersion(normally 0x60100)
				StreamVersion = decompressed.ReadULong(),
				//0xF0	8	CrcSeed
				CrcSeed = decompressed.ReadULong(),
				//0xF8	8	CrcSeedEncoded
				CrcSeedEncoded = decompressed.ReadULong(),
				//0x100	8	RandomSeed
				RandomSeed = decompressed.ReadULong(),
				//0x108	8	Header CRC64
				HeaderCRC64 = decompressed.ReadULong()
			};

			//Prepare the page data stream to read
			byte[] arr = await this.getPageBufferAsync(
				fileheader.CompressedMetadata.PagesMapOffset,
				fileheader.CompressedMetadata.PagesMapSizeCompressed,
				fileheader.CompressedMetadata.PagesMapSizeUncompressed,
				fileheader.CompressedMetadata.PagesMapCorrectionFactor,
				0xEF, _fileStream.Stream);

			//Read the page data
			StreamIO pageDataStream = new StreamIO(arr);

			long offset = 0;
			while (pageDataStream.Position < pageDataStream.Length)
			{
				long size = pageDataStream.ReadLong();
				long id = Math.Abs(pageDataStream.ReadLong());
				fileheader.Records.Add((int)id, new DwgSectionLocatorRecord((int)id, (int)offset, (int)size));

				//Add the size to the current offset
				offset += size;
			}

			//Prepare the section map data stream to read
			arr = await this.getPageBufferAsync(
				(ulong)fileheader.Records[(int)fileheader.CompressedMetadata.SectionsMapId].Seeker,
				fileheader.CompressedMetadata.SectionsMapSizeCompressed,
				fileheader.CompressedMetadata.SectionsMapSizeUncompressed,
				fileheader.CompressedMetadata.SectionsMapCorrectionFactor,
				239, _fileStream.Stream);

			//Section map stream
			StreamIO sectionMapStream = new StreamIO(arr);

			while (sectionMapStream.Position < sectionMapStream.Length)
			{
				DwgSectionDescriptor section = new DwgSectionDescriptor();
				//0x00	8	Data size
				section.CompressedSize = sectionMapStream.ReadULong<LittleEndianConverter>();
				//0x08	8	Max size
				section.DecompressedSize = sectionMapStream.ReadULong<LittleEndianConverter>();
				//0x10	8	Encryption
				section.Encrypted = (int)sectionMapStream.ReadULong<LittleEndianConverter>();
				//0x18	8	HashCode
				section.HashCode = sectionMapStream.ReadULong<LittleEndianConverter>();
				//0x20	8	SectionNameLength
				int sectionNameLength = (int)sectionMapStream.ReadLong<LittleEndianConverter>();
				//0x28	8	Unknown
				sectionMapStream.ReadULong<LittleEndianConverter>();
				//0x30	8	Encoding
				section.Encoding = sectionMapStream.ReadULong<LittleEndianConverter>();
				//0x38	8	NumPages.This is the number of pages present 
				//			in the file for the section, but this does not include 
				//			pages that contain zeroes only.A page that contains zeroes 
				//			only is not written to file.If a page’s data offset is 
				//			smaller than the sum of the decompressed size of all previous 
				//			pages, then it is to be preceded by a zero page with a size 
				//			that is equal to the difference between these two numbers.
				section.PageCount = (int)sectionMapStream.ReadULong<LittleEndianConverter>();

				//Read the name
				if (sectionNameLength > 0)
				{
					section.Name = sectionMapStream.ReadString(sectionNameLength, Encoding.Unicode);
					//Remove the empty characters
					section.Name = section.Name.Replace("\0", "");
				}

				ulong currentOffset = 0;
				for (int i = 0; i < section.PageCount; ++i)
				{
					DwgLocalSectionMap page = new DwgLocalSectionMap();
					//8	Page data offset.If a page’s data offset is 
					//	smaller than the sum of the decompressed size
					//	of all previous pages, then it is to be preceded 
					//	by a zero page with a size that is equal to the 
					//	difference between these two numbers.
					page.Offset = sectionMapStream.ReadULong<LittleEndianConverter>();
					//8	Page Size
					page.Size = sectionMapStream.ReadLong<LittleEndianConverter>();
					//8	Page Id
					page.PageNumber = (int)sectionMapStream.ReadLong<LittleEndianConverter>();
					//8	Page Uncompressed Size
					page.DecompressedSize = sectionMapStream.ReadULong<LittleEndianConverter>();
					//8	Page Compressed Size
					page.CompressedSize = sectionMapStream.ReadULong<LittleEndianConverter>();
					//8	Page Checksum
					page.Checksum = sectionMapStream.ReadULong<LittleEndianConverter>();
					//8	Page CRC
					page.CRC = sectionMapStream.ReadULong<LittleEndianConverter>();

					//Add the page to the section
					section.LocalSections.Add(page);
					//Move the offset
					currentOffset = page.Offset + page.DecompressedSize;
				}
				if (sectionNameLength > 0)
					fileheader.Descriptors.Add(section.Name, section);
			}
		}

		private void readFileMetaData(DwgFileHeaderAC18 fileheader, IDwgStreamReader sreader)
		{
			//5 bytes of 0x00 
			sreader.Advance(5);

			//0x0B	1	Maintenance release version
			fileheader.AcadMaintenanceVersion = sreader.ReadByte();
			//0x0C	1	Byte 0x00, 0x01, or 0x03
			sreader.Advance(1);
			//0x0D	4	Preview address(long), points to the image page + page header size(0x20).
			fileheader.PreviewAddress = sreader.ReadRawLong();
			//0x11	1	Dwg version (Acad version that writes the file)
			fileheader.DwgVersion = sreader.ReadByte();
			//0x12	1	Application maintenance release version(Acad maintenance version that writes the file)
			fileheader.AppReleaseVersion = sreader.ReadByte();

			//0x13	2	Codepage
			fileheader.DrawingCodePage = CadUtils.GetCodePage(sreader.ReadShort());

			//Advance empty bytes 
			//0x15	3	3 0x00 bytes
			sreader.Advance(3);

			//0x18	4	SecurityType (long), see R2004 meta data, the definition is the same, paragraph 4.1.
			fileheader.SecurityType = sreader.ReadRawLong();
			//0x1C	4	Unknown long
			sreader.ReadRawLong();
			//0x20	4	Summary info Address in stream
			fileheader.SummaryInfoAddr = sreader.ReadRawLong();
			//0x24	4	VBA Project Addr(0 if not present)
			fileheader.VbaProjectAddr = sreader.ReadRawLong();

			//0x28	4	0x00000080
			sreader.ReadRawLong();

			//0x2C	4	App info Address in stream
			sreader.ReadRawLong();

			//Get to offset 0x80
			//0x30	0x80	0x00 bytes
			sreader.Advance(80);
		}

		private void readEncriptedData(DwgFileHeaderAC18 fileHeader, IDwgStreamReader sreader)
		{
			//0x80	0x6C	Encrypted Data
			//Metadata:
			//The encrypted data at 0x80 can be decrypted by exclusive or’ing the 0x6c bytes of data 
			//from the file with the following magic number sequence:

			//29 23 BE 84 E1 6C D6 AE 52 90 49 F1 F1 BB E9 EB
			//B3 A6 DB 3C 87 0C 3E 99 24 5E 0D 1C 06 B7 47 DE
			//B3 12 4D C8 43 BB 8B A6 1F 03 5A 7D 09 38 25 1F
			//5D D4 CB FC 96 F5 45 3B 13 0D 89 0A 1C DB AE 32
			//20 9A 50 EE 40 78 36 FD 12 49 32 F6 9E 7D 49 DC
			//AD 4F 14 F2 44 40 66 D0 6B C4 30 B7

			StreamIO headerStream = new StreamIO(new CRC32StreamHandler(sreader.ReadBytes(0x6C), 0U)); //108
			headerStream.Encoding = TextEncoding.GetListedEncoding(CodePage.Windows1252);

			//0x00	12	“AcFssFcAJMB” file ID string
			string fileId = headerStream.ReadString(12);
			if (fileId != "AcFssFcAJMB\0")
			{
				this.notify($"File validation failed, id should be : AcFssFcAJMB\0, but is : {fileId}", NotificationType.Warning);
			}

			//0x0C	4	0x00(long)
			headerStream.ReadInt<LittleEndianConverter>();
			//0x10	4	0x6c(long)
			headerStream.ReadInt<LittleEndianConverter>();
			//0x14	4	0x04(long)
			headerStream.ReadInt<LittleEndianConverter>();
			//0x18	4	Root tree node gap
			fileHeader.RootTreeNodeGap = headerStream.ReadInt<LittleEndianConverter>();
			//0x1C	4	Lowermost left tree node gap
			fileHeader.LeftGap = headerStream.ReadInt<LittleEndianConverter>();
			//0x20	4	Lowermost right tree node gap
			fileHeader.RigthGap = headerStream.ReadInt<LittleEndianConverter>();
			//0x24	4	Unknown long(ODA writes 1)	
			headerStream.ReadInt<LittleEndianConverter>();
			//0x28	4	Last section page Id
			fileHeader.LastPageId = headerStream.ReadInt<LittleEndianConverter>();

			//0x2C	8	Last section page end address
			fileHeader.LastSectionAddr = headerStream.ReadULong<LittleEndianConverter>();
			//0x34	8	Second header data address pointing to the repeated header data at the end of the file
			fileHeader.SecondHeaderAddr = headerStream.ReadULong<LittleEndianConverter>();

			//0x3C	4	Gap amount
			fileHeader.GapAmount = headerStream.ReadUInt<LittleEndianConverter>();
			//0x40	4	Section page amount
			fileHeader.SectionAmount = headerStream.ReadUInt<LittleEndianConverter>();
			//0x44	4	0x20(long)
			headerStream.ReadInt<LittleEndianConverter>();
			//0x48	4	0x80(long)
			headerStream.ReadInt<LittleEndianConverter>();
			//0x4C	4	0x40(long)
			headerStream.ReadInt<LittleEndianConverter>();
			//0x50	4	Section Page Map Id
			fileHeader.SectionPageMapId = headerStream.ReadUInt<LittleEndianConverter>();
			//0x54	8	Section Page Map address(add 0x100 to this value)
			fileHeader.PageMapAddress = headerStream.ReadULong<LittleEndianConverter>() + 256UL;
			//0x5C	4	Section Map Id
			fileHeader.SectionMapId = headerStream.ReadUInt<LittleEndianConverter>();
			//0x60	4	Section page array size
			fileHeader.SectionArrayPageSize = headerStream.ReadUInt<LittleEndianConverter>();
			//0x64	4	Gap array size
			fileHeader.GapArraySize = headerStream.ReadUInt<LittleEndianConverter>();
			//0x68	4	CRC32(long).See paragraph 2.14.2 for the 32 - bit CRC calculation, 
			//			the seed is zero. Note that the CRC 
			//			calculation is done including the 4 CRC bytes that are 
			//			initially zero! So the CRC calculation takes into account 
			//			all of the 0x6c bytes of the data in this table.
			fileHeader.CRCSeed = headerStream.ReadUInt();
		}

		private async Task<byte[]> getPageBufferAsync(ulong pageOffset, ulong compressedSize, ulong uncompressedSize, ulong correctionFactor, int blockSize, Stream stream)
		{
			//Avoid shifted bits
			ulong v = compressedSize + 7L;
			ulong v1 = v & 0b11111111_11111111_11111111_11111000L;

			uint totalSize = (uint)(v1 * correctionFactor);

			int factor = (int)(totalSize + blockSize - 1L) / blockSize;
			int lenght = factor * byte.MaxValue;

			byte[] buffer = new byte[lenght];

			//Relative to data page map 1, add 0x480 to get stream position
			stream.Position = (long)(0x480 + pageOffset);
			await stream.ReadAsync(buffer, 0, lenght);

			byte[] compressedData = new byte[(int)totalSize];
			this.reedSolomonDecoding(buffer, compressedData, factor, blockSize);

			byte[] decompressedData = new byte[uncompressedSize];

			DwgLZ77AC21Decompressor.Decompress(compressedData, 0U, (uint)compressedSize, decompressedData);

			return decompressedData;
		}

		private byte[] getPageBuffer(ulong pageOffset, ulong compressedSize, ulong uncompressedSize, ulong correctionFactor, int blockSize, Stream stream)
		{
			//Avoid shifted bits
			ulong v = compressedSize + 7L;
			ulong v1 = v & 0b11111111_11111111_11111111_11111000L;

			uint totalSize = (uint)(v1 * correctionFactor);

			int factor = (int)(totalSize + blockSize - 1L) / blockSize;
			int lenght = factor * byte.MaxValue;

			byte[] buffer = new byte[lenght];

			//Relative to data page map 1, add 0x480 to get stream position
			stream.Position = (long)(0x480 + pageOffset);
			stream.Read(buffer, 0, lenght);

			byte[] compressedData = new byte[(int)totalSize];
			this.reedSolomonDecoding(buffer, compressedData, factor, blockSize);

			byte[] decompressedData = new byte[uncompressedSize];

			DwgLZ77AC21Decompressor.Decompress(compressedData, 0U, (uint)compressedSize, decompressedData);

			return decompressedData;
		}

		private void reedSolomonDecoding(byte[] encoded, byte[] buffer, int factor, int blockSize)
		{
			int index = 0;
			int n = 0;
			int length = buffer.Length;
			for (int i = 0; i < factor; ++i)
			{
				int cindex = n;
				if (n < encoded.Length)
				{
					int size = Math.Min(length, blockSize);
					length -= size;
					int offset = index + size;
					while (index < offset)
					{
						buffer[index] = encoded[cindex];
						++index;
						cindex += factor;
					}
				}
				++n;
			}
		}

		private void getPageHeaderData(IDwgStreamReader sreader,
			out long sectionType,
			out long decompressedSize,
			out long compressedSize,
			out long compressionType,
			out long checksum
			)
		{
			//0x00	4	Section page type:
			//Section page map: 0x41630e3b
			//Section map: 0x4163003b
			sectionType = sreader.ReadRawLong();
			//0x04	4	Decompressed size of the data that follows
			decompressedSize = sreader.ReadRawLong();
			//0x08	4	Compressed size of the data that follows(CompDataSize)
			compressedSize = sreader.ReadRawLong();

			//0x0C	4	Compression type(0x02)
			compressionType = sreader.ReadRawLong();
			//0x10	4	Section page checksum
			checksum = sreader.ReadRawLong();
		}

		private async Task<Stream> getHeaderAC15Stream(CancellationToken cancellationToken = default)
		{
			bool match = false;
			List<byte> buffer = new List<byte>();
			buffer.AddRange(await this._fileStream.ReadBytesAsync(16, cancellationToken));

			do
			{
				byte[] sn = buffer.Skip(Math.Max(0, buffer.Count() - 16)).ToArray();
				if (CheckSentinel(sn, DwgFileHeaderAC15.EndSentinel))
				{
					match = true;
				}
				else
				{
					buffer.Add(await this._fileStream.ReadByteAsync(cancellationToken));
				}
			}
			while (!match);

			return new MemoryStream(buffer.ToArray());
		}

		private async Task<IDwgStreamReader> getStreamChunkAsync(int length, CancellationToken cancellationToken = default)
		{
			MemoryStream ms = new MemoryStream(await this._fileStream.ReadBytesAsync(length, cancellationToken));
			return DwgStreamReaderBase.GetStreamHandler(this._version, ms);
		}

		private void updateFileVersion(byte[] buffer)
		{
			ACadVersion version = CadUtils.GetVersionFromName(Encoding.ASCII.GetString(buffer));
			this.setVersion(version);
		}
	}
}
