using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal class CadFileDataStorage
{
	public List<AcdsRecord> Records { get; } = new();

	public List<AcdsSchema> Schemes { get; } = new();
}