using ACadSharp.Prototype1b;
using ACadSharp.Prototype1b.Segments;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamReaders
{
    internal class DwgPrototype1bReader : DwgSectionIO
	{
		public static readonly uint[] TYPE_SIZES = new uint[] { 0, 0, 2, 1, 2, 4, 8, 1, 2, 4, 8, 4, 8, 0, 0, 0 };
		private readonly IDwgStreamReader _reader;
		private readonly DataStorage _storage = new DataStorage();

		public override string SectionName => DwgSectionDefinition.AcDsPrototype;

        public DwgPrototype1bReader(ACadVersion version, IDwgStreamReader reader) : base(version)
        {
            this._reader = reader;
        }

        public DataStorage Read()
        {
			try
			{
				this._storage.FileHeader = this.readFileHeader();

				// Indices for reading values
				this._storage.IndexPointers = new DataStoragePointers();
				this._storage.IndexPointers.SegmentIndex = this.readSegmentIndex();
				this._storage.IndexPointers.DataIndex = this.readDataIndex();
				this._storage.IndexPointers.SchemaIndex = this.readSchemaIndex();
				this._storage.IndexPointers.FreeSpace = this.readFreeSpace();		// Index to empty spaces (padding or so) within the file

				// Probably the file state before the last save
				this._storage.PreviousSave = this.readPreviousSave();

				// Schema data lookup
				this._storage.SchemaSearch = this.readSchemaSearch();
				foreach (SchemaSearchEntry search in this._storage.SchemaSearch.Entries) {
					search.SchemaName = this._storage.IndexPointers.SchemaIndex.SchemaNames[search.SchemaNameIndex];
				}

				this._storage.SchemaFields = [];
				this._storage.DataFields = [];
				this._storage.Blobs = [];

				// From this point on it should be possible to read storage entries sequentially
				// The issue is that sometimes there seemingly are empty padding sections (which should be
				// referenced by the FreeSpace entry) that are missing from the FreeSpace definition or are larger
				// than specified in the FreeSpace definition.

				this.ReadSegments();
			}
			catch (Exception ex)
			{
				this.notify("An error occurred while reading the Prototype1b", NotificationType.Error, ex);
			}

			return this._storage;
		}

        internal IPrototype1bSegment ReadSegmentAt(ulong offset)
        {
            long pos = this._reader.Position;
            this._reader.Position = (long)offset;
            IPrototype1bSegment segment = this.ReadSegment();
            this._reader.Position = pos;
            return segment;
        }

        private SegmentIndex readSegmentIndex()
        {
            return (SegmentIndex)this.ReadSegmentAt((ulong)this._storage.FileHeader.SegmentIndexOffset);
        }

        private DataIndex readDataIndex()
        {
            return (DataIndex)this.ReadSegmentAt(this._storage.IndexPointers.SegmentIndex.Pointers[this._storage.FileHeader.DataIndexSegmentIndex].Offset);
        }

        private SchemaSearch readSchemaSearch()
        {
            return (SchemaSearch)this.ReadSegmentAt(this._storage.IndexPointers.SegmentIndex.Pointers[this._storage.FileHeader.SearchSegmentIndex].Offset);
        }

        private FreeSpace readFreeSpace()
        {
            if (this._storage.FileHeader.FreeSpaceSegmentIndex == 0) return null;
            return (FreeSpace)this.ReadSegmentAt(this._storage.IndexPointers.SegmentIndex.Pointers[this._storage.FileHeader.FreeSpaceSegmentIndex].Offset);
        }

        private PreviousSave readPreviousSave()
        {
            if (this._storage.FileHeader.PreviousSaveIndex == 0) return null;
            return (PreviousSave)this.ReadSegmentAt(this._storage.IndexPointers.SegmentIndex.Pointers[this._storage.FileHeader.PreviousSaveIndex].Offset);
        }

        private SchemaIndex readSchemaIndex()
        {
            return (SchemaIndex)this.ReadSegmentAt(this._storage.IndexPointers.SegmentIndex.Pointers[this._storage.FileHeader.SchemaIndexSegmentIndex].Offset);
        }

        internal void ReadSegments()
        {
            foreach (KeyValuePair<int, SegmentIndexEntry> entry in this._storage.IndexPointers.SegmentIndex.Pointers) {
                if (entry.Value.Size == 0) continue;
                this._reader.Position = (long) entry.Value.Offset;

                IPrototype1bSegment segment = this.ReadSegment();
                switch (segment) {
                    // Those entries have already been parsed (as they were directly referenced in the header
                    // and there should probably not be any additional entries of the same type)
                    case SegmentIndex segidx:
                    case DataIndex datidx:
                    case SchemaIndex schidx:
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

        internal IPrototype1bSegment ReadSegment()
        {
            IPrototype1bSegment segment;
            long segmentStartPosition = this._reader.Position;
            SegmentHeader header = this.readSubItemHeader();
            segment = this.ReadSegmentData(header);

            // Blob01 entries do not specify how large they are (The headers SegmentSize only has the size of the header without any blob data, blobs are aligned to the next 128 byte boundary)
            if (header.IsBlob != 1) {
                // Skip to the end of this sub item
                long readSize = this._reader.Position - segmentStartPosition;
                long paddingDataSize = header.SegmentSize - readSize;
                byte[] _padding = this._reader.ReadBytes((int)paddingDataSize);     // Should be filled with only 0x70 values (TODO: Sometimes there are some other strange padding bytes)
            }

            return segment;
        }

        private SegmentHeader readSubItemHeader()
        {
            return new() {
                Signature = this._reader.ReadShort(),
                Name = Encoding.ASCII.GetString(this._reader.ReadBytes(6)),
                SegmentIndex = this._reader.ReadUInt(),
                IsBlob = this._reader.ReadInt(),
                SegmentSize = this._reader.ReadUInt(),			// Multiple of 0x40 bytes (AutoCAD uses 0x80), padded wth 0x70 values
                Unknown1 = this._reader.ReadInt(),
                DataStorageRevision = this._reader.ReadInt(),
                Unknown2 = this._reader.ReadInt(),
                SystemDataAlignmentOffset = this._reader.ReadInt(),
                ObjectDataAlignmentOffset = this._reader.ReadInt(),
                AlignmentBytes = this._reader.ReadBytes(8)      // Align to next 16 byte boundary
            };
        }

        internal IPrototype1bSegment ReadSegmentData(SegmentHeader header) => header.Name switch {
            "segidx" => this.readSegmentIndexValue(header),
            "datidx" => this.readDataIndexValue(header),
            "schidx" => this.readSchemaIndexValue(header),
            "search" => this.readSchemaSearchValue(header),
            "freesp" => this.readFreeSpaceValue(header),
            "prvsav" => this.readPreviousSaveValue(header),
            "schdat" => this.readSchemaDataValue(header),
            "_data_" => this.readDataValue(header),
            "blob01" => this.readBlob01(header),
            _ => throw new InvalidDataException($"Unknown segment type \"{header.Name}\""),
        };

        private FileHeader readFileHeader()
        {
            uint fileSignature = this._reader.ReadUInt();
            short fileHeaderSize = this._reader.ReadShort();
            return new()
            {
                FileSignature = fileSignature,
                FileHeaderSize = fileHeaderSize,
                UnknownFlag = this._reader.ReadShort(),
                Unknown1 = this._reader.ReadInt(),
                Version = this._reader.ReadInt(),
                Unknown2 = this._reader.ReadInt(),
                DataStorageRevision = this._reader.ReadInt(),
                SegmentIndexOffset = this._reader.ReadInt(),
                SegmentIndexUnknown = this._reader.ReadInt(),
                SegmentIndexEntryCount = this._reader.ReadInt(),
                SchemaIndexSegmentIndex = this._reader.ReadInt(),
                DataIndexSegmentIndex = this._reader.ReadInt(),
                SearchSegmentIndex = this._reader.ReadInt(),
                PreviousSaveIndex = this._reader.ReadInt(),
                FileSize = this._reader.ReadInt(),
                Unknown3 = this._reader.ReadInt(),
                FreeSpaceSegmentIndex = this._reader.ReadInt(),
                FreeSpaceEntryCount = this._reader.ReadInt(),
                Unknown5 = this._reader.ReadInt(),

                UnknownRemaining = this._reader.ReadBytes(fileHeaderSize - 72)
            };
        }

        private Schema readSchema()
        {
            Schema schema = new();

            ushort indexCount = BitConverter.ToUInt16(this._reader.ReadBytes(2), 0);
            schema.Indices = new ulong[indexCount];
            for (int i = 0; i < indexCount; i++) {
                schema.Indices[i] = this._reader.ReadRawULong();
            }

            ushort propCount = BitConverter.ToUInt16(this._reader.ReadBytes(2), 0);
            schema.Properties = new SchemaProperty[propCount];
            for (int i = 0; i < propCount; i++) {
                schema.Properties[i] = this.readSchemaProperty();
            }

            return schema;
        }

        private SchemaProperty readSchemaProperty()
        {
            SchemaProperty prop = new() {
                PropertyFlags = this._reader.ReadUInt(),   // 1 = Unknown / 2 = NoType / 8 = Unknown
                NameIndex = this._reader.ReadUInt()
            };

            // Get type size
            if (!((prop.PropertyFlags & (1 << 1)) != 0)) {
                prop.Type = this._reader.ReadUInt();		// 0 - 15
                if (prop.Type == 0xe) {
                    prop.TypeSize = this._reader.ReadUInt();
                }
                else {
                    prop.TypeSize = TYPE_SIZES[prop.Type.Value];
                }
            }

            // Read unknown fields
            if (prop.PropertyFlags == 1) {
                prop.Unknown1 = this._reader.ReadUInt();
            }
            else if (prop.PropertyFlags == 8) {
                prop.Unknown2 = this._reader.ReadUInt();
            }

            // Read values
            prop.PropertyValueCount = BitConverter.ToUInt16(this._reader.ReadBytes(2), 0);
            prop.Values = new byte[prop.PropertyValueCount, prop.TypeSize];
            if (prop.TypeSize != 0) {
                for (int i = 0; i < prop.PropertyValueCount; i++) {
                    byte[] propertyValue = this._reader.ReadBytes((int)prop.TypeSize);
                    for (int j = 0; j < propertyValue.Length; j++) {
                        prop.Values[0,j] = propertyValue[j];
                    }
                }
            }

            return prop;
        }

        private SegmentIndex readSegmentIndexValue(SegmentHeader header)
        {
            SegmentIndex index = new() {
                Header = header,
                Pointers = []
            };
            for (int j = 0; j < this._storage.FileHeader.SegmentIndexEntryCount; j++) {
                index.Pointers[j] = new SegmentIndexEntry {
                    Offset = this._reader.ReadRawULong(),
                    Size = this._reader.ReadUInt()
                };
            }

            // TODO: Sometimes additional bytes (that are not padding bytes) follow here until the segment size
            //	     They actually seem to be garbage data and can be ignored. Often times there are seemingly
            //	     random bytes without any pattern and sometimes there are bytes representing 2-Byte strings.
            //	     Garbage string examples are e.g.:
            //			 --> "nslation\0" followed by the bytes [0, 0, 7, 0, 176, 4]
            //			 --> "k\\auto"
            //			 --> "op><pr"
            //			 --> "utocad"
            //			 -->  "atio"

            return index;
        }

        private DataIndex readDataIndexValue(SegmentHeader header)
        {
            DataIndex index = new() { 
                Header = header 
            };
            int entryCount = this._reader.ReadInt();
            index.Unknown1 = this._reader.ReadInt();

            index.Entries = [];
            for (int j = 0; j < entryCount; j++) {
                uint dataSegmentIndex = this._reader.ReadUInt();
                uint dataSegmentOffset = this._reader.ReadUInt();
                uint schemaIndex = this._reader.ReadUInt();

                // Segment index values of 0 can be ignored (stub entries)
                if (dataSegmentIndex == 0) continue;

                // Get or create a data index entry
                if (!index.Entries.TryGetValue(dataSegmentIndex, out DataIndexEntry entry)) {
                    entry = new DataIndexEntry {
                        DataSegmentIndex = dataSegmentIndex,
                        Pointers = []
                    };
                }

                // Add the current pointer to the list of pointers
                entry.Pointers.Add(new DataIndexEntryPointer {
                    Offset = dataSegmentOffset,
                    SchemaIndex = schemaIndex
                });
                index.Entries[dataSegmentIndex] = entry;
            }

            return index;
        }

        private SchemaIndex readSchemaIndexValue(SegmentHeader header)
        {
            SchemaIndex schemaIndex = new() {
                Header = header
            };
            uint unknownPropertyCount = this._reader.ReadUInt();
            schemaIndex.SchemaPointer = new SchemaPropertyPointer[unknownPropertyCount];
            schemaIndex.Unknown1 = this._reader.ReadUInt();

            for (int i = 0; i < unknownPropertyCount; i++) {
                schemaIndex.SchemaPointer[i] = new SchemaPropertyPointer {
                    Index = this._reader.ReadUInt(),
                    SchemaIndex = this._reader.ReadUInt(),
                    Offset = this._reader.ReadUInt(),
                };
            }

            schemaIndex.UnknownMagic = this._reader.ReadRawLong();
            uint propertyEntryCount = this._reader.ReadUInt();
            uint unknownCount = this._reader.ReadUInt();    // 0; 8

            schemaIndex.UnknownPropertyEntryPointer = new SchemaPropertyPointer[propertyEntryCount];
            for (int i = 0; i < propertyEntryCount; i++) {
                schemaIndex.UnknownPropertyEntryPointer[i] = new SchemaPropertyPointer {
                    SchemaIndex = this._reader.ReadUInt(),
                    Offset = this._reader.ReadUInt(),
                    Index = this._reader.ReadUInt(),
                };
            }

            schemaIndex.SchemaUnknownPropertyPointer = new SchemaPropertyPointer[unknownCount];
            for (int i = 0; i < unknownCount; i++) {
                schemaIndex.SchemaUnknownPropertyPointer[i] = new SchemaPropertyPointer {
                    Index = this._reader.ReadUInt(),
                    SchemaIndex = this._reader.ReadUInt(),
                    Offset = this._reader.ReadUInt(),
                };
            }

            schemaIndex.UnknownIndex1 = this._reader.ReadUInt();

            // Align to the next 16 byte boundary
            long boundary = this._reader.Position % 16;
            if (boundary != 0) {
                byte[] _ = this._reader.ReadBytes((int)(16 - boundary));
            }

            // Read labels (NULL terminated strings)
            uint stringCount = this._reader.ReadUInt();
            schemaIndex.SchemaNames = new string[stringCount];
            for (int i = 0; i < stringCount; i++) {
                schemaIndex.SchemaNames[i] = this.readNullTerminatedString();
            }

            return schemaIndex;
        }

        private DataField readDataValue(SegmentHeader header)
        {
            DataField dataField = new() {
                Header = header,
                Entries = []
            };

            // Read headers
            List<DataHeader> headers = [];
            uint headerEndPosition = (uint)this._reader.Position;
            if (!this._storage.IndexPointers.DataIndex.Entries.TryGetValue(header.SegmentIndex, out DataIndexEntry entry)) {
                return dataField;
            }

            // As entries can be out of order, keep updating the max position to resume correctly after reading all entries
            long maxPos = 0;
            foreach (DataIndexEntryPointer pointer in entry.Pointers) {
                this._reader.Position = headerEndPosition + pointer.Offset;
                headers.Add(new DataHeader {
                    EntrySize = this._reader.ReadUInt(),
                    Unknown1 = this._reader.ReadUInt(),
                    Handle = this._reader.ReadRawULong(),
                    DataOffset = this._reader.ReadUInt(),
                    SchemaIndex = pointer.SchemaIndex
                });
                maxPos = Math.Max(maxPos, this._reader.Position);
            }
            this._reader.Position = maxPos;

			// Sort headers to get ascending offset values to prevent wrong offset calculations (TODO: Is this ok or should the order maybe be preserved?)
			headers = headers.OrderBy(x => x.DataOffset).ToList();

			// Get the position to the beginning of the data content section and skip padding and unreferenced DataHeader entries
			uint headerStartPosition = headerEndPosition - SegmentHeader.SIZE;
			uint dataRefPosition = headerStartPosition + (uint)(header.ObjectDataAlignmentOffset << 4);
            if (dataRefPosition != this._reader.Position) {
				byte[] data = this._reader.ReadBytes((int)(dataRefPosition - this._reader.Position));
				// Many unreferenced / seemingly dangling DataHeader entries might be defined here, they would
				// also have valid file data in the next step, where file contents are being read. This can be a lot of entries, e.g.
				// 10 - 20 valid DataHeader entries have been seen
			}

			// Read data associated with headers
			for (int i = 0; i < headers.Count; i++) {
                DataValue value = new();
                DataHeader dataHeader = headers[i];
                uint recordStreamOffset = dataRefPosition + dataHeader.DataOffset;
                uint maxRecordSize = i + 1 < headers.Count ? (headers[i + 1].DataOffset - dataHeader.DataOffset) : (header.SegmentSize - (uint)(header.ObjectDataAlignmentOffset << 4) - dataHeader.DataOffset);

				this._reader.Position = recordStreamOffset;
                value.DataSize = this._reader.ReadUInt();
                if ((value.DataSize + 4) <= maxRecordSize) {
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
                else if (value.DataSize == 0xbb106bb1) {
                    value.BlobReference = new DataBlobReference() {
                        TotalDataSize = this._reader.ReadRawULong(),
                        PageCount = this._reader.ReadUInt(),
                        RecordSize = this._reader.ReadUInt(),
                        PageSize = this._reader.ReadUInt(),
                        LastPageSize = this._reader.ReadUInt(),
                        Unknown1 = this._reader.ReadUInt(),
                        Unknown2 = this._reader.ReadUInt(),
                        SegmentPointers = []
                    };
                    for (int j = 0; j < value.BlobReference.PageCount; j++) {
                        value.BlobReference.SegmentPointers.Add((
                            this._reader.ReadUInt(),	// segment index
                            this._reader.ReadUInt()		// size
                        ));
                    }
                }
                else {
                    Debugger.Break();	// This should not be possible
                    value.DataSize = 0;
                }

                dataField.Entries.Add(new DataEntry {
                    Header = dataHeader,
                    Value = value
                });
            }

            return dataField;
        }

        private string readNullTerminatedString()
        {
            byte b;
            List<byte> bytes = [];
            while ((b = this._reader.ReadByte()) != 0) {
                bytes.Add(b);
            }
            return Encoding.UTF8.GetString(bytes.ToArray());     // TODO: Is this ASCII only or UTF-8 ?
        }

        private Blob01 readBlob01(SegmentHeader header)
        {
            Blob01 blob = new() {
                Header = header,
                TotalDataSize = this._reader.ReadRawULong(),
                PageStartOffset = this._reader.ReadRawULong(),
                PageIndex = this._reader.ReadUInt(),
                PageCount = this._reader.ReadUInt(),
                PageDataSize = this._reader.ReadRawULong()
            };
            blob.Data = this._reader.ReadBytes((int) blob.PageDataSize);

            // Align to the next 128 byte boundary
            long boundary = this._reader.Position % 128;
            if (boundary != 0) {
                byte[] _ = this._reader.ReadBytes((int)(128 - boundary));
            }

            return blob;
        }

        private SchemaData readSchemaDataValue(SegmentHeader header)
        {
            // Read unknown schema properties
            List<SchemaUnknownProperty> unknownProps = [];
            foreach (SchemaPropertyPointer pointer in this._storage.IndexPointers.SchemaIndex.SchemaUnknownPropertyPointer) {
                if (pointer.SchemaIndex != header.SegmentIndex) continue;
                unknownProps.Add(new SchemaUnknownProperty {
                    DataSize = this._reader.ReadUInt(),
                    UnknownFlags = this._reader.ReadUInt()
                });
            }

            // Read schemas
            List<Schema> schemaValues = [];
            foreach (SchemaPropertyPointer pointer in this._storage.IndexPointers.SchemaIndex.SchemaPointer) {
                if (pointer.SchemaIndex != header.SegmentIndex) continue;
                Schema schema = this.readSchema();
                schema.Index = pointer.Index;
                schema.Name = this._storage.IndexPointers.SchemaIndex.SchemaNames[pointer.Index];
                schemaValues.Add(schema);
            }

            // Align to the next 16 byte boundary
            long boundary = this._reader.Position % 16;
            if (boundary != 0) {
                byte[] _ = this._reader.ReadBytes((int)(16 - boundary));
            }

            // Read schema property names
            uint propertyNameCount = this._reader.ReadUInt();
            string[] propertyNames = new string[propertyNameCount];
            for (int i = 0; i < propertyNameCount; i++) {
                propertyNames[i] = this.readNullTerminatedString();
            }

            // Assign schema Property names
            foreach (Schema schema in schemaValues) {
                foreach (SchemaProperty property in schema.Properties) {
                    property.Name = propertyNames[property.NameIndex];
                }
            }

            return new SchemaData {
                Header = header,
                SchemaUnknownProperties = unknownProps,
                Values = schemaValues,
            };
        }

        private FreeSpace readFreeSpaceValue(SegmentHeader header)
        {
            FreeSpace space = new() {
                Header = header,
                Unknown = this._reader.ReadRawULong(),
                FreeSpaces = new FreeSpaceArea[this._storage.FileHeader.FreeSpaceEntryCount]
            };
            for (int i = 0; i < space.FreeSpaces.Length; i++) {		// TODO: Sometimes when there is an additional freespace definition, it might not match the `FreeSpaceEntryCount` count
                ulong position = this._reader.ReadRawULong();
                uint size = this._reader.ReadUInt();
                space.FreeSpaces[i] = new FreeSpaceArea {
                    Position = position,
                    Size = size
                };
            }
            return space;
        }

        private PreviousSave readPreviousSaveValue(SegmentHeader header) => new() {
            Header = header,
            FileHeader = this.readFileHeader()
        };

        private SchemaSearch readSchemaSearchValue(SegmentHeader header)
        {
            SchemaSearch schemaSearch = new() {
                Header = header
            };
            int schemaCount = this._reader.ReadInt();
            schemaSearch.Entries = [];
            for (int j = 0; j < schemaCount; j++) {
                schemaSearch.Entries.Add(this.readSchemaSearchEntry());
            }
            return schemaSearch;
        }

        private SchemaSearchEntry readSchemaSearchEntry()
        {
            SchemaSearchEntry search = new() {
                SchemaNameIndex = this._reader.ReadUInt()
            };

            ulong sortedIndexCount = this._reader.ReadRawULong();
            search.SortedIndices = new ulong[sortedIndexCount];
            for (ulong i = 0; i < sortedIndexCount; i++) {
                search.SortedIndices[i] = this._reader.ReadRawULong();
            }
            
            uint idIndexesCount = this._reader.ReadUInt();
            SearchEntryObject[][] idEntryObjects = new SearchEntryObject[idIndexesCount][];

            // TODO: Find out what it would mean if there were multiple index lists. Never happened in test files so far
            if (idIndexesCount > 1) {
                Debugger.Break();
            }

            if (idIndexesCount > 0) {
                search.Unknown1 = this._reader.ReadUInt();
                
                for (uint i = 0; i < idIndexesCount; i++) {
                    uint idIndexCount = this._reader.ReadUInt();

                    SearchEntryObject[] entryObjects = new SearchEntryObject[idIndexCount];
                    for (uint j = 0; j < idIndexCount; j++) {
                        ulong handle = this._reader.ReadRawULong();
                        
                        ulong indexCount = this._reader.ReadRawULong();
                        ulong[] indices = new ulong[indexCount];
                        for (ulong k = 0; k < indexCount; k++) {
                            indices[k] = this._reader.ReadRawULong();
                        }

                        entryObjects[j] = new SearchEntryObject {
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
    }
}
