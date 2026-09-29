using System;
using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal class CadFileDataStorage
{
	public List<AcdsSchema> Schemes { get; } = new();
}