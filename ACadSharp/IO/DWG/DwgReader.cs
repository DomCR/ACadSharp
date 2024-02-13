using ACadSharp.Classes;
using ACadSharp.Header;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ACadSharp.Exceptions;
using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.DwgStreamReaders;
using System.Threading.Tasks;
using System.Threading;

namespace ACadSharp.IO
{
	public class DwgReader : CadReaderBase
	{
		public DwgReaderConfiguration Configuration { get; set; } = new DwgReaderConfiguration();

		private ACadVersion _version = ACadVersion.Unknown;

		private DwgDocumentBuilder _builder;

		private DwgFileHeader _fileHeader;

		/// <summary>
		/// Initializes a new instance of the <see cref="DwgReader"/> class.
		/// </summary>
		/// <param name="filename">The filename of the file to open.</param>
		/// <param name="notification">Notification handler, sends any message or notification about the reading process.</param>
		public DwgReader(string filename, NotificationEventHandler notification = null) : base(filename, notification) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="DwgReader" /> class.
		/// </summary>
		/// <param name="stream">The stream to read from.</param>
		/// <param name="notification">Notification handler, sends any message or notification about the reading process.</param>
		public DwgReader(Stream stream, NotificationEventHandler notification = null) : base(stream, notification) { }

		/// <summary>
		/// Read a dwg document in a stream.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="notification">Notification handler, sends any message or notification about the reading process.</param>
		/// <returns></returns>
		public static CadDocument Read(Stream stream, NotificationEventHandler notification = null)
		{
			return Read(stream, new DwgReaderConfiguration(), notification);
		}

		/// <summary>
		/// Read a dwg document in a stream.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="configuration"></param>
		/// <param name="notification">Notification handler, sends any message or notification about the reading process.</param>
		/// <returns></returns>
		public static CadDocument Read(Stream stream, DwgReaderConfiguration configuration, NotificationEventHandler notification = null)
		{
			CadDocument doc = null;

			using (DwgReader reader = new DwgReader(stream, notification))
			{
				reader.Configuration = configuration;
				doc = reader.Read();
			}

			return doc;
		}

		/// <summary>
		/// Read a dwg document from a file
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="notification">Notification handler, sends any message or notification about the reading process.</param>
		/// <returns></returns>
		public static CadDocument Read(string filename, NotificationEventHandler notification = null)
		{
			return Read(filename, new DwgReaderConfiguration(), notification);
		}

		/// <summary>
		/// Read a dwg document from a file
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="configuration"></param>
		/// <param name="notification">Notification handler, sends any message or notification about the reading process.</param>
		/// <returns></returns>
		public static CadDocument Read(string filename, DwgReaderConfiguration configuration, NotificationEventHandler notification = null)
		{
			CadDocument doc = null;

			using (DwgReader reader = new DwgReader(filename, notification))
			{
				reader.Configuration = configuration;
				doc = reader.Read();
			}

			return doc;
		}

		/// <inheritdoc/>
		public override CadDocument Read()
		{
			this.initializeReader();

			//Read the file header
			this._fileHeader = this.readFileHeader();

			this._document.SummaryInfo = this.ReadSummaryInfo();
			this._document.Header = this.ReadHeader();
			this._document.Classes = this.readClasses();

			this.readAppInfo();

			//Read all the objects in the file
			this.readObjects();

			//Build the document 
			this._builder.BuildDocument();

			return this._document;
		}

		public async Task<CadDocument> ReadAsync(CancellationToken cancellationToken = default)
		{
			this.initializeReader();

			this._fileHeader = await this.readFileHeaderAsync(cancellationToken);

			if (this._fileHeader.AcadVersion >= ACadVersion.AC1018)
			{
				this._document.SummaryInfo = this.readSummaryInfo(await this.getSectionStreamAsync(DwgSectionDefinition.SummaryInfo));
			}

			this._document.Header = this.readHeader(await this.getSectionStreamAsync(DwgSectionDefinition.Header));

			return this._document;
		}

		/// <summary>
		/// Read the summary info of the dwg file.
		/// </summary>
		/// <remarks>
		/// Refers to AcDb:SummaryInfo data section.
		/// </remarks>
		/// <returns></returns>
		public CadSummaryInfo ReadSummaryInfo()
		{
			this._fileHeader = this._fileHeader ?? this.readFileHeader();

			//Older versions than 2004 don't have summaryinfo in it's file
			if (this._fileHeader.AcadVersion < ACadVersion.AC1018)
				return null;

			IDwgStreamReader reader = this.getSectionStream(DwgSectionDefinition.SummaryInfo);
			if (reader == null)
				return null;

			DwgSummaryInfoReader summaryReader = new DwgSummaryInfoReader(this._fileHeader.AcadVersion, reader);
			return summaryReader.Read();
		}

		private CadSummaryInfo readSummaryInfo(IDwgStreamReader reader)
		{
			DwgSummaryInfoReader summaryReader = new DwgSummaryInfoReader(this._fileHeader.AcadVersion, reader);
			return summaryReader.Read();
		}

		/// <summary>
		/// Read the preview image of the dwg file.
		/// </summary>
		/// <remarks>
		/// Refers to AcDb:Preview data section.
		/// </remarks>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public DwgPreview ReadPreview()
		{
			this._fileHeader = this._fileHeader ?? this.readFileHeader();

			//Check if the preview exist
			if (this._fileHeader.PreviewAddress < 0)
				return null;

			IDwgStreamReader sectionHandler = DwgStreamReaderBase.GetStreamHandler(this._fileHeader.AcadVersion, this._fileStream.Stream);
			sectionHandler.Position = this._fileHeader.PreviewAddress;

			//{0x1F,0x25,0x6D,0x07,0xD4,0x36,0x28,0x28,0x9D,0x57,0xCA,0x3F,0x9D,0x44,0x10,0x2B }
			byte[] sentinel = sectionHandler.ReadSentinel();

			//overall size	RL	overall size of image area
			long overallSize = sectionHandler.ReadRawLong();

			//imagespresent RC counter indicating what is present here
			byte imagespresent = (byte)sectionHandler.ReadRawChar();

			for (int i = 0; i < imagespresent; i++)
			{
				//Code RC code indicating what follows
				byte code = (byte)sectionHandler.ReadRawChar();
				switch (code)
				{
					case 1:
						//header data start RL start of header data
						long headerDataStart = sectionHandler.ReadRawLong();
						//header data size RL size of header data
						long headerDataSize = sectionHandler.ReadRawLong();
						break;
					case 2:
						//start of bmp RL start of bmp data
						long startOfBmp = sectionHandler.ReadRawLong();
						//size of bmp RL size of bmp data
						long sizeBmp = sectionHandler.ReadRawLong();
						break;
					case 3:
						//start of wmf RL start of wmf data
						long startOfWmf = sectionHandler.ReadRawLong();
						//size of wmf RL size of wmf data
						long sizeWmf = sectionHandler.ReadRawLong();
						break;
				}
			}

			//TODO: Implement the image reading
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		/// <remarks>
		/// Refers to AcDb:Header data section.
		/// </remarks>
		/// <returns></returns>
		public override CadHeader ReadHeader()
		{
			this._fileHeader = this._fileHeader ?? this.readFileHeader();

			CadHeader header = new CadHeader();
			header.CodePage = CadUtils.GetCodePageName(this._fileHeader.DrawingCodePage);

			IDwgStreamReader sreader = this.getSectionStream(DwgSectionDefinition.Header);

			DwgHeaderReader hReader = new DwgHeaderReader(this._fileHeader.AcadVersion, sreader, header);
			hReader.OnNotification += onNotificationEvent;

			hReader.Read(this._fileHeader.AcadMaintenanceVersion, out DwgHeaderHandlesCollection headerHandles);

			if (this._builder != null)
				this._builder.HeaderHandles = headerHandles;

			return header;
		}

		private CadHeader readHeader(IDwgStreamReader sreader)
		{
			CadHeader header = new CadHeader();
			header.CodePage = CadUtils.GetCodePageName(this._fileHeader.DrawingCodePage);

			DwgHeaderReader hReader = new DwgHeaderReader(this._fileHeader.AcadVersion, sreader, header);
			hReader.OnNotification += onNotificationEvent;

			hReader.Read(this._fileHeader.AcadMaintenanceVersion, out DwgHeaderHandlesCollection headerHandles);

			if (this._builder != null)
				this._builder.HeaderHandles = headerHandles;

			return header;
		}

		private void initializeReader()
		{
			this._document = new CadDocument(false);
			this._builder = new DwgDocumentBuilder(this._document, this.Configuration);
			this._builder.OnNotification += this.onNotificationEvent;
		}

		private DwgFileHeader readFileHeader()
		{
			DwgFileHeaderReader reader = new DwgFileHeaderReader(this._fileStream.Stream);
			this._fileHeader = reader.Read();

			this.setFileVersion(this._fileHeader);

			return this._fileHeader;
		}

		private async Task<DwgFileHeader> readFileHeaderAsync(CancellationToken cancellationToken = default)
		{
			DwgFileHeaderReader reader = new DwgFileHeaderReader(this._fileStream.Stream);
			this._fileHeader = await reader.ReadAsync(cancellationToken);

			this.setFileVersion(this._fileHeader);

			return this._fileHeader;
		}

		/// <summary>
		/// Read the classes section of the file.
		/// </summary>
		/// <remarks>
		/// Refers to AcDb:Classes data section.
		/// </remarks>
		/// <returns></returns>
		private DxfClassCollection readClasses()
		{
			this._fileHeader = this._fileHeader ?? this.readFileHeader();

			IDwgStreamReader sreader = this.getSectionStream(DwgSectionDefinition.Classes);

			var reader = new DwgClassesReader(this._fileHeader.AcadVersion, this._fileHeader);
			reader.OnNotification += onNotificationEvent;

			return reader.Read(sreader);
		}

		private void readAppInfo()
		{
#if TEST
			this._fileHeader = this._fileHeader ?? this.readFileHeader();

			IDwgStreamReader sreader = this.getSectionStream(DwgSectionDefinition.AppInfo);
			if (sreader is null)
			{
				return;
			}

			var reader = new DwgAppInfoReader(this._fileHeader.AcadVersion, sreader);
			reader.OnNotification += onNotificationEvent;

			reader.Read();
#else
			//Optional section, only for testing
			return;
#endif
		}

		/// <summary>
		/// Read the handles of the file, each entry of the dictionary is composed by
		/// the handler of the object (key) and the offset (value).
		/// </summary>
		/// <remarks>
		/// Refers to AcDb:Handles data section.
		/// </remarks>
		/// <returns></returns>
		private Dictionary<ulong, long> readHandles()
		{
			this._fileHeader = this._fileHeader ?? this.readFileHeader();

			IDwgStreamReader sreader = this.getSectionStream(DwgSectionDefinition.Handles);

			var handleReader = new DwgHandleReader(sreader, this._fileHeader.AcadVersion);
			handleReader.OnNotification += onNotificationEvent;

			return handleReader.Read();
		}

		/// <summary>
		/// Method only needed for versions <see cref="ACadVersion.AC1015"/> or lower.
		/// </summary>
		/// <remarks>
		/// Refers to AcDb:ObjFreeSpace data section.
		/// </remarks>
		/// <returns>the offset where the object section is</returns>
		private uint readObjFreeSpace()
		{
			this._fileHeader = this._fileHeader ?? this.readFileHeader();

			if (this._fileHeader.AcadVersion < ACadVersion.AC1018)
				return 0;

			IDwgStreamReader sreader = this.getSectionStream(DwgSectionDefinition.ObjFreeSpace);

			//Int32				4	0
			//UInt32			4	Approximate number of objects in the drawing(number of handles).
			//Julian datetime	8	If version > R14 then system variable TDUPDATE otherwise TDUUPDATE.
			sreader.Advance(16);

			//UInt32	4	Offset of the objects section in the stream.
			return sreader.ReadUInt();
		}

		/// <summary>
		/// Read the Template section.
		/// </summary>
		/// <remarks>
		/// Refers to AcDb:Template data section.
		/// </remarks>
		/// <returns></returns>
		private void readTemplate()
		{
			this._fileHeader = this._fileHeader ?? this.readFileHeader();

			IDwgStreamReader sreader = this.getSectionStream(DwgSectionDefinition.Template);

			throw new NotImplementedException();
		}

		/// <summary>
		/// Read the objects section of the files, this section contains all the entities.
		/// </summary>
		/// <remarks>
		/// Refers to AcDb:AcDbObjects data section.
		/// </remarks>
		private void readObjects()
		{
			Dictionary<ulong, long> handles = this.readHandles();
			this._document.Classes = this.readClasses();

			IDwgStreamReader sreader = null;
			if (this._fileHeader.AcadVersion <= ACadVersion.AC1015)
			{
				sreader = DwgStreamReaderBase.GetStreamHandler(this._fileHeader.AcadVersion, this._fileStream.Stream);
				//Handles are in absolute offset for this versions
				sreader.Position = 0;
			}
			else
			{
				sreader = this.getSectionStream(DwgSectionDefinition.AcDbObjects);
			}

			Queue<ulong> objectHandles = new Queue<ulong>(this._builder.HeaderHandles.GetHandles()
				.Where(o => o.HasValue)
				.Select(a => a.Value));

			DwgObjectReader sectionReader = new DwgObjectReader(
				this._fileHeader.AcadVersion,
				this._builder,
				sreader,
				objectHandles,
				handles,
				this._document.Classes);

			sectionReader.Read();
		}

		private void setFileVersion(DwgFileHeader fileHeader)
		{
			this._version = fileHeader.AcadVersion;
			this._encoding = this.getListedEncoding((int)_fileHeader.DrawingCodePage);
		}

		private IDwgStreamReader getSectionStream(string sectionName)
		{
			Stream sectionStream = null;

			//Get the section buffer
			switch (this._fileHeader.AcadVersion)
			{
				case ACadVersion.Unknown:
					throw new DwgNotSupportedException();
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
					throw new DwgNotSupportedException(this._fileHeader.AcadVersion);
				case ACadVersion.AC1012:
				case ACadVersion.AC1014:
				case ACadVersion.AC1015:
					sectionStream = this.getSectionBuffer15(this._fileHeader as DwgFileHeaderAC15, sectionName);
					break;
				case ACadVersion.AC1018:
					sectionStream = this.getSectionBuffer18(this._fileHeader as DwgFileHeaderAC18, sectionName);
					break;
				case ACadVersion.AC1021:
					sectionStream = this.getSectionBuffer21(this._fileHeader as DwgFileHeaderAC21, sectionName);
					break;
				case ACadVersion.AC1024:
				case ACadVersion.AC1027:
				case ACadVersion.AC1032:
					//Check if it works...
					sectionStream = this.getSectionBuffer18(this._fileHeader as DwgFileHeaderAC18, sectionName);
					break;
				default:
					break;
			}

			//Section not found
			if (sectionStream == null)
				return null;

			IDwgStreamReader streamHandler = DwgStreamReaderBase.GetStreamHandler(this._fileHeader.AcadVersion, sectionStream);

			//Set the encoding if needed
			streamHandler.Encoding = this._encoding;

			return streamHandler;
		}

		private async Task<IDwgStreamReader> getSectionStreamAsync(string sectionName, CancellationToken cancellationToken = default)
		{
			Stream sectionStream = null;

			//Get the section buffer
			switch (this._fileHeader.AcadVersion)
			{
				case ACadVersion.Unknown:
					throw new DwgNotSupportedException();
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
					throw new DwgNotSupportedException(this._fileHeader.AcadVersion);
				case ACadVersion.AC1012:
				case ACadVersion.AC1014:
				case ACadVersion.AC1015:
					sectionStream = await this.getSectionBuffer15Async(this._fileHeader as DwgFileHeaderAC15, sectionName, cancellationToken);
					break;
				case ACadVersion.AC1018:
					sectionStream = await this.getSectionBuffer18Async(this._fileHeader as DwgFileHeaderAC18, sectionName, cancellationToken);
					break;
				case ACadVersion.AC1021:
					sectionStream = this.getSectionBuffer21(this._fileHeader as DwgFileHeaderAC21, sectionName);
					break;
				case ACadVersion.AC1024:
				case ACadVersion.AC1027:
				case ACadVersion.AC1032:
					//Check if it works...
					sectionStream = await this.getSectionBuffer18Async(this._fileHeader as DwgFileHeaderAC18, sectionName, cancellationToken);
					break;
				default:
					break;
			}

			//Section not found
			if (sectionStream == null)
				return null;

			IDwgStreamReader streamHandler = DwgStreamReaderBase.GetStreamHandler(this._fileHeader.AcadVersion, sectionStream);

			//Set the encoding if needed
			streamHandler.Encoding = this._encoding;

			return streamHandler;
		}


		private Stream getSectionBuffer15(DwgFileHeaderAC15 fileheader, string sectionName)
		{
			Stream stream = null;

			//Get the section locator
			var sectionLocator = DwgSectionDefinition.GetSectionLocatorByName(sectionName);

			if (!sectionLocator.HasValue)
				//There is no section for this version
				return null;

			if (fileheader.Records.TryGetValue(sectionLocator.Value, out DwgSectionLocatorRecord record))
			{
				//set the stream position
				stream = this._fileStream.Stream;
				stream.Position = record.Seeker;
			}

			return stream;
		}

		private async Task<Stream> getSectionBuffer15Async(DwgFileHeaderAC15 fileheader, string sectionName, CancellationToken cancellationToken = default)
		{
			Stream stream = null;

			//Get the section locator
			var sectionLocator = DwgSectionDefinition.GetSectionLocatorByName(sectionName);

			if (!sectionLocator.HasValue)
				//There is no section for this version
				return null;

			if (fileheader.Records.TryGetValue(sectionLocator.Value, out DwgSectionLocatorRecord record))
			{
				//set the stream position
				this._fileStream.Stream.Position = record.Seeker;
				byte[] buffer = await this._fileStream.ReadBytesAsync((int)record.Size, cancellationToken);
				stream = new MemoryStream(buffer);
			}

			return stream;
		}

		private Stream getSectionBuffer18(DwgFileHeaderAC18 fileheader, string sectionName)
		{
			if (!fileheader.Descriptors.TryGetValue(sectionName, out DwgSectionDescriptor descriptor))
				return null;

			//get the total size of the page
			MemoryStream memoryStream = new MemoryStream((int)descriptor.DecompressedSize * descriptor.LocalSections.Count);

			foreach (DwgLocalSectionMap section in descriptor.LocalSections)
			{
				if (section.IsEmpty)
				{
					//Page is empty, fill the gap with 0s
					for (int index = 0; index < (int)section.DecompressedSize; ++index)
					{
						memoryStream.WriteByte(0);
					}
				}
				else
				{
					//Get the page section header
					IDwgStreamReader sreader = DwgStreamReaderBase.GetStreamHandler(fileheader.AcadVersion, this._fileStream.Stream);
					sreader.Position = section.Seeker;
					//Get the header data
					this.decryptDataSection(section, sreader);

					if (descriptor.IsCompressed)
					{
						//Page is compressed
						DwgLZ77AC18Decompressor.DecompressToDest(this._fileStream.Stream, memoryStream);
					}
					else
					{
						//Read the stream normally
						byte[] buffer = new byte[section.CompressedSize];
						sreader.Stream.Read(buffer, 0, (int)section.CompressedSize);
						memoryStream.Write(buffer, 0, (int)section.CompressedSize);
					}
				}
			}

			//Reset the stream
			memoryStream.Position = 0L;
			return memoryStream;
		}

		private async Task<Stream> getSectionBuffer18Async(DwgFileHeaderAC18 fileheader, string sectionName, CancellationToken cancellationToken = default)
		{
			if (!fileheader.Descriptors.TryGetValue(sectionName, out DwgSectionDescriptor descriptor))
				return null;

			//get the total size of the page
			MemoryStream memoryStream = new MemoryStream((int)descriptor.DecompressedSize * descriptor.LocalSections.Count);

			foreach (DwgLocalSectionMap section in descriptor.LocalSections)
			{
				if (section.IsEmpty)
				{
					//Page is empty, fill the gap with 0s
					for (int index = 0; index < (int)section.DecompressedSize; ++index)
					{
						memoryStream.WriteByte(0);
					}
				}
				else
				{
					this._fileStream.Position = section.Seeker;
					byte[] buffer = await this._fileStream.ReadBytesAsync((int)section.CompressedSize);

					//Get the page section header
					IDwgStreamReader sreader = DwgStreamReaderBase.GetStreamHandler(fileheader.AcadVersion, new MemoryStream(buffer));
					//Get the header data
					this.decryptDataSection(section, sreader);

					if (descriptor.IsCompressed)
					{
						//Page is compressed
						DwgLZ77AC18Decompressor.DecompressToDest(sreader.Stream, memoryStream);
					}
					else
					{
						//Read the stream normally
						memoryStream.Write(await this._fileStream.ReadBytesAsync((int)section.CompressedSize), 0, (int)section.CompressedSize);
					}
				}
			}

			//Reset the stream
			memoryStream.Position = 0L;
			return memoryStream;
		}

		private void decryptDataSection(DwgLocalSectionMap section, IDwgStreamReader sreader)
		{
			int secMask = 0x4164536B ^ (int)sreader.Position;

			//0x00	4	Section page type, since it’s always a data section: 0x4163043b
			var pageType = sreader.ReadRawLong() ^ secMask;
			//0x04	4	Section number
			var sectionNumber = sreader.ReadRawLong() ^ secMask;
			//0x08	4	Data size (compressed)
			section.CompressedSize = (ulong)(sreader.ReadRawLong() ^ secMask);
			//0x0C	4	Page Size (decompressed)
			section.PageSize = sreader.ReadRawLong() ^ secMask;
			//0x10	4	Start Offset (in the decompressed buffer)
			var startOffset = sreader.ReadRawLong() ^ secMask;
			//0x14	4	Page header Checksum (section page checksum calculated from unencoded header bytes, with the data checksum as seed)
			var checksum = sreader.ReadRawLong() ^ secMask;
			section.Offset = (ulong)(checksum + startOffset);

			//0x18	4	Data Checksum (section page checksum calculated from compressed data bytes, with seed 0)
			section.Checksum = (uint)(sreader.ReadRawLong() ^ secMask);
			//0x1C	4	Unknown (ODA writes a 0)
			var oda = (uint)(sreader.ReadRawLong() ^ secMask);
		}

		private Stream getSectionBuffer21(DwgFileHeaderAC21 fileheader, string sectionName)
		{
			Stream stream = null;

			if (!fileheader.Descriptors.TryGetValue(sectionName, out DwgSectionDescriptor section))
				return null;

			//Get the total lenght of all uncompressed pages
			ulong totalLength = 0;
			foreach (DwgLocalSectionMap page in section.LocalSections)
				totalLength += page.DecompressedSize;

			//Total buffer for the page
			byte[] pagesBuffer = new byte[totalLength];

			long currOffset = 0;
			foreach (DwgLocalSectionMap page in section.LocalSections)
			{
				if (page.IsEmpty)
				{
					//Page is empty, fill the gap with 0s
					for (int i = 0; i < (int)page.DecompressedSize; ++i)
						pagesBuffer[(int)currOffset++] = 0;
				}
				else
				{
					//Get the page data
					DwgSectionLocatorRecord pageData = fileheader.Records[page.PageNumber];

					//Set the pointer on the current page
					this._fileStream.Position = pageData.Seeker + 0x480L;

					//Get the page data
					byte[] pageBytes = new byte[pageData.Size];
					this._fileStream.Stream.Read(pageBytes, 0, (int)pageData.Size);

					if (section.Encoding == 4)
					{
						//Encoded page, use reed solomon

						//Avoid shifted bits
						ulong v = page.CompressedSize + 7L;
						ulong v1 = v & 0b11111111_11111111_11111111_11111000L;

						int alignedPageSize = (int)((v1 + 251 - 1) / 251);
						byte[] arr = new byte[alignedPageSize * 251];

						this.reedSolomonDecoding(pageBytes, arr, alignedPageSize, 251);
						pageBytes = arr;
					}

					if ((long)page.CompressedSize != (long)page.DecompressedSize)
					{
						//Page is compressed
						byte[] arr = new byte[page.DecompressedSize];
						DwgLZ77AC21Decompressor.Decompress(pageBytes, 0U, (uint)page.CompressedSize, arr);
						pageBytes = arr;
					}

					for (int i = 0; i < (int)page.DecompressedSize; ++i)
						pagesBuffer[(int)currOffset++] = pageBytes[i];
				}
			}

			stream = new MemoryStream(pagesBuffer, 0, pagesBuffer.Length, false, true);

			return stream;
		}

		/// <summary>
		/// Apply a simple reed Solomon decoding to a byte array.
		/// </summary>
		/// <param name="encoded"></param>
		/// <param name="buffer"></param>
		/// <param name="factor"></param>
		/// <param name="blockSize"></param>
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
	}
}
