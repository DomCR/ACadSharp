using ACadSharp.Exceptions;
using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.DwgStreamWriters;
using CSUtilities.IO;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.IO;

namespace ACadSharp.IO
{
	public class DwgWriter : CadWriterBase
	{
		private ACadVersion _version { get { return this._document.Header.Version; } }

		private Stream _stream;

		private DwgFileHeader _fileHeader;

		private IDwgFileHeaderWriter _fileHeaderWriter;

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
			this._stream = stream;
			this._document = document;
			this._fileHeader = DwgFileHeader.CreateFileHeader(_version);
		}

		/// <inheritdoc/>
		public override void Write()
		{
			this.getFileHeaderWriter();

			this.writeHeader();
			this.writeClasses();
			//this.writeEmptySection();
			this.writeSummaryInfo();
			this.writePreview();
			//this.writeVBASection();
			this.writeAppInfo();
			this.writeFileDepList();
			this.writeRevHistory();
			//this.writeSecurity();
			this.writeAuxHeader();
			this.writeObjects();
			this.writeObjFreeSpace();
			this.writeTemplate();
			//this.writePrototype();

			//Write in the last place to avoid conflicts with versions < AC1018
			this.writeHandles();

			this._fileHeaderWriter.WriteFile();
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			this._stream.Dispose();
		}

		private void getFileHeaderWriter()
		{
			switch (this._document.Header.Version)
			{
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
				case ACadVersion.AC1012:
					throw new DwgNotSupportedException(this._document.Header.Version);
				case ACadVersion.AC1014:
				case ACadVersion.AC1015:
					this._fileHeaderWriter = new DwgFileHeaderWriterAC15(_stream, _document);
					break;
				case ACadVersion.AC1018:
					this._fileHeaderWriter = new DwgFileHeaderWriterAC18(_stream, _document);
					break;
				case ACadVersion.AC1021:
					throw new DwgNotSupportedException(this._document.Header.Version);
					//this._fileHeaderWriter = new DwgFileHeaderWriterAC21(_stream, _document);
					//break;
				case ACadVersion.AC1024:
				case ACadVersion.AC1027:
				case ACadVersion.AC1032:
					throw new DwgNotSupportedException(this._document.Header.Version);
					//this._fileHeaderWriter = new DwgFileHeaderWriterAC18(_stream, _document);
					//break;
				case ACadVersion.Unknown:
				default:
					throw new DwgNotSupportedException();
			}
		}

		private void writeHeader()
		{
			MemoryStream stream = new MemoryStream();
			DWG.DwgHeaderWriter writer = new DWG.DwgHeaderWriter(stream, this._document);
			writer.OnNotification += triggerNotification;
			writer.Write();

			this._fileHeaderWriter.AddSection(DwgSectionDefinition.Header, stream, true);
		}

		private void writeClasses()
		{
			MemoryStream stream = new MemoryStream();
			DwgClassesWriter writer = new DwgClassesWriter(this._document, this._version, stream);
			writer.Write();

			this._fileHeaderWriter.AddSection(DwgSectionDefinition.Classes, stream, false);
		}

		private void writeSummaryInfo()
		{
			///<see cref="DwgReader.ReadSummaryInfo"/>

			if (this._fileHeader.AcadVersion < ACadVersion.AC1018)
				return;

			MemoryStream stream = new MemoryStream();
			var writer = DwgStreamWriterBase.GetStreamHandler(_version, stream, TextEncoding.Windows1252());

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

			writer.Write8BitJulianDate(info.CreatedDate);
			writer.Write8BitJulianDate(info.ModifiedDate);

			//Int16	2 + 2 * (2 + n)	Property count, followed by PropertyCount key/value string pairs.
			writer.WriteRawShort((ushort)info.Properties.Count);
			foreach (KeyValuePair<string, string> property in info.Properties)
			{
				writer.WriteTextUnicode(property.Key);
				writer.WriteTextUnicode(property.Value);
			}

			writer.WriteInt(0);
			writer.WriteInt(0);

			this._fileHeaderWriter.AddSection(DwgSectionDefinition.SummaryInfo, stream, false, 0x100);
		}

		private void writePreview()
		{
			MemoryStream stream = new MemoryStream();
			DwgPreviewWriter writer = new DwgPreviewWriter(this._version, stream);
			writer.Write();

			this._fileHeaderWriter.AddSection(DwgSectionDefinition.Preview, stream, false, 0x400);
		}

		private void writeAppInfo()
		{
			if (this._fileHeader.AcadVersion < ACadVersion.AC1018)
				return;

			MemoryStream stream = new MemoryStream();
			DwgAppInfoWriter writer = new DwgAppInfoWriter(this._version, stream);
			writer.Write();

			this._fileHeaderWriter.AddSection(DwgSectionDefinition.AppInfo, stream, false, 0x80);
		}

		private void writeFileDepList()
		{
			if (this._fileHeader.AcadVersion < ACadVersion.AC1018)
				return;

			MemoryStream stream = new MemoryStream();
			StreamIO swriter = new StreamIO(stream);

			//nt32	4	Feature count(ftc)
			swriter.Write<uint>(0);

			//String32	ftc * (4 + n)	Feature name list.A feature name is one of the following:
			/*
			 * “Acad: XRef” (for block table record)
			 * “Acad: Image” (for image definition)
			 * “Acad: PlotConfig” (for plotsetting)
			 * “Acad: Text” (for text style)
			*/

			//Int32	4	File count
			swriter.Write<uint>(0);

			//Then follows an array of features(repeated file count times). The feature name + the full filename constitute the lookup key of a file dependency:

			//String32	4 + n	Full filename
			//String32	4 + n	Found path, path at which file was found
			//String32	4 + n	Fingerprint GUID(applies to xref’s only)
			//String32	4 + n	Version GUID(applies to xref’s only)
			//Int32	4	Feature index in the feature list above.
			//Int32	4	Timestamp(Seconds since 1 / 1 / 1980)
			//Int32	4	Filesize
			//Int16	2	Affects graphics(1 = true, 0 = false)
			//Int32	4	Reference count

			this._fileHeaderWriter.AddSection(DwgSectionDefinition.FileDepList, stream, false, 0x80);
		}

		private void writeRevHistory()
		{
			if (this._fileHeader.AcadVersion < ACadVersion.AC1018)
				return;

			MemoryStream stream = new MemoryStream();
			StreamIO swriter = new StreamIO(stream);
			swriter.Write<uint>(0);
			swriter.Write<uint>(0);
			swriter.Write<uint>(0);
			this._fileHeaderWriter.AddSection(DwgSectionDefinition.RevHistory, stream, true);
		}

		private void writeObjects()
		{
			MemoryStream stream = new MemoryStream();
			DwgObjectWriter writer = new DwgObjectWriter(stream, this._document);
			writer.Write();
			this._handlesMap = writer.Map;

			this._fileHeaderWriter.AddSection(DwgSectionDefinition.AcDbObjects, stream, true);
		}

		private void writeObjFreeSpace()
		{
			MemoryStream stream = new MemoryStream();
			StreamIO writer = new StreamIO(stream);

			//Int32	4	0
			writer.Write<int>(0);
			//UInt32	4	Approximate number of objects in the drawing(number of handles).
			writer.Write<uint>((uint)this._handlesMap.Count);

			//Julian datetime	8	If version > R14 then system variable TDUPDATE otherwise TDUUPDATE.
			if (_version >= ACadVersion.AC1015)
			{
				CadUtils.DateToJulian(this._document.Header.UniversalUpdateDateTime, out int jdate, out int mili);
				writer.Write<int>(jdate);
				writer.Write<int>(mili);
			}
			else
			{
				CadUtils.DateToJulian(this._document.Header.UpdateDateTime, out int jdate, out int mili);
				writer.Write<int>(jdate);
				writer.Write<int>(mili);
			}

			//UInt32	4	Offset of the objects section in the stream.
			writer.Write<uint>(0);  //It may be the cause of failure for version AC1024

			//UInt8	1	Number of 64 - bit values that follow(ODA writes 4).
			writer.Stream.WriteByte(4);
			//UInt32	4	ODA writes 0x00000032
			writer.Write<uint>(0x00000032);
			//UInt32	4	ODA writes 0x00000000
			writer.Write<uint>(0x00000000);
			//UInt32	4	ODA writes 0x00000064
			writer.Write<uint>(0x00000064);
			//UInt32	4	ODA writes 0x00000000
			writer.Write<uint>(0x00000000);
			//UInt32	4	ODA writes 0x00000200
			writer.Write<uint>(0x00000200);
			//UInt32	4	ODA writes 0x00000000
			writer.Write<uint>(0x00000000);
			//UInt32	4	ODA writes 0xffffffff
			writer.Write<uint>(0xffffffff);
			//UInt32	4	ODA writes 0x00000000
			writer.Write<uint>(0x00000000);

			this._fileHeaderWriter.AddSection(DwgSectionDefinition.ObjFreeSpace, stream, true);
		}

		private void writeTemplate()
		{
			MemoryStream stream = new MemoryStream();
			StreamIO writer = new StreamIO(stream);

			//Int16	2	Template description string length in bytes(the ODA always writes 0 here).
			writer.Write<short>(0);
			//UInt16	2	MEASUREMENT system variable(0 = English, 1 = Metric).
			writer.Write<ushort>((ushort)1);

			this._fileHeaderWriter.AddSection(DwgSectionDefinition.Template, stream, true);
		}

		private void writeHandles()
		{
			MemoryStream stream = new MemoryStream();
			DwgHandleWriter writer = new DwgHandleWriter(this._version, stream, this._handlesMap);
			writer.Write(this._fileHeaderWriter.HandleSectionOffset);

			this._fileHeaderWriter.AddSection(DwgSectionDefinition.Handles, stream, true);
		}

		private void writeAuxHeader()
		{
			MemoryStream stream = new MemoryStream();
			DwgAuxHeaderWriter writer = new DwgAuxHeaderWriter(stream, this._document.Header);
			writer.Write();

			this._fileHeaderWriter.AddSection(DwgSectionDefinition.AuxHeader, stream, true);
		}
	}
}
