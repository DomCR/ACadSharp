using ACadSharp.Prototype1b.Segments;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Prototype1b
{
    public class DataStorage
    {
        public FileHeader FileHeader { get; set; }

        // Information about file state at last revision
        public PreviousSave PreviousSave { get; set; }

        public DataStoragePointers IndexPointers { get; set; }

        // All registered Schemas
        public List<SchemaData> SchemaFields { get; set; }

        // The search object to get the data entries associated with schema entries
        public SchemaSearch SchemaSearch { get; set; }
        public List<DataField> DataFields { get; set; }
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
                        .ToDictionary(x => x.Key, x => x.First());	// TODO: Find out how to correctly handle the case where duplicate handles exist, but have different values

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
	}
}
