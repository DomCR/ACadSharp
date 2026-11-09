namespace ACadSharp.Prototype1b
{
	public class SchemaSearchEntry
    {
        public uint SchemaNameIndex { get; set; }
        public ulong[] SortedIndices { get; set; }
        public uint Unknown1 { get; set; }
        public SearchEntryObject[][] IdEntryObjects { get; set; }

        // Resolved values
        public string SchemaName { get; set; }
    }
}
