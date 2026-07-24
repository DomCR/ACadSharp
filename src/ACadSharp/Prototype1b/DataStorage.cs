using ACadSharp.Entities;
using ACadSharp.Prototype1b.Segments;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Prototype1b
{
    public class DataStorage
    {
		/// <summary>
		/// General information about the header of the <see cref="DataStorage"/> and Prototype_1b section
		/// </summary>
        public FileHeader FileHeader { get; set; }

        /// <summary>
		/// Information about file state at last revision
		/// </summary>
        public PreviousSave PreviousSave { get; set; }

		/// <summary>
		/// Pointers to individual segments of the Prototype_1b section referenced to by the <see cref="FileHeader"/>
		/// </summary>
        public DataStoragePointers IndexPointers { get; set; }

        /// <summary>
		/// All registered Schemas
		/// </summary>
        public List<SchemaData> SchemaFields { get; set; }

        /// <summary>
		/// The search object to get the data entries associated with schema entries
		/// </summary>
        public SchemaSearch SchemaSearch { get; set; }

		/// <summary>
		/// Fields containing data entries with bytes of embedded files, or references to <see cref="Blobs"/>
		/// </summary>
        public List<DataField> DataFields { get; set; }

		/// <summary>
		/// Blobs of bytes referenced to by <see cref="DataFields"/>. 
		/// Embedded files larger than 0x40000 bytes will be split into one or more blobs
		/// </summary>
		public List<Blob01> Blobs { get; set; }

        public Schema GetSchemaByName(string name)
        {
            foreach (SchemaData data in this.SchemaFields) {
                foreach (Schema schema in data.Values) {
                    if (schema.Name == name) {
                        return schema;
                    }
                }
            }
            return null;
        }

		public List<DataEntry> GetSchemaData(Schema schema)
        {
            if (schema == null) return [];

            foreach (SchemaSearchEntry search in this.SchemaSearch.Entries) {
                if (search.SchemaNameIndex == schema.Index) {
                    List<DataEntry> data = [];
                    Dictionary<ulong, DataEntry> allDataEntries = this.DataFields
                        .SelectMany(x => x.Entries)
                        .Where(x => x.Header.SchemaIndex == schema.Index)
                        .GroupBy(x => x.Header.Handle)
                        .ToDictionary(x => x.Key, x => x.First());

                    foreach (SearchEntryObject[] entries in search.IdEntryObjects) {
                        foreach (SearchEntryObject entry in entries) {
                            if (entry.Indices.Length == 0) continue;	// TODO: Find out what this index really means (maybe index inside the data field entry where the item is?) and why there can be multiple arrays and why some have none at all
                            data.Add(allDataEntries[entry.Handle]);
                        }
                    }
                    return data;
                }
            }

            return [];
		}

		/// <summary>
		/// Get all <see cref="DataEntry"/> fields referenced by the given schema. 
		/// Known schema names are available as constants in the <see cref="Schema"/> class.
		/// To get all ACIS data stored in the <see cref="DataStorage"/> for example, pass <see cref="Schema.ACIS"/> as <paramref name="schemaName"/>
		/// </summary>
		/// <param name="schemaName">The schema name to get all data entries for</param>
		/// <returns>A list of all data entries referenced by the given schema</returns>
		public List<DataEntry> GetSchemaData(string schemaName)
		{
			return this.GetSchemaData(this.GetSchemaByName(schemaName));
		}

		/// <summary>
		/// Checks if a data entry with the given handle exists in the <see cref="DataStorage"/>
		/// </summary>
		/// <param name="handle">The handle to check</param>
		/// <returns>Whether or not a data entry with the given handle exists</returns>
		public bool ContainsDataWithHandle(ulong handle)
		{
			foreach (DataField field in this.DataFields) {
				foreach (DataEntry entry in field.Entries) {
					if (entry.Header.Handle == handle) {
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Get the bytes of the file stored by a data field with the given handle.
		/// If the entry references file blobs, they will be resolved.
		/// </summary>
		/// <param name="handle">The handle of the data entry to resolve</param>
		/// <returns>The bytes of the resolved data entry</returns>
		/// <exception cref="KeyNotFoundException"></exception>
		public byte[] GetDataByHandle(ulong handle)
		{
			foreach (DataField field in this.DataFields) {
				foreach (DataEntry entry in field.Entries) {
					if (entry.Header.Handle == handle) {
						return this.ResolveDataEntry(entry);
					}
				}
			}

			throw new KeyNotFoundException($"Unable to find data in DataStorage for handle {handle}");
		}

		/// <summary>
		/// Resolve the bytes of the provided <see cref="DataEntry"/>. 
		/// If the entry references file blobs, they will be resolved.
		/// </summary>
		/// <param name="entry">The data entry to resolve</param>
		/// <returns>The bytes of the resolved data entry</returns>
		public byte[] ResolveDataEntry(DataEntry entry)
		{
			byte[] bytes = entry.Value.Data;
			if (entry.Value.BlobReference != null) {
				List<Blob01> blobs = [];
				foreach ((uint segidx, uint _size) in entry.Value.BlobReference.SegmentPointers) {
					blobs.Add(this.Blobs.First(b => b.Header.SegmentIndex == segidx));
				}

				// There could probably be instances of blob01 entries not being contiguous (only when a full blob01 segment would only contain zeros).
				// blob01 segments have a max size of 0xFFFB0 bytes, meaning for a segment to be missing here, it would have to contain almost 1MB of 
				// bytes with value 0. It is unknown whether this is even handled the same way as in the section map of the file header of a DWG file.

				bytes = blobs.OrderBy(x => x.PageIndex).SelectMany(x => x.Data).ToArray();
			}
			return bytes;
		}

		/// <summary>
		/// Get the bytes of an embedded file with a given handle. 
		/// This can be the handle of a <see cref="ModelerGeometry"/> referencing an ACIS file or
		/// a handle for a thunbnail
		/// </summary>
		/// <param name="handle">The handle of the data entry to get</param>
		/// <returns>The bytes of the embedded file</returns>
		public byte[] this[ulong handle] 
		{
			get 
			{
				return this.GetDataByHandle(handle);
			}
		}

		/// <summary>
		/// Get the bytes of an embedded file for the given <see cref="ModelerGeometry"/>.
		/// </summary>
		/// <param name="geometry">The entity to get the stored file data for</param>
		/// <returns>The bytes of the embedded file</returns>
		public byte[] this[ModelerGeometry geometry]
		{
			get
			{
				return this.GetDataByHandle(geometry.Handle);
			}
		}
	}
}
