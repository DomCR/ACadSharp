using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.DataStorage;

internal class CadFileDataStorage
{
	public const string Id = "AcDbDs::ID";
	public const string AsmData = "ASM_Data";

	public List<AcdsRecord> Records { get; } = new();

	public List<AcdsSchema> Schemes { get; } = new();
}