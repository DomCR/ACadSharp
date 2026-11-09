using System.IO;

namespace ACadSharp.DataStorage;

internal class AcdsRecordColumn
{
	public short DataType { get; set; }

	public string Name { get; set; } = string.Empty;

	public ulong Handle { get; set; }

	public MemoryStream Data { get; set; }

	public override string ToString()
	{
		return $"{this.Name} : {this.DataType}";
	}
}