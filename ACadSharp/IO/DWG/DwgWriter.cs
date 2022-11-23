using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.DwgStreamWriters;
using CSUtilities.Converters;
using CSUtilities.IO;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACadSharp.IO
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
		public DwgWriter(string filename, CadDocument document)
			: this(File.Create(filename), document)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="document"></param>
		public DwgWriter(Stream stream, CadDocument document)
		{
			this._stream = new StreamIO(stream);
			this._document = document;
			this._fileHeader = DwgFileHeader.CreateFileHeader(_version);

			this._writer = DwgStreamWriterBase.GetStreamHandler(_version, this._stream.Stream, Encoding.Default);

			this._fileHeaderWriter = new DwgFileHeaderWriterAC18(document, stream, _version);
		}

		/// <inheritdoc/>
		public override void Write()
		{
			//this._fileHeaderWriter.CreateSection("", new System.IO.MemoryStream(), true);

			this.writeHeader();
			//this.writeClasses();
			//this.writeEmptySection();
			this.writeSummaryInfo();
			this.writePreview();
			//this.writeVBASection();
			this.writeAppInfo();
			//this.writeFileDepList();
			//this.writeRevHistory();
			//this.writeSecurity();
			//this.writeObjects();
			//this.writeObjFreeSpace();
			//this.writeTemplate();
			//this.writeHandles();
			//this.writePrototype();
			//this.writeAuxHeader();

			//this.writeFileHeader();

			this._fileHeaderWriter.WriteDescriptors();

			this._fileHeaderWriter.WriteRecords();

			this._fileHeaderWriter.WriteFileMetaData();
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			this._stream.Dispose();
		}

		private void writeHeader()
		{
			MemoryStream stream = new MemoryStream();
			DwgHeaderWriter writer = new DwgHeaderWriter(stream, this._document);
			writer.OnNotification += triggerNotification;
			writer.Write();

			this._fileHeaderWriter.CreateSection(DwgSectionDefinition.Header, stream, true);
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

			this._fileHeaderWriter.CreateSection(DwgSectionDefinition.SummaryInfo, stream, false, 256);
		}

		private void writePreview()
		{
			MemoryStream stream = new MemoryStream();
			DwgPreviewWriter writer = new DwgPreviewWriter(this._version, stream);
			writer.Write();

			this._fileHeaderWriter.CreateSection(DwgSectionDefinition.Preview, stream, false, 1024);
		}

		private void writeAppInfo()
		{
			MemoryStream stream = new MemoryStream();
			DwgAppInfodWriter writer = new DwgAppInfodWriter(this._version, stream);
			writer.Write();

			this._fileHeaderWriter.CreateSection(DwgSectionDefinition.AppInfo, stream, false, 128);
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
			swriter.WriteInt((int)this._descriptors[DwgSectionDefinition.AppInfo].LocalSections[0].Seeker + 32);
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
