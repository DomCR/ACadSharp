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

		private CadDocument _document;

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
			_stream = new StreamIO(stream);
			this._document = document;
			this._fileHeader = DwgFileHeader.GetFileHeader(_version);
			this._writer = DwgStreamWriterBase.GetStreamHandler(_version, this._stream.Stream, Encoding.Default);
		}

		/// <inheritdoc/>
		public override void Write()
		{
			//0x00	6	“ACXXXX” version string
			//this._stream.Write(CadUtils.GetNameFromVersion(this._document.Header.Version), Encoding.ASCII);

			this.writeSummaryInfo();
			this.writePreview();
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			this._stream.Dispose();
		}

		private void writeSummaryInfo()
		{
			///<see cref="DwgReader.ReadSummaryInfo"/>

			if (this._fileHeader.AcadVersion < ACadVersion.AC1018)
				return;

			CadSummaryInfo info = this._document.SummaryInfo;
			
			_writer.WriteTextUnicode(info.Title);
			_writer.WriteTextUnicode(info.Subject);
			_writer.WriteTextUnicode(info.Author);
			_writer.WriteTextUnicode(info.Keywords);
			_writer.WriteTextUnicode(info.Comments);
			_writer.WriteTextUnicode(info.LastSavedBy);
			_writer.WriteTextUnicode(info.RevisionNumber);
			_writer.WriteTextUnicode(info.HyperlinkBase);

			//?	8	Total editing time(ODA writes two zero Int32’s)
			_writer.WriteInt(0);
			_writer.WriteInt(0);

			_writer.WriteDateTime(info.CreatedDate);
			_writer.WriteDateTime(info.ModifiedDate);

			//Int16	2 + 2 * (2 + n)	Property count, followed by PropertyCount key/value string pairs.
			_writer.WriteRawShort((ushort)info.Properties.Count);
			foreach (KeyValuePair<string, string> property in info.Properties)
			{
				_writer.WriteTextUnicode(property.Key);
				_writer.WriteTextUnicode(property.Value);
			}

			_writer.WriteInt(0);
			_writer.WriteInt(0);
		}

		private void writePreview()
		{
			DwgPreviewWriter writer = new DwgPreviewWriter(this._version, _writer);
			writer.Write();
		}

		private void writeFileMetaData()
		{
#if METADA_WRITER
			IDwgStreamWriter swriter = DwgStreamWriterBase.GetStreamHandler(_version, this._stream.Stream, Encoding.Default);

			//5 bytes of 0x00 
			swriter.WriteBytes(new byte[5]);

			//0x0B	1	Maintenance release version
			swriter.WriteByte((byte)this._document.Header.MaintenanceVersion);
			//0x0C	1	Byte 0x00, 0x01, or 0x03
			swriter.WriteByte((byte)0x03);
			//0x0D	4	Preview address(long), points to the image page + page header size(0x20).
			swriter.WriteRawLong();
			//0x11	1	Dwg version (Acad version that writes the file)
			fileheader.DwgVersion = sreader.ReadByte();
			//0x12	1	Application maintenance release version(Acad maintenance version that writes the file)
			fileheader.AppReleaseVersion = sreader.ReadByte();

			//0x13	2	Codepage
			fileheader.DrawingCodePage = (CodePage)sreader.ReadShort();
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

			//0x2C	0x54	0x00 bytes
			sreader.ReadRawLong();
			//Get to offset 0x80
			sreader.Advance(80);
#endif
		}
	}
}
