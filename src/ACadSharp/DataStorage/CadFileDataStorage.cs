using System;
using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal class AcdsPropertyDescriptor
{
}

internal class Schema
{
	public uint Index { get; set; }

	public List<ulong> Indexes { get; } = new();

	public string Name { get; set; }

	public List<SchemaProperty> Properties { get; } = new();

	public List<AcdsPropertyDescriptor> PropertyDescriptors { get; } = new();
}

internal class CadFileDataStorage
{
	public List<Schema> Schemes { get; } = new();
}

internal class SchemaProperty
{
	public static readonly uint[] TypeSizes = new uint[] { 0, 0, 2, 1, 2, 4, 8, 1, 2, 4, 8, 4, 8, 0, 0, 0 };

	public string Name { get; set; }

	public uint NameIndex { get; set; }

	public SchemaPropertyFlags PropertyFlags { get; set; }

	public uint PropertyValueCount { get; set; }

	public uint? Type { get; set; }

	public uint TypeSize { get; set; }

	public uint Unknown1 { get; set; }

	public uint Unknown2 { get; set; }

	public byte[,] Values { get; set; }

	public override string ToString()
	{
		return this.Name;
	}
}

[Flags]
internal enum SchemaPropertyFlags
{
	None = 0,
	Unknown1 = 1,
	NoType = 2,
	Unknown2 = 8
}