using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal class AcdsSchema
{
	public List<AcdsSchemaRecord> EmbeddedRecords { get; } = new();

	public uint Index { get; set; }

	public List<ulong> Indexes { get; } = new();

	public string Name { get; set; }

	public List<AcdsSchemaProperty> Properties { get; } = new();
}

internal class AcdsRecord
{
	public uint Index { get; set; }

	public List<AcdsRecordColumn> Columns { get; } = new();
}

internal class AcdsRecordColumn
{
	
}