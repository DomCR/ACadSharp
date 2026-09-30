using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal class AcdsRecordColumn
{
	public short DataType { get; set; }

	public string Name { get; set; } = string.Empty;

	public KeyValuePair<int, object> CodeValuePair { get; set; }

	public override string ToString()
	{
		return $"{this.Name} : {this.DataType} : {this.CodeValuePair}";
	}
}