using ACadSharp.DataStorage;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal class DwgPrototype1bWriter : DwgSectionIO
{
	public override string SectionName => DwgSectionDefinition.AcDsPrototype;

	private readonly CadFileDataStorage _dataStorage;

	private readonly FileHeader _fileHeader = new FileHeader();

	private readonly MemoryStream _stream;

	private readonly IDwgStreamWriter _writer;

	public DwgPrototype1bWriter(ACadVersion version, MemoryStream stream, Encoding encoding, CadFileDataStorage dataStorage)
	: base(version)
	{
		this._stream = stream;
		this._dataStorage = dataStorage;

		this._writer = DwgStreamWriterBase.GetStreamWriter(this._version, this._stream, encoding);
	}

	public void Write()
	{
		for (int i = 0; i < 128; i++)
		{
			this._writer.WriteByte(0);
		}

		this._writer.SetPositionInBits(0);
		this.writeFileHeader(this._fileHeader);
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
}