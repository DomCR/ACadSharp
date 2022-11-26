using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.DwgStreamWriters;
using CSUtilities.IO;
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

		private Dictionary<ulong, long> _handlesMap = new Dictionary<ulong, long>();

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
			this.writeHeader();
			this.writeClasses();
			//this.writeEmptySection();
			this.writeSummaryInfo();
			this.writePreview();
			//this.writeVBASection();
			this.writeAppInfo();
			//this.writeFileDepList();
			//this.writeRevHistory();
			//this.writeSecurity();
			this.writeObjects();
			//this.writeObjFreeSpace();
			//this.writeTemplate();
			this.writeHandles();
			//this.writePrototype();
			this.writeAuxHeader();

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

		private void writeClasses()
		{
			MemoryStream stream = new MemoryStream();
			DwgClassesWriter writer = new DwgClassesWriter(this._document, this._version, stream);
			writer.Write();

			this._fileHeaderWriter.CreateSection(DwgSectionDefinition.Classes, stream, false);
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

		private void writeObjects()
		{
			MemoryStream stream = new MemoryStream();
			DwgObjectWriter writer = new DwgObjectWriter(stream, this._document);
			writer.Write();
			this._handlesMap = writer.Map;

			this._fileHeaderWriter.CreateSection(DwgSectionDefinition.AcDbObjects, stream, true);
		}

		private void writeHandles()
		{
			MemoryStream stream = new MemoryStream();
			DwgHandleWriter writer = new DwgHandleWriter(this._version, stream, this._handlesMap);
			writer.Write();

			this._fileHeaderWriter.CreateSection(DwgSectionDefinition.Handles, stream, true);
		}

		private void writeAuxHeader()
		{
			MemoryStream stream = new MemoryStream();
			DwgAuxHeaderWriter writer = new DwgAuxHeaderWriter(stream, this._document.Header);
			writer.Write();

			this._fileHeaderWriter.CreateSection(DwgSectionDefinition.AuxHeader, stream, true);
		}
	}
}
