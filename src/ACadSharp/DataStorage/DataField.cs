using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal partial class DataField : FileSegment
{
	public List<DataEntry> DataEntries { get; set; } = new();
}