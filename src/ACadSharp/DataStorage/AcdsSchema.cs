using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal class AcdsSchema
{
	public List<AcdsSchemaRecord> EmbeddedRecords { get; } = new();

	public uint Index { get; set; }

	public List<ulong> Indices { get; } = new();

	public string Name { get; set; }

	public List<AcdsSchemaProperty> Properties { get; } = new();

	public override string ToString()
	{
		return $"{this.Index} : {this.Name}";
	}
}