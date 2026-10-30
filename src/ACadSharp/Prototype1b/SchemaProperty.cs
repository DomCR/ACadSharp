using System;

namespace ACadSharp.Prototype1b
{
	[Obsolete("Equivalent to DataStorage.SchemaProperty")]
	public class SchemaProperty
	{
		public string Name { get; set; }

		public uint NameIndex { get; set; }

		public uint PropertyFlags { get; set; }

		public uint PropertyValueCount { get; set; }

		public uint? Type { get; set; }

		public uint TypeSize { get; set; }

		public uint Unknown1 { get; set; }

		public uint Unknown2 { get; set; }

		public byte[,] Values { get; set; }
	}
}