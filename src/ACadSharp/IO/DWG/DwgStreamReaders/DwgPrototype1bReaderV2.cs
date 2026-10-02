using ACadSharp.DataStorage;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamReaders;

internal class DwgPrototype1bReaderV2 : DwgSectionIO
{
	public override string SectionName => DwgSectionDefinition.AcDsPrototype;

	private readonly DwgDocumentBuilder _builder;

	private readonly IDwgStreamReader _reader;

	private FileHeader _fileHeader;

	private FileSegmentIndex _fileSegmentIndex;

	private SchemaIndex _schemaIndex;

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
			this._fileSegmentIndex = this.readFileSegmentIndex();
			this._schemaIndex = this.readSchemaIndex();

			var schemes = this.readSchemaDataEntries();
			storage.Schemes.AddRange(schemes.SelectMany(s => s.Schemes));

			this.readDataIndex();

			this._builder.DataStorage = storage;
		}
		catch (Exception ex)
		{
			if (!this._builder.Configuration.Failsafe)
				throw;

			this.notify("An error occurred while reading the Prototype1b", NotificationType.Error, ex);
		}
	}

	private AcdsSchema readAcdsSchema()
	{
		AcdsSchema schema = new();

		ushort indexCount = BitConverter.ToUInt16(this._reader.ReadBytes(2), 0);
		for (int i = 0; i < indexCount; i++)
		{
			schema.Indices.Add(this._reader.ReadRawULong());
		}

		ushort propCount = BitConverter.ToUInt16(this._reader.ReadBytes(2), 0);
		for (int i = 0; i < propCount; i++)
		{
			schema.EmbeddedRecords.Add(this.readAcdsSchemaRecord());
		}

		return schema;
	}

	private AcdsSchemaRecord readAcdsSchemaRecord()
	{
		AcdsSchemaRecord record = new();

		record.Flags = (SchemaRecordFlags)this._reader.ReadInt();
		record.NameIndex = this._reader.ReadUInt();

		// Get type size
		if ((record.Flags & SchemaRecordFlags.NoType) == 0)
		{
			record.Type = this._reader.ReadUInt();
			if (record.Type == 0xE)
			{
				record.TypeSize = this._reader.ReadUInt();
			}
			else
			{
				record.TypeSize = AcdsSchemaRecord.TypeSizes[record.Type];
			}
		}

		// Read unknown fields
		if (record.Flags == SchemaRecordFlags.Unknown0)
		{
			record.Unknown1 = this._reader.ReadUInt();
		}
		else if (record.Flags == SchemaRecordFlags.Unknown1)
		{
			record.Unknown2 = this._reader.ReadUInt();
		}

		// Read values
		var valuesCount = BitConverter.ToUInt16(this._reader.ReadBytes(2), 0);
		if (record.TypeSize == 0)
		{
			return record;
		}

		for (int i = 0; i < valuesCount; i++)
		{
			record.Values.Add(this._reader.ReadBytes((int)record.TypeSize));
		}

		return record;
	}

	private void readDataIndex()
	{
		this._reader.Position = (long)this._fileSegmentIndex.Entries[(int)this._fileHeader.DataIndexSegmentIndex].Offset;

		DataIndexSegment dataIndex = new();

		this.readDataStorageFileSegment(dataIndex);

		//UInt32 Unknown property count
		int entryCount = this._reader.ReadInt();
		//UInt32 Unknown (0)
		var unknown1 = this._reader.ReadInt();
		for (int i = 0; i < entryCount; i++)
		{
			//UInt32 Segment index (0 means stub entry and can be ignored).
			uint segmentIndex = this._reader.ReadUInt();
			//UInt32 Local offset. This is a local offset in the stream, relative to the file segment’s
			//stream start position.This points to a data file segment, see paragraph 24.2.2.3.
			uint localOffset = this._reader.ReadUInt();
			//UInt32 Schema index
			uint schemaIndex = this._reader.ReadUInt();

			if (segmentIndex == 0)
			{
				continue;
			}

			dataIndex.Entries.Add(new DataIndexSegment.Entry
			{
				SegmentIndex = segmentIndex,
				LocalOffset = localOffset,
				SchemaIndex = schemaIndex,
			});
		}
	}

	private void readDataStorageFileSegment(FileSegment segment)
	{
		var pos = this._reader.Position;
		segment.Header = this.readSubItemHeader();
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

	private string readNullTerminatedString()
	{
		byte b;
		List<byte> bytes = [];
		while ((b = this._reader.ReadByte()) != 0)
		{
			bytes.Add(b);
		}
		return this._reader.Encoding.GetString(bytes.ToArray());
	}

	private SchemaData readSchemaData(long offset)
	{
		this._reader.Position = offset;

		var schemaData = new SchemaData();

		this.readDataStorageFileSegment(schemaData);

		long start = this._reader.Position;

		// Begin repeat schema unknown properties in the associated schema index file
		//segment(paragraph 24.2.2.4), where the property’s segment index is equal to
		//this file segment’s segment index(found in the header).
		var unknownPropertyCount = this._schemaIndex.PropertyPointers.Count(p => p.SegmentIndex == schemaData.Header.SegmentIndex);
		for (int i = 0; i < unknownPropertyCount; i++)
		{
			var dataSize = this._reader.ReadUInt();
			var unknownFlags = this._reader.ReadUInt();
		}

		// Read schemas
		foreach (var pointer in this._schemaIndex.SchemaPointers.Where(p => p.SegmentIndex == schemaData.Header.SegmentIndex))
		{
			AcdsSchema schema = this.readAcdsSchema();

			schema.Index = pointer.Index;
			schema.Name = this._schemaIndex.SchemaNames[(int)pointer.Index];
			schemaData.Schemes.Add(schema);
		}

		// Align to the next 16 byte boundary
		long boundary = this._reader.Position % 16;
		if (boundary != 0)
		{
			byte[] _ = this._reader.ReadBytes((int)(16 - boundary));
		}

		// Read schema property names
		uint propertyNameCount = this._reader.ReadUInt();
		string[] propertyNames = new string[propertyNameCount];
		for (int i = 0; i < propertyNameCount; i++)
		{
			propertyNames[i] = this.readNullTerminatedString();
		}

		// Assign schema Property names
		foreach (AcdsSchema schema in schemaData.Schemes)
		{
			foreach (AcdsSchemaRecord property in schema.EmbeddedRecords)
			{
				property.Name = propertyNames[(int)property.NameIndex];
			}
		}

		return schemaData;
	}

	private List<SchemaData> readSchemaDataEntries()
	{
		List<SchemaData> schemes = new();

		foreach (var item in this._schemaIndex.GetAllPointers())
		{
			var schema = this.readSchemaData((long)this._fileSegmentIndex.Entries[(int)item.SegmentIndex].Offset);
			schemes.Add(schema);
		}

		return schemes;
	}

	private SchemaIndex readSchemaIndex()
	{
		var startOffset = (long)this._fileSegmentIndex.Entries[(int)this._fileHeader.SchemaIndexSegmentIndex].Offset;
		this._reader.Position = startOffset;

		var schema = new SchemaIndex();

		this.readDataStorageFileSegment(schema);

		//UInt32 Unknown property count
		uint propertyCount = this._reader.ReadUInt();
		//UInt32 Unknown (0)
		var unknown0 = this._reader.ReadUInt();
		for (int i = 0; i < propertyCount; i++)
		{
			//UInt32 Index (starting at 0)
			var index = this._reader.ReadUInt();
			//UInt32 Segment index into the segment index file segment entry table (paragraph
			//24.2.2.1) of the schema data file segment (paragraph 24.2.2.6)
			var schemaIndex = this._reader.ReadUInt();
			//UInt32 Local offset of the unknown schema property. This is a local offset in the
			//stream, relative to the schema data file segment’s stream start position.
			var offset = this._reader.ReadUInt();
			schema.SchemaPointers.Add(new SchemaIndex.Pointer
			{
				Index = index,
				SegmentIndex = schemaIndex,
				LocalOffset = offset,
			});
		}

		//Int64 Unknown (0x0af10c)
		var unknown1 = this._reader.ReadRawULong();
		//UInt32 Property entry count
		uint propertyEntryCount = this._reader.ReadUInt();
		//UInt32 Unknown (0)
		uint unknownCount = this._reader.ReadUInt();
		for (int i = 0; i < propertyEntryCount; i++)
		{
			//UInt32 Segment index into the segment index file segment entry table (paragraph
			//24.2.2.1) of the schema data file segment(paragraph 24.2.2.6).
			var schemaIndex = this._reader.ReadUInt();
			//UInt32 Local offset of the schema property. This is a local offset in the stream, relative
			//to the schema data file segment’s stream start position.
			var offset = this._reader.ReadUInt();
			//UInt32 Index
			var index = this._reader.ReadUInt();
			schema.PropertyPointers.Add(new SchemaIndex.Pointer
			{
				SegmentIndex = schemaIndex,
				LocalOffset = offset,
				Index = index,
			});
		}

		//Begin repeat schema unknown properties in the associated schema index file
		//segment(paragraph 24.2.2.4), where the property’s segment index is equal to
		//this file segment’s segment index(found in the header).

		//Begin repeat schema entries in the associated schema index file segment
		//(paragraph 24.2.2.4), where the property’s segment index is equal to this file
		//segment’s segment index(found in the header).

		//A schema, see paragraph 24.2.2.6.1. The stream position is the file segment’s
		//start position + the schema entry’s local offset.
		var propertyNamesOffset = (long)schema.Header.SystemDataAlignmentOffset << 4;
		if (propertyNamesOffset != 0)
		{
			this._reader.Position = (long)(startOffset + propertyNamesOffset);
			//Uint32 Property name count
			var nproperties = this._reader.ReadUInt();
			for (int i = 0; i < nproperties; i++)
			{
				//AnsiString Property name (zero byte delimited). These names are referred to by the
				//schema’s schema property’s name index(paragraph 24.2.2.6.1.1).Name
				//strings can be shared between multiple schema properties this way.See
				//paragraph 24.2.2.6.1 for details about the schema.
				schema.SchemaNames.Add(this.readNullTerminatedString());
			}
		}

		return schema;
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
}