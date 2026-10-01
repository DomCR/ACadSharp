using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal class CadFileDataStorage
{
	public List<AcdsRecord> Records { get; } = new();

	public List<AcdsSchema> Schemes { get; } = new();

	public const string AsmData = "ASM_Data";

	public const string Id = "AcDbDs::ID";
}