using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal partial class DataField : FileSegment
{
	public List<DataEntry> DataEntries { get; set; } = new();

	public DataField()
		: base("_data_")
	{
	}
}