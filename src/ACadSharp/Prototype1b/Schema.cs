using System;

namespace ACadSharp.Prototype1b
{
	[Obsolete("Replace for DataStorage.Schema")]
	public class Schema
	{
		public uint Index { get; set; }

		public ulong[] Indices { get; set; }

		public string Name { get; set; }

		// TODO: Find out what PropertyDescriptors are
		public SchemaProperty[] Properties { get; set; }

		public const string ACIS = "AcDb3DSolid_ASM_Data";

		public const string HANDLE_ATTRIBUTE = "AcDbDs::HandleAttributeSchema";

		public const string INDEXED_PROPERTY = "AcDbDs::IndexedPropertySchema";

		public const string LEGACY = "AcDbDs::LegacySchema";

		public const string THUMBNAIL = "AcDb_Thumbnail_Schema";

		public const string TREATED_AS_OBJECT_DATA = "AcDbDs::TreatedAsObjectDataSchema";
	}
}