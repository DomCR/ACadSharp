using ACadSharp.DataStorage;
using CSUtilities.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal class DwgPrototype1bWriter : DwgSectionIO
{
	public override string SectionName => DwgSectionDefinition.AcDsPrototype;

	private const uint BlobReferenceSignature = 0xbb106bb1;

	private const byte DataAlignmentPadding = 0x62;

	private const uint DataHeaderSize = 20;

	private const uint DataIndexSegmentIndex = 1;

	private const uint DataSegmentIndex = 2;

	private const int DataStorageRevision = 1;

	private const int FileHeaderSize = 0x80;

	//"jard"
	private const uint FileSignature = 0x6472616A;

	private const uint FirstBlobSegmentIndex = 6;

	//Records bigger than this size are stored in blob01 segments, as AutoCAD does
	private const uint MaxInlineDataSize = 0x40000;

	private const byte SchemaAlignmentPadding = 0x73;

	private const uint SchemaDataSegmentIndex = 5;

	private const uint SchemaIndexSegmentIndex = 4;

	private const ulong SchemaIndexTag = 0x0af10c;

	private const uint SearchSegmentIndex = 3;

	private const int SegmentAlignment = 0x80;

	private const uint SegmentIndexSegmentIndex = 0;

	private const byte SegmentPadding = 0x70;

	private static readonly string[] _attributePropertyNames =
	[
		CadFileDataStorage.TreatedAsObjectData,
		CadFileDataStorage.Legacy,
		CadFileDataStorage.Indexable,
		CadFileDataStorage.HandleAttribute,
	];

	private static readonly string[] _indexedPropertyAttributes =
	[
		CadFileDataStorage.IndexedPropertySchemaName,
		CadFileDataStorage.HandleAttributeSchemaName,
	];

	private static readonly string[] _schemaAttributes =
	[
		CadFileDataStorage.TreatedAsObjectDataSchemaName,
		CadFileDataStorage.LegacySchemaName,
	];

	private readonly List<uint> _attributeValues = new();

	private readonly DataField _dataField = new();

	private readonly DataIndexSegment _dataIndex = new();

	private readonly CadFileDataStorage _dataStorage;

	private readonly FileHeader _fileHeader = new();

	private readonly List<string> _propertyNames = new();

	private readonly SchemaIndex _schemaIndex = new();

	private readonly List<AcdsSchema> _schemas = new();

	private readonly FileSegmentIndex _segmentIndex = new();

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
		this.buildSchemas();
		this.buildRecords();

		int blobCount = this._dataField.DataEntries.Sum(e => e.BlobReference?.Blobs.Count ?? 0);
		for (int i = 0; i < FirstBlobSegmentIndex + blobCount; i++)
		{
			this._segmentIndex.AddEntry(0, 0);
		}

		//Placeholder for the file header, written once all the segments are in place
		this.writePadding(0, FileHeaderSize);

		this.writeFileSegmentIndex();
		this.writeDataField();
		this.writeDataIndex();
		this.writeSearch();
		this.writeSchemaData();
		this.writeSchemaIndex();

		long fileSize = this._stream.Position;

		//Rewrite the segment index now that the location of all the segments is known
		this._stream.Position = FileHeaderSize;
		this.writeFileSegmentIndex();

		this._fileHeader.FileSignature = FileSignature;
		this._fileHeader.FileHeaderSize = FileHeaderSize;
		this._fileHeader.DataStorageRevision = DataStorageRevision;
		this._fileHeader.SegmentIndexOffset = FileHeaderSize;
		this._fileHeader.SegmentIndexEntryCount = this._segmentIndex.Entries.Count;
		this._fileHeader.SchemaIndexSegmentIndex = (int)SchemaIndexSegmentIndex;
		this._fileHeader.DataIndexSegmentIndex = (int)DataIndexSegmentIndex;
		this._fileHeader.SearchSegmentIndex = (int)SearchSegmentIndex;
		this._fileHeader.FileSize = (int)fileSize;

		this._stream.Position = 0;
		this.writeFileHeader(this._fileHeader);

		this._stream.Position = fileSize;
	}

	private static uint getPaddingSize(uint length, int alignment)
	{
		return (uint)(alignment - 1 - (long)(length - 1) % (long)alignment);
	}

	private static byte[] getValueBytes(ulong value, uint size)
	{
		byte[] bytes = LittleEndianConverter.Instance.GetBytes(value);
		Array.Resize(ref bytes, (int)size);
		return bytes;
	}

	private void alignPosition(long segmentStart, int alignment, byte padding)
	{
		uint length = (uint)(this._stream.Position - segmentStart);
		this.writePadding(padding, getPaddingSize(length, alignment));
	}

	private long beginSegment(FileSegment segment, uint segmentIndex)
	{
		long start = this._stream.Position;

		segment.Header.SegmentIndex = segmentIndex;
		segment.Header.DataStorageRevision = DataStorageRevision;
		this.writeSubItemHeader(segment.Header);

		return start;
	}

	private void buildRecords()
	{
		List<(uint SchemaIndex, ulong Handle, byte[] Data)> records = new();
		foreach (AcdsRecord record in this._dataStorage.Records)
		{
			if (!record.Columns.TryGetValue(CadFileDataStorage.AcDbDsId, out AcdsRecordColumn idColumn))
			{
				this.notify($"Data storage record without {CadFileDataStorage.AcDbDsId} column will not be written.", NotificationType.Warning);
				continue;
			}

			AcdsRecordColumn dataColumn = record.Columns.Values.FirstOrDefault(c => c.Data != null);
			if (dataColumn == null)
			{
				this.notify($"Data storage record for handle {idColumn.Handle:X} has no data and will not be written.", NotificationType.Warning);
				continue;
			}

			records.Add((record.Index, idColumn.Handle, dataColumn.Data.ToArray()));
		}

		//The record index used by the search segment is the position of the record in the data index
		uint blobSegmentIndex = FirstBlobSegmentIndex;
		foreach (var item in records.OrderBy(r => r.SchemaIndex).ThenBy(r => r.Handle))
		{
			DataField.DataEntry entry = new();
			entry.Header.Size = DataHeaderSize;
			entry.Header.Handle = item.Handle;
			entry.DataSize = (uint)item.Data.Length;

			if (entry.DataSize > MaxInlineDataSize)
			{
				Blob01 blob = new Blob01
				{
					TotalDataSize = entry.DataSize,
					PageStartOffset = 0,
					PageIndex = 0,
					PageCount = 1,
					PageDataSize = entry.DataSize,
					Data = item.Data,
				};
				blob.Header.IsBlob = 1;
				blob.Header.SegmentIndex = blobSegmentIndex++;

				entry.BlobReference = new DataField.DataBlobReference
				{
					TotalDataSize = entry.DataSize,
					PageCount = 1,
					PageSize = entry.DataSize,
					LastPageSize = entry.DataSize,
					Unknown1 = 0,
					Unknown2 = 0,
				};
				entry.BlobReference.Blobs.Add(blob);
				//Size of the reference without the signature: 32 bytes of fields + 8 bytes per page
				entry.BlobReference.RecordSize = 32 + 8 * entry.BlobReference.PageCount;
			}
			else
			{
				entry.Data = item.Data;
			}

			this._dataIndex.Entries.Add(new DataIndexSegment.Entry
			{
				SegmentIndex = DataSegmentIndex,
				LocalOffset = (uint)this._dataField.DataEntries.Count * DataHeaderSize,
				SchemaIndex = item.SchemaIndex,
			});
			this._dataField.DataEntries.Add(entry);
		}
	}

	private void buildSchemas()
	{
		List<AcdsSchema> source = this._dataStorage.Schemes.OrderBy(s => s.Index).ToList();

		foreach (AcdsSchema schema in source)
		{
			this._schemaIndex.SchemaNames.Add(schema.Name);
		}

		foreach (AcdsSchema schema in source)
		{
			AcdsSchema dwgSchema = new AcdsSchema
			{
				Index = schema.Index,
				Name = schema.Name,
			};

			//In the dwg the attributes are stored as instances referenced by index from the schema and its properties
			bool isDataSchema = schema.Properties.Any(p => p.PropertyFlags.HasFlag(AcdsSchemaPropertyFlags.Unknown2));
			if (isDataSchema)
			{
				foreach (string attribute in _schemaAttributes)
				{
					if (this.tryAddAttribute(source, attribute, out ulong index))
					{
						dwgSchema.Indices.Add(index);
					}
				}
			}

			foreach (AcdsSchemaProperty property in schema.Properties)
			{
				if (isDataSchema && _attributePropertyNames.Contains(property.Name))
				{
					continue;
				}

				AcdsSchemaRecord record = this.createSchemaRecord(property);

				if (property.PropertyFlags.HasFlag(AcdsSchemaPropertyFlags.Unknown2) && record.TypeSize > 0)
				{
					foreach (string attribute in _indexedPropertyAttributes)
					{
						if (this.tryAddAttribute(source, attribute, out ulong index))
						{
							record.Values.Add(getValueBytes(index, record.TypeSize));
						}
					}
				}

				dwgSchema.EmbeddedRecords.Add(record);
			}

			this._schemas.Add(dwgSchema);
			this._schemaIndex.SchemaPointers.Add(new SchemaIndex.Pointer
			{
				Index = dwgSchema.Index,
				SegmentIndex = SchemaDataSegmentIndex,
			});
		}
	}

	private AcdsSchemaRecord createSchemaRecord(AcdsSchemaProperty property)
	{
		AcdsSchemaRecord record = new AcdsSchemaRecord
		{
			Name = property.Name,
			NameIndex = this.getPropertyNameIndex(property.Name),
		};

		if (property.Name == CadFileDataStorage.HandleAttribute)
		{
			//AutoCAD stores the handle attribute as a byte property with a single default value
			record.Flags = SchemaRecordFlags.Unknown1;
			record.Type = 7;
			record.TypeSize = AcdsSchemaRecord.TypeSizes[record.Type];
			record.Unknown2 = 1;
			record.Values.Add(new byte[] { 0 });
			return record;
		}

		if (!property.Type.HasValue || property.PropertyFlags.HasFlag(AcdsSchemaPropertyFlags.NoType))
		{
			record.Flags = SchemaRecordFlags.NoType;
			return record;
		}

		record.Flags = SchemaRecordFlags.None;
		record.Type = property.Type.Value;
		record.TypeSize = record.Type == 0xE ? property.TypeSize : AcdsSchemaRecord.TypeSizes[record.Type];

		return record;
	}

	private void endSegment(FileSegment segment, long start)
	{
		uint length = (uint)(this._stream.Position - start);
		uint paddingSize = getPaddingSize(length, SegmentAlignment);
		this.writePadding(SegmentPadding, paddingSize);

		uint size = length + paddingSize;

		//AutoCAD sets only the header size in the blob segments, the full size is in the segment index
		segment.Header.SegmentSize = segment.Header.IsBlob != 0 ? FileSegmentHeader.SIZE : size;

		long end = this._stream.Position;
		this._stream.Position = start;
		this.writeSubItemHeader(segment.Header);
		this._stream.Position = end;

		FileSegmentIndex.Entry entry = this._segmentIndex.Entries[(int)segment.Header.SegmentIndex];
		entry.Offset = (ulong)start;
		entry.Size = size;
	}

	private uint getPropertyNameIndex(string name)
	{
		int index = this._propertyNames.IndexOf(name);
		if (index < 0)
		{
			index = this._propertyNames.Count;
			this._propertyNames.Add(name);
		}

		return (uint)index;
	}

	private bool tryAddAttribute(List<AcdsSchema> schemas, string attributeSchemaName, out ulong index)
	{
		index = 0;

		AcdsSchema attributeSchema = schemas.FirstOrDefault(s => s.Name == attributeSchemaName);
		if (attributeSchema == null)
		{
			return false;
		}

		index = (ulong)this._schemaIndex.PropertyPointers.Count;

		//Each attribute instance takes 8 bytes at the beginning of the schema data segment
		this._schemaIndex.PropertyPointers.Add(new SchemaIndex.Pointer
		{
			SegmentIndex = SchemaDataSegmentIndex,
			LocalOffset = (uint)index * 8,
			Index = attributeSchema.Index,
		});
		this._attributeValues.Add(attributeSchemaName == CadFileDataStorage.HandleAttributeSchemaName ? 0u : 1u);

		return true;
	}

	private void writeAcdsSchema(AcdsSchema schema)
	{
		this._writer.WriteRawShort((ushort)schema.Indices.Count);
		foreach (ulong index in schema.Indices)
		{
			this.writeULong(index);
		}

		this._writer.WriteRawShort((ushort)schema.EmbeddedRecords.Count);
		foreach (AcdsSchemaRecord record in schema.EmbeddedRecords)
		{
			this.writeAcdsSchemaRecord(record);
		}
	}

	private void writeAcdsSchemaRecord(AcdsSchemaRecord record)
	{
		this._writer.WriteInt((int)record.Flags);
		this._writer.WriteUInt(record.NameIndex);

		if ((record.Flags & SchemaRecordFlags.NoType) == 0)
		{
			this._writer.WriteUInt(record.Type);
			if (record.Type == 0xE)
			{
				this._writer.WriteUInt(record.TypeSize);
			}
		}

		if (record.Flags == SchemaRecordFlags.Unknown0)
		{
			this._writer.WriteUInt(record.Unknown1);
		}
		else if (record.Flags == SchemaRecordFlags.Unknown1)
		{
			this._writer.WriteUInt(record.Unknown2);
		}

		if (record.TypeSize == 0)
		{
			this._writer.WriteRawShort((ushort)0);
			return;
		}

		this._writer.WriteRawShort((ushort)record.Values.Count);
		foreach (byte[] value in record.Values)
		{
			this._writer.WriteBytes(value, 0, (int)record.TypeSize);
		}
	}

	private void writeBlob(Blob01 blob)
	{
		long start = this.beginSegment(blob, blob.Header.SegmentIndex);

		this.writeULong(blob.TotalDataSize);
		this.writeULong(blob.PageStartOffset);
		this._writer.WriteUInt(blob.PageIndex);
		this._writer.WriteUInt(blob.PageCount);
		this.writeULong(blob.PageDataSize);
		this._writer.WriteBytes(blob.Data);

		this.endSegment(blob, start);
	}

	private void writeDataField()
	{
		long start = this.beginSegment(this._dataField, DataSegmentIndex);

		uint localOffset = 0;
		foreach (DataField.DataEntry entry in this._dataField.DataEntries)
		{
			entry.Header.LocalOffset = localOffset;
			localOffset += entry.BlobReference == null ? 4 + entry.DataSize : 4 + entry.BlobReference.RecordSize;

			this._writer.WriteUInt(entry.Header.Size);
			//Unknown (always 1)
			this._writer.WriteUInt(1);
			this.writeULong(entry.Header.Handle);
			this._writer.WriteUInt(entry.Header.LocalOffset);
		}

		this.alignPosition(start, 16, DataAlignmentPadding);
		this._dataField.Header.ObjectDataAlignmentOffset = (int)((this._stream.Position - start) >> 4);

		foreach (DataField.DataEntry entry in this._dataField.DataEntries)
		{
			if (entry.BlobReference == null)
			{
				this._writer.WriteUInt(entry.DataSize);
				this._writer.WriteBytes(entry.Data);
				continue;
			}

			DataField.DataBlobReference reference = entry.BlobReference;
			this._writer.WriteUInt(BlobReferenceSignature);
			this.writeULong(reference.TotalDataSize);
			this._writer.WriteUInt(reference.PageCount);
			this._writer.WriteUInt(reference.RecordSize);
			this._writer.WriteUInt(reference.PageSize);
			this._writer.WriteUInt(reference.LastPageSize);
			this._writer.WriteUInt(reference.Unknown1);
			this._writer.WriteUInt(reference.Unknown2);

			foreach (Blob01 blob in reference.Blobs)
			{
				this._writer.WriteUInt(blob.Header.SegmentIndex);
				this._writer.WriteUInt((uint)blob.PageDataSize);
			}
		}

		this.endSegment(this._dataField, start);

		foreach (DataField.DataEntry entry in this._dataField.DataEntries.Where(e => e.BlobReference != null))
		{
			foreach (Blob01 blob in entry.BlobReference.Blobs)
			{
				this.writeBlob(blob);
			}
		}
	}

	private void writeDataIndex()
	{
		long start = this.beginSegment(this._dataIndex, DataIndexSegmentIndex);

		//UInt32 Entry count
		this._writer.WriteUInt((uint)this._dataIndex.Entries.Count);
		//UInt32 Unknown (0)
		this._writer.WriteUInt(0);
		foreach (DataIndexSegment.Entry entry in this._dataIndex.Entries)
		{
			this._writer.WriteUInt(entry.SegmentIndex);
			this._writer.WriteUInt(entry.LocalOffset);
			this._writer.WriteUInt(entry.SchemaIndex);
		}

		this.endSegment(this._dataIndex, start);
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
		long start = this.beginSegment(this._segmentIndex, SegmentIndexSegmentIndex);

		foreach (FileSegmentIndex.Entry entry in this._segmentIndex.Entries)
		{
			this.writeULong(entry.Offset);
			this._writer.WriteUInt(entry.Size);
		}

		this.endSegment(this._segmentIndex, start);
	}

	private void writeNullTerminatedString(string value)
	{
		this._writer.WriteBytes(this._writer.Encoding.GetBytes(value));
		this._writer.WriteByte(0);
	}

	private void writePadding(byte value, long count)
	{
		for (long i = 0; i < count; i++)
		{
			this._writer.WriteByte(value);
		}
	}

	private void writeSchemaData()
	{
		SchemaData schemaData = new SchemaData();
		long start = this.beginSegment(schemaData, SchemaDataSegmentIndex);
		long dataStart = start + FileSegmentHeader.SIZE;

		//Attribute instances referenced by the schema index property entries
		foreach (uint value in this._attributeValues)
		{
			this._writer.WriteUInt(8);
			this._writer.WriteUInt(value);
		}

		for (int i = 0; i < this._schemas.Count; i++)
		{
			this._schemaIndex.SchemaPointers[i].LocalOffset = (uint)(this._stream.Position - dataStart);
			this.writeAcdsSchema(this._schemas[i]);
		}

		this.alignPosition(start, 16, SchemaAlignmentPadding);
		schemaData.Header.SystemDataAlignmentOffset = (int)((this._stream.Position - start) >> 4);

		//Uint32 Property name count
		this._writer.WriteUInt((uint)this._propertyNames.Count);
		foreach (string name in this._propertyNames)
		{
			this.writeNullTerminatedString(name);
		}

		this.endSegment(schemaData, start);
	}

	private void writeSchemaIndex()
	{
		long start = this.beginSegment(this._schemaIndex, SchemaIndexSegmentIndex);

		//UInt32 Schema count
		this._writer.WriteUInt((uint)this._schemaIndex.SchemaPointers.Count);
		//UInt32 Unknown (0)
		this._writer.WriteUInt(0);
		foreach (SchemaIndex.Pointer pointer in this._schemaIndex.SchemaPointers)
		{
			this._writer.WriteUInt(pointer.Index);
			this._writer.WriteUInt(pointer.SegmentIndex);
			this._writer.WriteUInt(pointer.LocalOffset);
		}

		//Int64 Unknown (0x0af10c)
		this.writeULong(SchemaIndexTag);

		//UInt32 Property entry count
		this._writer.WriteUInt((uint)this._schemaIndex.PropertyPointers.Count);
		//UInt32 Unknown (0)
		this._writer.WriteUInt(0);
		foreach (SchemaIndex.Pointer pointer in this._schemaIndex.PropertyPointers)
		{
			this._writer.WriteUInt(pointer.SegmentIndex);
			this._writer.WriteUInt(pointer.LocalOffset);
			this._writer.WriteUInt(pointer.Index);
		}

		this.alignPosition(start, 16, SegmentPadding);
		this._schemaIndex.Header.SystemDataAlignmentOffset = (int)((this._stream.Position - start) >> 4);

		//Uint32 Schema name count
		this._writer.WriteUInt((uint)this._schemaIndex.SchemaNames.Count);
		foreach (string name in this._schemaIndex.SchemaNames)
		{
			this.writeNullTerminatedString(name);
		}

		this.endSegment(this._schemaIndex, start);
	}

	private void writeSearch()
	{
		FileSegment search = new FileSegment("search");
		long start = this.beginSegment(search, SearchSegmentIndex);

		var groups = this._dataIndex.Entries
			.Select((e, i) => (e.SchemaIndex, RecordIndex: (ulong)i, this._dataField.DataEntries[i].Header.Handle))
			.GroupBy(r => r.SchemaIndex)
			.OrderBy(g => g.Key)
			.ToList();

		//UInt32 Search schema count
		this._writer.WriteUInt((uint)groups.Count);
		foreach (var group in groups)
		{
			var sorted = group.OrderBy(r => r.Handle).ToList();

			//UInt32 Schema index
			this._writer.WriteUInt(group.Key);

			//UInt64 Sorted index count, followed by the record indexes sorted by handle
			this.writeULong((ulong)sorted.Count);
			foreach (var item in sorted)
			{
				this.writeULong(item.RecordIndex);
			}

			//UInt32 Indexed property count (only AcDbDs::ID)
			this._writer.WriteUInt(1);
			//UInt32 Unknown (0)
			this._writer.WriteUInt(0);

			//UInt32 Id entry count
			this._writer.WriteUInt((uint)sorted.Count);
			foreach (var item in sorted)
			{
				this.writeULong(item.Handle);
				//UInt64 Record index count
				this.writeULong(1);
				this.writeULong(item.RecordIndex);
			}
		}

		this.endSegment(search, start);
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

	private void writeULong(ulong value)
	{
		this._writer.WriteBytes(LittleEndianConverter.Instance.GetBytes(value));
	}
}