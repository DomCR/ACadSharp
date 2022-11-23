using ACadSharp.IO.DWG.DwgStreamWriters;
using CSUtilities.Converters;
using CSUtilities.IO;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	public class DwgWriter : CadWriterBase
	{
		private ACadVersion _version { get { return this._document.Header.Version; } }

		private StreamIO _stream;

		private IDwgStreamWriter _writer;

		private DwgFileHeader _fileHeader;

		private DwgFileHeaderWriterAC18 _fileHeaderWriter;

		private CadDocument _document;

		[Obsolete]
		private Dictionary<string, DwgSectionDescriptor> _descriptors = new Dictionary<string, DwgSectionDescriptor>();

		private List<DwgLocalSectionMap> _localSectionsMaps = new List<DwgLocalSectionMap>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="document"></param>
		/// <exception cref="NotImplementedException">Binary writer not implemented</exception>
		public DwgWriter(string filename, CadDocument document)
			: this(File.Create(filename), document)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="document"></param>
		/// <exception cref="NotImplementedException">Binary writer not implemented</exception>
		public DwgWriter(Stream stream, CadDocument document)
		{
			this._stream = new StreamIO(stream);
			this._document = document;
			this._fileHeader = DwgFileHeader.CreateFileHeader(_version);

			this._writer = DwgStreamWriterBase.GetStreamHandler(_version, this._stream.Stream, Encoding.Default);

			this._fileHeaderWriter = new DwgFileHeaderWriterAC18(stream, _version);
		}

		/// <inheritdoc/>
		public override void Write()
		{
			this.writeHeader();
			//this.writeClasses();
			//this.writeEmptySection();
			this.writeSummaryInfo();
			this.writePreview();
			//this.writeVBASection();
			//this.writeAppInfo();
			//this.writeFileDepList();
			//this.writeRevHistory();
			//this.writeSecurity();
			//this.writeObjects();
			//this.writeObjFreeSpace();
			//this.writeTemplate();
			//this.writeHandles();
			//this.writePrototype();
			//this.writeAuxHeader();

			this.writeFileHeader();
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			this._stream.Dispose();
		}

		private void createSectionDescriptor(string name, MemoryStream stream, bool isCompressed)
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
			long position = this._stream.Position;
			int compressDiff = DwgCheckSumCalculator.CompressionCalculator((int)mainStream.Length);

			localMap.PageNumber = this._localSectionsMaps.Count + 1;
			localMap.Offset = offset;
			localMap.Seeker = position;
			//localMap.ODA = 0;
			localMap.CompressedSize = (ulong)mainStream.Length;
			localMap.DecompressedSize = totalSize;
			localMap.PageSize = (long)localMap.CompressedSize + 32 + compressDiff;
			localMap.Checksum = 0u;

			System.IO.MemoryStream checkSumStream = new System.IO.MemoryStream(32);
			this.writeHeaderDataSection(checkSumStream, descriptor, localMap, (int)pageSize);
			localMap.Checksum = DwgCheckSumCalculator.Calculate(0, checkSumStream.GetBuffer(), 0, (int)checkSumStream.Length);
			checkSumStream.SetLength(0L);
			checkSumStream.Position = 0L;

			this.writeHeaderDataSection(checkSumStream, descriptor, localMap, (int)pageSize);

			this.applyMask(checkSumStream.GetBuffer(), 0, (int)checkSumStream.Length);

			this._stream.Stream.Write(checkSumStream.GetBuffer(), 0, (int)checkSumStream.Length);
			this._stream.Stream.Write(mainStream.GetBuffer(), 0, (int)mainStream.Length);

			if (isCompressed)
			{
				this._stream.Stream.Write(DwgCheckSumCalculator.MagicSequence, 0, compressDiff);
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

		private void writeHeader()
		{
			MemoryStream stream = new MemoryStream();
			DwgHeaderWriter writer = new DwgHeaderWriter(stream, this._document);
			writer.OnNotification += triggerNotification;
			writer.Write();

			if (this._version < ACadVersion.AC1018)
			{
				//TODO: Create the process for section record
				throw new NotImplementedException();
			}
			else
			{
				this._fileHeaderWriter.CreateSection(DwgSectionDefinition.Header, stream, false);
			}
		}

		private void writeSummaryInfo()
		{
			///<see cref="DwgReader.ReadSummaryInfo"/>

			if (this._fileHeader.AcadVersion < ACadVersion.AC1018)
				return;

			MemoryStream stream = new MemoryStream();
			var writer = DwgStreamWriterBase.GetStreamHandler(_version, stream, Encoding.Default);

			CadSummaryInfo info = this._document.SummaryInfo;

			writer.WriteTextUnicode(info.Title);
			writer.WriteTextUnicode(info.Subject);
			writer.WriteTextUnicode(info.Author);
			writer.WriteTextUnicode(info.Keywords);
			writer.WriteTextUnicode(info.Comments);
			writer.WriteTextUnicode(info.LastSavedBy);
			writer.WriteTextUnicode(info.RevisionNumber);
			writer.WriteTextUnicode(info.HyperlinkBase);

			//?	8	Total editing time(ODA writes two zero Int32’s)
			writer.WriteInt(0);
			writer.WriteInt(0);

			writer.WriteDateTime(info.CreatedDate);
			writer.WriteDateTime(info.ModifiedDate);

			//Int16	2 + 2 * (2 + n)	Property count, followed by PropertyCount key/value string pairs.
			writer.WriteRawShort((ushort)info.Properties.Count);
			foreach (KeyValuePair<string, string> property in info.Properties)
			{
				writer.WriteTextUnicode(property.Key);
				writer.WriteTextUnicode(property.Value);
			}

			writer.WriteInt(0);
			writer.WriteInt(0);

			this._fileHeaderWriter.CreateSection(DwgSectionDefinition.Header, stream, false);
		}

		private void writePreview()
		{
			MemoryStream stream = new MemoryStream();
			DwgPreviewWriter writer = new DwgPreviewWriter(this._version, stream);
			writer.Write();

			this._fileHeaderWriter.CreateSection(DwgSectionDefinition.Header, stream, false);
		}

		private void writeFileHeader()
		{
			IDwgStreamWriter swriter = DwgStreamWriterBase.GetStreamHandler(_version, this._stream.Stream, Encoding.Default);

			//5 bytes of 0x00 
			swriter.WriteBytes(new byte[5]);

			//0x0B	1	Maintenance release version
			swriter.WriteByte((byte)this._document.Header.MaintenanceVersion);
			//0x0C	1	Byte 0x00, 0x01, or 0x03
			swriter.WriteByte(0x03);
			//0x0D	4	Preview address(long), points to the image page + page header size(0x20).
			swriter.WriteRawLong(this._descriptors[DwgSectionDefinition.Preview].LocalSections[0].Seeker + 0x20);
			//0x11	1	Dwg version (Acad version that writes the file)
			swriter.WriteByte((byte)this._version);
			//0x12	1	Application maintenance release version(Acad maintenance version that writes the file)
			swriter.WriteByte(0);   //Read from examples

			//0x13	2	Codepage
			swriter.WriteRawShort((ushort)CodePage.Windows1252);

			//Advance empty bytes 
			//0x15	3	3 0x00 bytes
			swriter.WriteBytes(new byte[3]);

			//0x18	4	SecurityType (long), see R2004 meta data, the definition is the same, paragraph 4.1.
			swriter.WriteInt(0);
			//0x1C	4	Unknown long
			swriter.WriteInt(0);
			//0x20	4	Summary info Address in stream
			swriter.WriteInt((int)(this._descriptors[DwgSectionDefinition.SummaryInfo].LocalSections[0].Seeker + 32));
			//0x24	4	VBA Project Addr(0 if not present)
			swriter.WriteInt(0);

			//0x28	4	0x00000080
			swriter.WriteInt(0x00000080);

			//0x2C	0x54	0x00 bytes
			swriter.WriteInt(((int)this._descriptors[DwgSectionDefinition.AppInfo].LocalSections[0].Seeker + 32));
			//Get to offset 0x80
			swriter.WriteBytes(new byte[80]);


			System.IO.MemoryStream fileHeaderStream = new System.IO.MemoryStream();
			this.writeFileHeaderAC18(fileHeaderStream, (DwgFileHeaderAC18)this._fileHeader);

			this._stream.Stream.Write(fileHeaderStream.GetBuffer(), 0, (int)fileHeaderStream.Length);
			this._stream.Stream.Write(DwgCheckSumCalculator.MagicSequence, 236, 20);
		}

		private void writeFileHeaderAC18(MemoryStream stream, DwgFileHeaderAC18 fileheader)
		{
			//0x00	12	“AcFssFcAJMB” file ID string
			CRC32StreamHandler crcStream = new CRC32StreamHandler(stream, 0u);
			StreamIO headerStream = new StreamIO(crcStream);

			crcStream.Write(TextEncoding.GetListedEncoding(CodePage.Windows1252).GetBytes("AcFssFcAJMB\0"), 0, 12);
			//0x0C	4	0x00(long)
			headerStream.Write<int>(0, LittleEndianConverter.Instance);
			//0x10	4	0x6c(long)
			headerStream.Write<int>(0x6c, LittleEndianConverter.Instance);
			//0x14	4	0x04(long)
			headerStream.Write<int>(0x04, LittleEndianConverter.Instance);

			headerStream.Write<int>(fileheader.RootTreeNodeGap, LittleEndianConverter.Instance);
			headerStream.Write<int>(fileheader.LeftGap, LittleEndianConverter.Instance);
			headerStream.Write<int>(fileheader.RigthGap, LittleEndianConverter.Instance);
			headerStream.Write<int>(1, LittleEndianConverter.Instance);
			headerStream.Write<int>(fileheader.LastPageId, LittleEndianConverter.Instance);

			//0x2C	8	Last section page end address
			headerStream.Write(fileheader.LastSectionAddr, LittleEndianConverter.Instance);
			headerStream.Write(fileheader.SecondHeaderAddr, LittleEndianConverter.Instance);
			headerStream.Write(fileheader.GapAmount, LittleEndianConverter.Instance);
			headerStream.Write(fileheader.SectionAmount, LittleEndianConverter.Instance);

			headerStream.Write<int>(0x20, LittleEndianConverter.Instance);
			headerStream.Write<int>(0x80, LittleEndianConverter.Instance);
			headerStream.Write<int>(0x40, LittleEndianConverter.Instance);

			headerStream.Write(fileheader.SectionPageMapId, LittleEndianConverter.Instance);
			headerStream.Write(fileheader.PageMapAddress - 256, LittleEndianConverter.Instance);
			headerStream.Write(fileheader.SectionMapId, LittleEndianConverter.Instance);
			headerStream.Write(fileheader.SectionArrayPageSize, LittleEndianConverter.Instance);
			headerStream.Write(fileheader.GapArraySize, LittleEndianConverter.Instance);
		}
	}
}
