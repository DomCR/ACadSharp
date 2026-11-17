using ACadSharp.DataStorage;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal class DwgPrototype1bWriter : DwgSectionIO
{
	public override string SectionName => DwgSectionDefinition.AcDsPrototype;

	private readonly DataIndexSegment _dataIndex;

	private readonly CadFileDataStorage _dataStorage;

	private readonly FileHeader _fileHeader = new FileHeader();

	private readonly MemoryStream _stream;

	private readonly IDwgStreamWriter _writer;

	private uint _currentSegmentIndex = 0;

	private uint StartPosition = 0;

	public DwgPrototype1bWriter(ACadVersion version, MemoryStream stream, Encoding encoding, CadFileDataStorage dataStorage)
				: base(version)
	{
		this._stream = stream;
		this._dataStorage = dataStorage;

		this._writer = DwgStreamWriterBase.GetStreamWriter(this._version, this._stream, encoding);

		this._dataIndex = new DataIndexSegment();
	}

	public void Write()
	{
		for (int i = 0; i < 128; i++)
		{
			this._writer.WriteByte(0);
		}

		this.writeDataFields();

		this._writer.SetPositionInBits(0);
		this.writeFileHeader(this._fileHeader);
	}

	private static uint getPaddingSize(uint length, int alignment)
	{
		return (uint)(alignment - 1 - (long)(length - 1) % (long)alignment);
	}

	private void writeDataField(DataField dataField)
	{
		writeDataStorageFileSegment(dataField);

		throw new NotImplementedException();
	}

	private void writeDataFields()
	{
		DataField dataField = new DataField();
		foreach (AcdsRecord item in this._dataStorage.Records)
		{
			DataField.DataEntry entry = new();
			entry.Header.Handle = item.Columns[CadFileDataStorage.AcDbDsId].Handle;
			entry.Data = item.Columns[CadFileDataStorage.AsmData].Data.ToArray();

			dataField.DataEntries.Add(entry);
		}

		this.writeDataField(dataField);
	}

	private void writeDataStorageFileSegment(FileSegment segment)
	{
		long offset = this._writer.Stream.Position - this.StartPosition;
		segment.Header.SegmentIndex = this._currentSegmentIndex++;

		var fileSegmentHeaderPosition = this._writer.Stream.Position;
		this.writeSubItemHeader(segment.Header);//size is header + name bytes

		//Write header body

		uint segmentLength = (uint)((ulong)this._writer.Stream.Position - this.StartPosition);
		uint paddingSize = getPaddingSize(segmentLength, 128);
		segment.Header.SegmentSize = segmentLength + paddingSize;

		for (int i = 0; i < paddingSize; i++)
		{
			this._writer.WriteByte(0x70);
		}

		long endPosition = this._writer.Stream.Position;
		this._writer.Stream.Position = (long)this.StartPosition;
		this.writeSubItemHeader(segment.Header);
		this._writer.Stream.Position = endPosition;
	}

	private void writeFileHeader(FileHeader fileHeader)
	{
		//UInt32 File signature
		this._writer.WriteUInt(fileHeader.FileSignature);
		//Int32 File header size
		this._writer.WriteInt(fileHeader.FileHeaderSize);
		//Int32 Unknown 1 (always 2?)
		this._writer.WriteInt(fileHeader.Unknown1);
		//Int32 Version(always 2 ?)
		this._writer.WriteInt(fileHeader.Version);
		//Int32 Unknown 2 (always 0?)
		this._writer.WriteInt(fileHeader.Unknown2);
		//Int32 Data storage revision
		this._writer.WriteInt(fileHeader.DataStorageRevision);
		//Int32 Segment index offset (the stream off set from the data store’s stream start
		//position). See paragraph 24.2.2.1 for the segment index file segment.
		this._writer.WriteInt(fileHeader.SegmentIndexOffset);
		//Int32 Segment index unknown
		this._writer.WriteInt(fileHeader.SegmentIndexUnknown);
		//Int32 Segment index entry count
		this._writer.WriteInt(fileHeader.SegmentIndexEntryCount);
		//Int32 Schema index segment index. This is the index into the segment index entry
		//array(see paragraph 24.2.2.1) for the schema index file segment(see paragraph 24.2.2.4).
		this._writer.WriteInt(fileHeader.SchemaIndexSegmentIndex);
		//Int32 Data index segment index. This is the index into the segment index entry array
		//(see paragraph 24.2.2.1) for the data index file segment(see paragraph 24.2.2.2).
		this._writer.WriteInt(fileHeader.DataIndexSegmentIndex);
		//Int32 Search segment index
		this._writer.WriteInt(fileHeader.SearchSegmentIndex);
		//Int32 Previous save index
		this._writer.WriteInt(fileHeader.PreviousSaveIndex);
		//Int32 File size
		this._writer.WriteInt(fileHeader.FileSize);
	}

	private void writeFileSegmentIndex()
	{
		throw new NotImplementedException();
	}

	private void writeSubItemHeader(FileSegmentHeader header)
	{
		this._writer.WriteRawShort(header.Signature);

		var name = Encoding.ASCII.GetBytes(header.Name);
		this._writer.WriteBytes(name, 0, name.Length);

		this._writer.WriteUInt(header.SegmentIndex);
		this._writer.WriteInt(header.IsBlob);
		this._writer.WriteUInt(header.SegmentSize);
		this._writer.WriteInt(header.Unknown2);
		this._writer.WriteInt(header.DataStorageRevision);
		this._writer.WriteInt(header.Unknown3);
		this._writer.WriteInt(header.SystemDataAlignmentOffset);
		this._writer.WriteInt(header.ObjectDataAlignmentOffset);

		for (int i = 0; i < 8; i++)
		{
			this._writer.WriteByte(0x55);
		}
	}
}