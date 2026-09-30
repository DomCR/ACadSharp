using ACadSharp.Objects.Evaluations;
using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal class AcdsRecord
{
	public List<AcdsRecordColumn> Columns { get; } = new();

	public uint Index { get; set; }
}

internal class AcdsSchema
{
	public List<AcdsSchemaRecord> EmbeddedRecords { get; } = new();

	public uint Index { get; set; }

	public List<ulong> Indexes { get; } = new();

	public string Name { get; set; }

	public List<AcdsSchemaProperty> Properties { get; } = new();
}

internal class AcdsRecordColumn
{
	public short DataType { get; set; }

	public string Name { get; set; } = string.Empty;

	public KeyValuePair<int, object> CodeValuePair { get; set; }
}