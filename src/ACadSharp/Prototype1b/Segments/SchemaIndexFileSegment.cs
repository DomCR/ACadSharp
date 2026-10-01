using System.Collections.Generic;

namespace ACadSharp.Prototype1b.Segments;

public class SchemaIndexFileSegment : FileSegment
{
	public List<string> SchemaNames { get; set; } = new();

	public List<SchemaPropertyPointer> UnknownPropertyPointers { get; set; } = new();

	public List<SchemaPropertyPointer> SchemaUnknownPropertyPointer { get; set; } = new();

	public uint UnknownIndex1 { get; set; }

	public long UnknownMagic { get; set; }

	public List<SchemaPropertyPointer> PropertyPointers { get; set; } = new();
}