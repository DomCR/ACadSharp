using ACadSharp.DataStorage;
using System;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamReaders;

internal class DwgPrototype1bReaderV2 : DwgSectionIO
{
	public override string SectionName => DwgSectionDefinition.AcDsPrototype;

	private readonly DwgDocumentBuilder _builder;

	private readonly IDwgStreamReader _reader;

	private FileHeader _fileHeader;

	public DwgPrototype1bReaderV2(ACadVersion version, DwgDocumentBuilder builder, IDwgStreamReader reader) : base(version)
	{
		this._reader = reader;
		this._builder = builder;
	}

	public void Read()
	{
		CadFileDataStorage storage = new();

		try
		{
			this._fileHeader = this.readFileHeader();
			this.readFileSegmentIndex();

			this._builder.DataStorage = storage;
		}
		catch (Exception ex)
		{
			if (!this._builder.Configuration.Failsafe)
				throw;

			this.notify("An error occurred while reading the Prototype1b", NotificationType.Error, ex);
		}
	}

	private FileSegmentIndex readFileSegmentIndex()
	{
		this._reader.Position = this._fileHeader.SegmentIndexOffset;

		FileSegmentIndex segmentIndex = new();

		this.readDataStorageFileSegment(segmentIndex);

		for (int i = 0; i < this._fileHeader.SegmentIndexEntryCount; i++)
		{
			var offset = this._reader.ReadRawULong();
			var size = this._reader.ReadUInt();
			segmentIndex.AddEntry(offset, size);
		}

		return segmentIndex;
	}

	private void readDataStorageFileSegment(FileSegment segment)
	{
		var pos = this._reader.Position;
		segment.Header = this.readSubItemHeader();
	}

	private FileSegmentHeader readSubItemHeader()
	{
		var header = new FileSegmentHeader();

		//Int16 Signature (always 0xd5ac?)
		header.Signature = this._reader.ReadShort();
		//byte[6] Name (6 bytes). Names for the several file segments are:
		header.Name = Encoding.ASCII.GetString(this._reader.ReadBytes(6));
		//Int32 Segment index
		header.SegmentIndex = this._reader.ReadUInt();
		//Int32 Unknown 1 (0 or 1? 1 in blob01 segment).
		header.IsBlob = this._reader.ReadInt();
		//Int32 Segment size (multiple of 0x40 bytes (AutoCAD uses 0x80), padded with 0x70 values).
		header.SegmentSize = this._reader.ReadUInt();
		//Int32 Unknown 2 (always 0?)
		header.Unknown2 = this._reader.ReadInt();
		//Int32 Data storage revision
		header.DataStorageRevision = this._reader.ReadInt();
		//Int32 Unknown 3 (always 0?)
		header.Unknown3 = this._reader.ReadInt();
		//Int32 System data alignment offset (calculate the stream position by shifting left 4
		//bits and adding to the file segment’s stream start position). This offset is used
		//for schema index and schema data segments.So the name “system data” seems
		//to refer to schema index/schema data.
		header.SystemDataAlignmentOffset = this._reader.ReadInt();
		//Int32 Object data alignment offset (calculate the stream position by shifting left 4
		//bits and adding to the file segment’s stream start position). This offset is used
		//for the data segment.
		header.ObjectDataAlignmentOffset = this._reader.ReadInt();
		//byte[8] 8 alignment bytes (always 8 x 0x55?).
		var alignmentBytes = this._reader.ReadBytes(8);

		return header;
	}

	private FileHeader readFileHeader()
	{
		FileHeader fileHeader = new();

		//UInt32 File signature
		fileHeader.FileSignature = this._reader.ReadUInt();
		//Int32 File header size
		fileHeader.FileHeaderSize = this._reader.ReadInt();
		//Int32 Unknown 1 (always 2?)
		fileHeader.Unknown1 = this._reader.ReadInt();
		//Int32 Version(always 2 ?)
		fileHeader.Version = this._reader.ReadInt();
		//Int32 Unknown 2 (always 0?)
		fileHeader.Unknown2 = this._reader.ReadInt();
		//Int32 Data storage revision
		fileHeader.DataStorageRevision = this._reader.ReadInt();
		//Int32 Segment index offset (the stream off set from the data store’s stream start
		//position). See paragraph 24.2.2.1 for the segment index file segment.
		fileHeader.SegmentIndexOffset = this._reader.ReadInt();
		//Int32 Segment index unknown
		fileHeader.SegmentIndexUnknown = this._reader.ReadInt();
		//Int32 Segment index entry count
		fileHeader.SegmentIndexEntryCount = this._reader.ReadInt();
		//Int32 Schema index segment index. This is the index into the segment index entry
		//array(see paragraph 24.2.2.1) for the schema index file segment(see paragraph 24.2.2.4).
		fileHeader.SchemaIndexSegmentIndex = this._reader.ReadInt();
		//Int32 Data index segment index. This is the index into the segment index entry array
		//(see paragraph 24.2.2.1) for the data index file segment(see paragraph 24.2.2.2).
		fileHeader.DataIndexSegmentIndex = this._reader.ReadInt();
		//Int32 Search segment index
		fileHeader.SearchSegmentIndex = this._reader.ReadInt();
		//Int32 Previous save index
		fileHeader.PreviousSaveIndex = this._reader.ReadInt();
		//Int32 File size
		fileHeader.FileSize = this._reader.ReadInt();

		return fileHeader;
	}
}