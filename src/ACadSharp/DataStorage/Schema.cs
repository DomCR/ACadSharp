using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal class Schema
{
	public List<AcdsRecord> EmbeddedRecords { get; } = new();

	public uint Index { get; set; }

	public List<ulong> Indexes { get; } = new();

	public string Name { get; set; }

	public List<SchemaProperty> Properties { get; } = new();
}
