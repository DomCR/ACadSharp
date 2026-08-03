using System;
using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal class CadFileDataStorage
{
	public List<Schema> Schemes { get; } = new();
}