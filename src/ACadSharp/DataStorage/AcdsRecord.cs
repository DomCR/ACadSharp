using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal class AcdsRecord
{
	public Dictionary<string, AcdsRecordColumn> Columns { get; } = new();

	public uint Index { get; set; }

	public override string ToString()
	{
		return $"{this.Index} : {nameof(AcdsRecord)}";
	}
}
