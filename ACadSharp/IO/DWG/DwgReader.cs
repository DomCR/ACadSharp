using ACadSharp.Classes;
using ACadSharp.Header;
using CSUtilities.IO;
using CSUtilities.Converters;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ACadSharp.Exceptions;
using ACadSharp.IO.DWG;

namespace ACadSharp.IO
{
	public class DwgReader : CadReaderBase
	{
		public DwgReaderConfiguration Configuration { get; set; } = new DwgReaderConfiguration();

		private DwgDocumentBuilder _builder;

		private DwgFileHeader _fileHeader;

		private CadDocument _document;

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
			CadDocument doc = null;

			using (DwgReader reader = new DwgReader(stream, notification))
			{
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
			this._document = new CadDocument(false);
			this._builder = new DwgDocumentBuilder(this._document, this.Configuration);
			this._builder.OnNotification += this.onNotificationEvent;

			//Read the file header
			this._fileHeader = this.readFileHeader();

			this._document.SummaryInfo = this.ReadSummaryInfo();
			this._document.Header = this.ReadHeader();
			this._document.Classes = this.readClasses();

			//Read all the objects in the file
			this.readObjects();

			//Build the document 
			this._builder.BuildDocument();

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

			IDwgStreamReader reader = this.getSectionStream("AcDb:SummaryInfo");
			if (reader == null)
				return null;

			CadSummaryInfo summary = new CadSummaryInfo();

			//This section contains summary information about the drawing. 
			//Strings are encoded as a 16-bit length, followed by the character bytes (0-terminated).

			//String	2 + n	Title
			summary.Title = reader.ReadTextUnicode();
			//String	2 + n	Subject
			summary.Subject = reader.ReadTextUnicode();
			//String	2 + n	Author
			summary.Author = reader.ReadTextUnicode();
			//String	2 + n	Keywords
			summary.Keywords = reader.ReadTextUnicode();
			//String	2 + n	Comments
			summary.Comments = reader.ReadTextUnicode();
			//String	2 + n	LastSavedBy
			summary.LastSavedBy = reader.ReadTextUnicode();
			//String	2 + n	RevisionNumber
			summary.RevisionNumber = reader.ReadTextUnicode();
			//String	2 + n	RevisionNumber
			summary.HyperlinkBase = reader.ReadTextUnicode();

			//?	8	Total editing time(ODA writes two zero Int32’s)
			reader.ReadInt();
			reader.ReadInt();

			//Julian date	8	Create date time
			summary.CreatedDate = reader.Read8BitJulianDate();  //{12/13/2006 01:38:03}

			//Julian date	8	Modified date timez
			summary.ModifiedDate = reader.Read8BitJulianDate();

			//Int16	2 + 2 * (2 + n)	Property count, followed by PropertyCount key/value string pairs.
			short nproperties = reader.ReadShort();
			for (int i = 0; i < nproperties; i++)
			{
				string propName = reader.ReadTextUnicode();
				string propValue = reader.ReadTextUnicode();

				//Add the property
				summary.Properties.Add(propName, propValue);
			}

			//Int32	4	Unknown(write 0)
			reader.ReadInt();
			//Int32	4	Unknown(write 0)
			reader.ReadInt();

			return summary;
		}

		/// <summary>
		/// Read the preview image of the dwg file.
		/// </summary>
		/// <remarks>
		/// Refers to AcDb:Preview data section.
		/// </remarks>
		/// <returns></returns>
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
			IDwgStreamReader sreader = this.getSectionStream(DwgSectionDefinition.Header);

			DwgHeaderReader hreader = new DwgHeaderReader(this._fileHeader.AcadVersion, sreader);
			hreader.OnNotification += onNotificationEvent;

			CadHeader header = hreader.Read(this._fileHeader.AcadMaintenanceVersion, out DwgHeaderHandlesCollection headerHandles);

			if (this._builder != null)
				this._builder.HeaderHandles = headerHandles;

			return header;
		}

		/// <summary>
		/// Read the file header data.
		/// </summary>
		/// <returns></returns>
		internal DwgFileHeader readFileHeader()
		{
			//Reset the stream position at the begining
			this._fileStream.Position = 0L;

			//0x00	6	“ACXXXX” version string
			ACadVersion version = CadUtils.GetVersionFromName(this._fileStream.ReadString(6, Encoding.ASCII));
			DwgFileHeader fileHeader = DwgFileHeader.CreateFileHeader(version);

			//Get the stream reader
			IDwgStreamReader sreader = DwgStreamReaderBase.GetStreamHandler(fileHeader.AcadVersion, this._fileStream.Stream);

			//Read the file header
			switch (fileHeader.AcadVersion)
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
				default:
					break;
			}

			return fileHeader;
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

			DwgObjectSectionReader sectionReader = new DwgObjectSectionReader(
				this._fileHeader.AcadVersion,
				this._builder,
				sreader,
				objectHandles,
				handles,
				this._document.Classes);

			sectionReader.Read();
		}

		#region File Header reading methods

		/// <summary>
		/// Read the file header for the AC1012 to AC1015 (R13-R15) versions of the header.
		/// </summary>
		/// <param name="fileheader">File header to read</param>
		/// <param name="sreader"></param>
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
			sreader.Encoding = TextEncoding.GetListedEncoding(fileheader.DrawingCodePage);

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
			sreader.ResetShift();

			var sn = sreader.ReadSentinel();
			if (!DwgSectionIO.CheckSentinel(sn, DwgFileHeaderAC15.EndSentinel))
			{
				this.triggerNotification($"Invalid section sentinel found in FileHeader", NotificationType.Warning);
			}
		}

		/// <summary>
		/// Read the file header for the AC1018 (2004-2006) version of the header.
		/// </summary>
		/// <param name="fileheader">File header to read</param>
		/// <param name="sreader"></param>
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

			sreader.ReadBytes(20);  //CHECK IF IS USEFUL

			#region Read header encrypted data

			//0x00	12	“AcFssFcAJMB” file ID string
			string fileId = headerStream.ReadString(12);
			if (fileId != "AcFssFcAJMB\0")
			{
				this.triggerNotification($"File validation failed, id should be : AcFssFcAJMB\0, but is : {fileId}", NotificationType.Warning);
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
			StreamIO decompressed = new StreamIO(Dwg2004LZ77.Decompress(sreader.Stream, decompressedSize));

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
			//Get the page size
			this.getPageHeaderData(sreader, out _, out decompressedSize, out _, out _, out _);
			StreamIO decompressedStream = new StreamIO(Dwg2004LZ77.Decompress(sreader.Stream, decompressedSize));
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

		/// <summary>
		/// Read the file header for the AC1021 (2007-2009) version of the header.
		/// </summary>
		/// <param name="fileheader">File header to read</param>
		/// <param name="sreader"></param>
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
				DwgR21LZ77.Decompress(decodedData, 32U, (uint)comprLen, buffer);
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

#if false
//this code it doesn't take any effect on the reading
					//Create an empty page to fill the gap
					if (currentOffset < page.Offset)
					{
						ulong decompressedSize = page.Offset - currentOffset;
						DwgLocalSectionMap emptyPage = new DwgLocalSectionMap();
						emptyPage.IsEmpty = true;
						emptyPage.Offset = currentOffset;
						emptyPage.CompressedSize = 0;
						emptyPage.DecompressedSize = decompressedSize;

						//Add the empty local section to the current descriptor
						section.LocalSections.Add(emptyPage);
					}
#endif
					//Add the page to the section
					section.LocalSections.Add(page);
					//Move the offset
					currentOffset = page.Offset + page.DecompressedSize;
				}
				if (sectionNameLength > 0)
					fileheader.Descriptors.Add(section.Name, section);
			}
		}

		/// <summary>
		/// Read the metadata from the file.
		/// </summary>
		/// <param name="fileheader">File header where the data will be stored</param>
		/// <param name="sreader"></param>
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
			sreader.Encoding = TextEncoding.GetListedEncoding(fileheader.DrawingCodePage);

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

		#endregion

		private IDwgStreamReader getSectionStream(string sectionName)
		{
			Stream sectionStream = null;
			Encoding encoding = null;
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
					encoding = TextEncoding.GetListedEncoding((this._fileHeader as DwgFileHeaderAC15).DrawingCodePage);
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
			if (encoding != null)
				streamHandler.Encoding = encoding;

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
						Dwg2004LZ77.DecompressToDest(this._fileStream.Stream, memoryStream);
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
						DwgR21LZ77.Decompress(pageBytes, 0U, (uint)page.CompressedSize, arr);
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

		/// <summary>
		/// Get the buffer to read the dwg page.
		/// </summary>
		/// <param name="pageOffset"></param>
		/// <param name="compressedSize"></param>
		/// <param name="uncompressedSize"></param>
		/// <param name="correctionFactor"></param>
		/// <param name="blockSize"></param>
		/// <param name="stream"></param>
		/// <returns></returns>
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

			DwgR21LZ77.Decompress(compressedData, 0U, (uint)compressedSize, decompressedData);

			return decompressedData;
		}
	}
}
