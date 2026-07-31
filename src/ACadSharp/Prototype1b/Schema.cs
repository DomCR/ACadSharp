namespace ACadSharp.Prototype1b
{
	public class Schema
    {
		public const string ACIS = "AcDb3DSolid_ASM_Data";
		public const string THUMBNAIL = "AcDb_Thumbnail_Schema";
		public const string TREATED_AS_OBJECT_DATA = "AcDbDs::TreatedAsObjectDataSchema";
		public const string LEGACY = "AcDbDs::LegacySchema";
		public const string INDEXED_PROPERTY = "AcDbDs::IndexedPropertySchema";
		public const string HANDLE_ATTRIBUTE = "AcDbDs::HandleAttributeSchema";

		public uint Index { get; set; }
        public string Name { get; set; }
        public ulong[] Indices { get; set; }
        // TODO: Find out what PropertyDescriptors are
        public SchemaProperty[] Properties { get; set; }
    }
}
