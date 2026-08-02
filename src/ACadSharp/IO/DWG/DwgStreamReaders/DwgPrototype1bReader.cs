using ACadSharp.Prototype1b;
using ACadSharp.Prototype1b.Segments;
using CSUtilities.Converters;
using CSUtilities.IO;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamReaders;

internal class DwgPrototype1bReader : DwgSectionIO
{
	public override string SectionName => DwgSectionDefinition.AcDsPrototype;

	public static readonly uint[] TYPE_SIZES = new uint[] { 0, 0, 2, 1, 2, 4, 8, 1, 2, 4, 8, 4, 8, 0, 0, 0 };

	private readonly DwgDocumentBuilder _builder;

	private readonly Encoding _encoding = TextEncoding.Windows1252();

	private readonly StreamIO _reader;

	private readonly DataStorage _storage = new DataStorage();

	private ulong _currentOffset;

	public DwgPrototype1bReader(ACadVersion version, DwgDocumentBuilder builder, IDwgStreamReader reader) : base(version)
	{
		this._reader = new StreamIO(reader.Stream);
		this._reader.EndianConverter = new LittleEndianConverter();

		this._builder = builder;
	}

	public DataStorage Read()
	{
		try
		{
			this._storage.FileHeader = this.readFileHeader();

			// Indices for reading values
			this._storage.SegmentIndex = this.readSegmentIndex();
			this._storage.DataIndex = this.readDataIndex();
			this._storage.SchemaIndex = this.readSchemaIndex();

			// Index to empty spaces (padding or so) within the file
			this._storage.FreeSpace = this.readFreeSpace();

			// Probably the file state before the last save
			this._storage.PreviousSave = this.readPreviousSave();

			// Schema data lookup
			this._storage.SchemaSearch = this.readSchemaSearch();
			foreach (SchemaSearchEntry search in this._storage.SchemaSearch.Entries)
			{
				search.SchemaName = this._storage.SchemaIndex.SchemaNames[search.SchemaNameIndex];
			}

			this._storage.SchemaFields = [];
			this._storage.DataFields = [];
			this._storage.Blobs = [];

			// From this point on it should be possible to read storage entries sequentially
			// The issue is that sometimes there seemingly are empty padding sections (which should be
			// referenced by the FreeSpace entry) that are missing from the FreeSpace definition or are larger
			// than specified in the FreeSpace definition.

			this.readSegments();
		}
		catch (Exception ex)
		{
			if (!this._builder.Configuration.Failsafe)
				throw;

			this.notify("An error occurred while reading the Prototype1b", NotificationType.Error, ex);
		}

		return this._storage;
	}

	private Blob01 readBlob01(FileSegmentHeader header)
	{
		Blob01 blob = new()
		{
			Header = header,
			TotalDataSize = this._reader.ReadULong(),
			PageStartOffset = this._reader.ReadULong(),
			PageIndex = this._reader.ReadUInt(),
			PageCount = this._reader.ReadUInt(),
			PageDataSize = this._reader.ReadULong()
		};

		blob.Data = this._reader.ReadBytes((int)blob.PageDataSize);

		// Align to the next 128 byte boundary
		long boundary = this._reader.Position % 128;
		if (boundary != 0)
		{
			byte[] _ = this._reader.ReadBytes((int)(128 - boundary));
		}

		return blob;
	}

	private DataIndex readDataIndex()
	{
		var pointer = this._storage.SegmentIndex.Pointers[this._storage.FileHeader.DataIndexSegmentIndex];
		return (DataIndex)this.readSegmentAt(pointer.Offset);
	}

	private DataIndex readDataIndexValue(FileSegmentHeader header)
	{
		var pos = this._reader.Position;

		DataIndex dataIndex = new()
		{
			Header = header
		};

		//UInt32 Unknown property count
		int entryCount = this._reader.ReadInt();
		//UInt32 Unknown (0)
		dataIndex.Unknown1 = this._reader.ReadInt();
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

			dataIndex.Entries.Add(new DataIndex.Entry
			{
				SegmentIndex = segmentIndex,
				LocalOffset = localOffset,
				SchemaIndex = schemaIndex,
			});
		}

		return dataIndex;
	}

	private DataField readDataValue(FileSegmentHeader header)
	{
		DataField dataField = new()
		{
			Header = header,
			Entries = []
		};

		// Read headers
		List<DataHeader> headers = [];
		uint headerEndPosition = (uint)this._reader.Position;
		if (!this._storage.DataIndex.Entries_Old.TryGetValue(header.SegmentIndex, out DataIndexEntry entry))
		{
			return dataField;
		}

		// As entries can be out of order, keep updating the max position to resume correctly after reading all entries
		long maxPos = 0;
		foreach (DataIndexEntryPointer pointer in entry.Pointers)
		{
			this._reader.Position = headerEndPosition + pointer.Offset;
			headers.Add(new DataHeader
			{
				EntrySize = this._reader.ReadUInt(),
				Unknown1 = this._reader.ReadUInt(),
				Handle = this._reader.ReadULong(),
				DataOffset = this._reader.ReadUInt(),
				SchemaIndex = pointer.SchemaIndex
			});
			maxPos = Math.Max(maxPos, this._reader.Position);
		}
		this._reader.Position = maxPos;

		// Sort headers to get ascending offset values to prevent wrong offset calculations (TODO: Is this ok or should the order maybe be preserved?)
		headers = headers.OrderBy(x => x.DataOffset).ToList();

		// Get the position to the beginning of the data content section and skip padding and unreferenced DataHeader entries
		uint headerStartPosition = headerEndPosition - FileSegmentHeader.SIZE;
		uint dataRefPosition = headerStartPosition + (uint)(header.ObjectDataAlignmentOffset << 4);
		if (dataRefPosition != this._reader.Position)
		{
			byte[] data = this._reader.ReadBytes((int)(dataRefPosition - this._reader.Position));
			// Many unreferenced / seemingly dangling DataHeader entries might be defined here, they would
			// also have valid file data in the next step, where file contents are being read. This can be a lot of entries, e.g.
			// 10 - 20 valid DataHeader entries have been seen
		}

		// Read data associated with headers
		for (int i = 0; i < headers.Count; i++)
		{
			DataValue value = new();
			DataHeader dataHeader = headers[i];
			uint recordStreamOffset = dataRefPosition + dataHeader.DataOffset;
			uint maxRecordSize = i + 1 < headers.Count ? (headers[i + 1].DataOffset - dataHeader.DataOffset) : (header.SegmentSize - (uint)(header.ObjectDataAlignmentOffset << 4) - dataHeader.DataOffset);

			this._reader.Position = recordStreamOffset;
			value.DataSize = this._reader.ReadUInt();
			if ((value.DataSize + 4) <= maxRecordSize)
			{
				value.Data = this._reader.ReadBytes((int)value.DataSize);

				//// Example how to detect the actual file type based on the file byte signature
				//byte[] FILE_MAGIC_PNG = [137, 80, 78, 71, 13, 10, 26, 10];
				//byte[] FILE_MAGIC_ACIS_BINARY = Encoding.ASCII.GetBytes("ACIS BinaryFile");
				//byte[] FILE_MAGIC_ASM_BINARY = Encoding.ASCII.GetBytes("ASM BinaryFile");

				//if (value.Data.Length >= FILE_MAGIC_PNG.Length && value.Data.AsSpan(0, FILE_MAGIC_PNG.Length).SequenceEqual(FILE_MAGIC_PNG)) {
				//    // The bytes represent a PNG file
				//}
				//else if (value.Data.Length >= FILE_MAGIC_ACIS_BINARY.Length && value.Data.AsSpan(0, FILE_MAGIC_ACIS_BINARY.Length).SequenceEqual(FILE_MAGIC_ACIS_BINARY)) {
				//    // The bytes represent an ACIS binary file
				//}
				//else if (value.Data.Length >= FILE_MAGIC_ASM_BINARY.Length && value.Data.AsSpan(0, FILE_MAGIC_ASM_BINARY.Length).SequenceEqual(FILE_MAGIC_ASM_BINARY)) {
				//    // The bytes represent an ACIS / ASM binary file
				//}
				//else {
				//    // The bytes represent a file other than a PNG and ACIS file
				//}
			}
			else if (value.DataSize == 0xbb106bb1)
			{
				value.BlobReference = new DataBlobReference()
				{
					TotalDataSize = this._reader.ReadULong(),
					PageCount = this._reader.ReadUInt(),
					RecordSize = this._reader.ReadUInt(),
					PageSize = this._reader.ReadUInt(),
					LastPageSize = this._reader.ReadUInt(),
					Unknown1 = this._reader.ReadUInt(),
					Unknown2 = this._reader.ReadUInt(),
					SegmentPointers = []
				};
				for (int j = 0; j < value.BlobReference.PageCount; j++)
				{
					value.BlobReference.SegmentPointers.Add((
						this._reader.ReadUInt(),    // segment index
						this._reader.ReadUInt()     // size
					));
				}
			}
			else
			{
				Debugger.Break();   // This should not be possible
				value.DataSize = 0;
			}

			dataField.Entries.Add(new DataEntry
			{
				Header = dataHeader,
				Value = value
			});
		}

		return dataField;
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

	private FreeSpace readFreeSpace()
	{
		if (this._storage.FileHeader.FreeSpaceSegmentIndex == 0) return null;
		return (FreeSpace)this.readSegmentAt(this._storage.SegmentIndex.Pointers[this._storage.FileHeader.FreeSpaceSegmentIndex].Offset);
	}

	private FreeSpace readFreeSpaceValue(FileSegmentHeader header)
	{
		FreeSpace space = new()
		{
			Header = header,
			Unknown = this._reader.ReadULong(),
			FreeSpaces = new FreeSpaceArea[this._storage.FileHeader.FreeSpaceEntryCount]
		};

		for (int i = 0; i < space.FreeSpaces.Length; i++)
		{
			// TODO: Sometimes when there is an additional freespace definition, it might not match the `FreeSpaceEntryCount` count
			ulong position = this._reader.ReadULong();
			uint size = this._reader.ReadUInt();
			space.FreeSpaces[i] = new FreeSpaceArea
			{
				Position = position,
				Size = size
			};
		}
		return space;
	}

	private string readNullTerminatedString()
	{
		byte b;
		List<byte> bytes = [];
		while ((b = this._reader.ReadByte()) != 0)
		{
			bytes.Add(b);
		}
		return this._encoding.GetString(bytes.ToArray());
	}

	private PreviousSave readPreviousSave()
	{
		if (this._storage.FileHeader.PreviousSaveIndex == 0) return null;
		return (PreviousSave)this.readSegmentAt(this._storage.SegmentIndex.Pointers[this._storage.FileHeader.PreviousSaveIndex].Offset);
	}

	private PreviousSave readPreviousSaveValue(FileSegmentHeader header) => new()
	{
		Header = header,
		FileHeader = this.readFileHeader()
	};

	private Schema readSchema()
	{
		Schema schema = new();

		ushort indexCount = BitConverter.ToUInt16(this._reader.ReadBytes(2), 0);
		schema.Indices = new ulong[indexCount];
		for (int i = 0; i < indexCount; i++)
		{
			schema.Indices[i] = this._reader.ReadULong();
		}

		ushort propCount = BitConverter.ToUInt16(this._reader.ReadBytes(2), 0);
		schema.Properties = new SchemaProperty[propCount];
		for (int i = 0; i < propCount; i++)
		{
			schema.Properties[i] = this.readSchemaProperty();
		}

		return schema;
	}

	private SchemaData readSchemaDataValue(FileSegmentHeader header)
	{
		// Read unknown schema properties
		List<SchemaUnknownProperty> unknownProps = [];
		foreach (SchemaPropertyPointer pointer in this._storage.SchemaIndex.SchemaUnknownPropertyPointer)
		{
			if (pointer.SchemaIndex != header.SegmentIndex) continue;
			unknownProps.Add(new SchemaUnknownProperty
			{
				DataSize = this._reader.ReadUInt(),
				UnknownFlags = this._reader.ReadUInt()
			});
		}

		// Read schemas
		List<Schema> schemaValues = [];
		foreach (SchemaPropertyPointer pointer in this._storage.SchemaIndex.UnknownPropertyPointers)
		{
			if (pointer.SchemaIndex != header.SegmentIndex) continue;
			Schema schema = this.readSchema();
			schema.Index = pointer.Index;
			schema.Name = this._storage.SchemaIndex.SchemaNames[(int)pointer.Index];
			schemaValues.Add(schema);
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
		foreach (Schema schema in schemaValues)
		{
			foreach (SchemaProperty property in schema.Properties)
			{
				property.Name = propertyNames[(int)property.NameIndex];
			}
		}

		return new SchemaData
		{
			Header = header,
			SchemaUnknownProperties = unknownProps,
			Values = schemaValues,
		};
	}

	private SchemaIndexFileSegment readSchemaIndex()
	{
		return (SchemaIndexFileSegment)this.readSegmentAt(this._storage.SegmentIndex.Pointers[this._storage.FileHeader.SchemaIndexSegmentIndex].Offset);
	}

	private SchemaIndexFileSegment readSchemaIndexValue(FileSegmentHeader header)
	{
		SchemaIndexFileSegment schema = new()
		{
			Header = header
		};

		//UInt32 Unknown property count
		uint unknownPropertyCount = this._reader.ReadUInt();
		//UInt32 Unknown (0)
		var unknown0 = this._reader.ReadUInt();
		for (int i = 0; i < unknownPropertyCount; i++)
		{
			//UInt32 Index (starting at 0)
			var index = this._reader.ReadUInt();
			//UInt32 Segment index into the segment index file segment entry table (paragraph
			//24.2.2.1) of the schema data file segment (paragraph 24.2.2.6)
			var schemaIndex = this._reader.ReadUInt();
			//UInt32 Local offset of the unknown schema property. This is a local offset in the
			//stream, relative to the schema data file segment’s stream start position.
			var offset = this._reader.ReadUInt();
			schema.UnknownPropertyPointers.Add(new SchemaPropertyPointer
			{
				Index = index,
				SchemaIndex = schemaIndex,
				Offset = offset,
			});
		}

		//Int64 Unknown (0x0af10c)
		var unknown1 = this._reader.ReadLong();
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
			schema.PropertyPointers.Add(new SchemaPropertyPointer
			{
				SchemaIndex = schemaIndex,
				Offset = offset,
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
		var propertyNamesOffset = (ulong)((long)header.SystemDataAlignmentOffset << 4);
		if (propertyNamesOffset != 0)
		{
			this._reader.Position = (long)(this._currentOffset + propertyNamesOffset);
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

	private SchemaProperty readSchemaProperty()
	{
		SchemaProperty prop = new()
		{
			PropertyFlags = this._reader.ReadUInt(),   // 1 = Unknown / 2 = NoType / 8 = Unknown
			NameIndex = this._reader.ReadUInt()
		};

		// Get type size
		if (!((prop.PropertyFlags & (1 << 1)) != 0))
		{
			prop.Type = this._reader.ReadUInt();        // 0 - 15
			if (prop.Type == 0xe)
			{
				prop.TypeSize = this._reader.ReadUInt();
			}
			else
			{
				prop.TypeSize = TYPE_SIZES[prop.Type.Value];
			}
		}

		// Read unknown fields
		if (prop.PropertyFlags == 1)
		{
			prop.Unknown1 = this._reader.ReadUInt();
		}
		else if (prop.PropertyFlags == 8)
		{
			prop.Unknown2 = this._reader.ReadUInt();
		}

		// Read values
		prop.PropertyValueCount = BitConverter.ToUInt16(this._reader.ReadBytes(2), 0);
		prop.Values = new byte[prop.PropertyValueCount, prop.TypeSize];
		if (prop.TypeSize != 0)
		{
			for (int i = 0; i < prop.PropertyValueCount; i++)
			{
				byte[] propertyValue = this._reader.ReadBytes((int)prop.TypeSize);
				for (int j = 0; j < propertyValue.Length; j++)
				{
					prop.Values[0, j] = propertyValue[j];
				}
			}
		}

		return prop;
	}

	private SchemaSearch readSchemaSearch()
	{
		return (SchemaSearch)this.readSegmentAt(this._storage.SegmentIndex.Pointers[this._storage.FileHeader.SearchSegmentIndex].Offset);
	}

	private SchemaSearchEntry readSchemaSearchEntry()
	{
		SchemaSearchEntry search = new()
		{
			SchemaNameIndex = (int)this._reader.ReadUInt()
		};

		ulong sortedIndexCount = this._reader.ReadULong();
		search.SortedIndices = new ulong[sortedIndexCount];
		for (ulong i = 0; i < sortedIndexCount; i++)
		{
			search.SortedIndices[i] = this._reader.ReadULong();
		}

		uint idIndexesCount = this._reader.ReadUInt();
		SearchEntryObject[][] idEntryObjects = new SearchEntryObject[idIndexesCount][];

		// TODO: Find out what it would mean if there were multiple index lists. Never happened in test files so far
		if (idIndexesCount > 1)
		{
			Debugger.Break();
		}

		if (idIndexesCount > 0)
		{
			search.Unknown1 = this._reader.ReadUInt();

			for (uint i = 0; i < idIndexesCount; i++)
			{
				uint idIndexCount = this._reader.ReadUInt();

				SearchEntryObject[] entryObjects = new SearchEntryObject[idIndexCount];
				for (uint j = 0; j < idIndexCount; j++)
				{
					ulong handle = this._reader.ReadULong();

					ulong indexCount = this._reader.ReadULong();
					ulong[] indices = new ulong[indexCount];
					for (ulong k = 0; k < indexCount; k++)
					{
						indices[k] = this._reader.ReadULong();
					}

					entryObjects[j] = new SearchEntryObject
					{
						Handle = handle,
						Indices = indices
					};
				}
				idEntryObjects[i] = entryObjects;
			}
		}
		search.IdEntryObjects = idEntryObjects;

		return search;
	}

	private SchemaSearch readSchemaSearchValue(FileSegmentHeader header)
	{
		SchemaSearch schemaSearch = new()
		{
			Header = header
		};
		
		int schemaCount = this._reader.ReadInt();
		schemaSearch.Entries = [];
		for (int j = 0; j < schemaCount; j++)
		{
			schemaSearch.Entries.Add(this.readSchemaSearchEntry());
		}

		return schemaSearch;
	}

	private IPrototype1bSegment readSegment()
	{
		IPrototype1bSegment segment;
		long segmentStartPosition = this._reader.Position;

		FileSegmentHeader header = this.readSubItemHeader();

		segment = this.readSegmentData(header);

		// Blob01 entries do not specify how large they are (The headers SegmentSize only has the size of the header without any blob data, blobs are aligned to the next 128 byte boundary)
		if (header.IsBlob != 1)
		{
			// Skip to the end of this sub item
			long readSize = this._reader.Position - segmentStartPosition;
			long paddingDataSize = header.SegmentSize - readSize;
			byte[] _padding = this._reader.ReadBytes((int)paddingDataSize);     // Should be filled with only 0x70 values (TODO: Sometimes there are some other strange padding bytes)
		}

		return segment;
	}

	private IPrototype1bSegment readSegmentAt(ulong offset)
	{
		long pos = this._reader.Position;
		this._reader.Position = (long)offset;
		_currentOffset = offset;

		IPrototype1bSegment segment = this.readSegment();

		this._reader.Position = pos;
		return segment;
	}

	private IPrototype1bSegment readSegmentData(FileSegmentHeader header)
	{
		return header.Name switch
		{
			FileSegment.SegmentIndexName => this.readSegmentIndexValue(header),
			FileSegment.DataIndexName => this.readDataIndexValue(header),
			FileSegment.SchemaIndexName => this.readSchemaIndexValue(header),
			FileSegment.SchemaSearchName => this.readSchemaSearchValue(header),
			FileSegment.FreeSpaceName => this.readFreeSpaceValue(header),
			FileSegment.PreviousSaveName => this.readPreviousSaveValue(header),
			FileSegment.SchemaDataName => this.readSchemaDataValue(header),
			FileSegment.DataName => this.readDataValue(header),
			FileSegment.Blob01Name => this.readBlob01(header),
			_ => throw new InvalidDataException($"Unknown segment type \"{header.Name}\""),
		};
	}

	private SegmentIndex readSegmentIndex()
	{
		return (SegmentIndex)this.readSegmentAt((ulong)this._storage.FileHeader.SegmentIndexOffset);
	}

	private SegmentIndex readSegmentIndexValue(FileSegmentHeader header)
	{
		SegmentIndex index = new()
		{
			Header = header
		};

		for (int j = 0; j < this._storage.FileHeader.SegmentIndexEntryCount; j++)
		{
			var offset = this._reader.ReadULong();
			var size = this._reader.ReadUInt();

			index.Pointers[j] = new SegmentIndexEntry
			{
				Offset = offset,
				Size = size
			};
		}

		return index;
	}

	private void readSegments()
	{
		foreach (KeyValuePair<int, SegmentIndexEntry> entry in this._storage.SegmentIndex.Pointers)
		{
			if (entry.Value.Size == 0) continue;
			this._reader.Position = (long)entry.Value.Offset;

			IPrototype1bSegment segment = this.readSegment();
			switch (segment)
			{
				// Those entries have already been parsed (as they were directly referenced in the header
				// and there should probably not be any additional entries of the same type)
				case SegmentIndex segidx:
				case DataIndex datidx:
				case SchemaIndexFileSegment schidx:
				case SchemaSearch search:
				case FreeSpace freesp:
				case PreviousSave prvsav:
					break;
				// Add entries which are types that can appear multiple times to a collection of them
				case SchemaData schdat:
					this._storage.SchemaFields.Add(schdat);
					break;
				case DataField data:
					this._storage.DataFields.Add(data);
					break;
				case Blob01 blob:
					this._storage.Blobs.Add(blob);
					break;
				default:
					break;
			}
		}
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