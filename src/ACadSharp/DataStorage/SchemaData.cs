using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal class SchemaData : FileSegment
{
	public List<AcdsSchema> Schemes { get; } = new();
}
