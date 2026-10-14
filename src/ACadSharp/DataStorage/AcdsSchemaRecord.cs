namespace ACadSharp.DataStorage;

internal class AcdsSchemaRecord
{
	public SchemaRecordFlags Flags { get; set; }

	public int Id { get; set; }

	public int Index { get; set; }

	public string Name { get; set; }

	public uint NameIndex { get; set; }

	public uint Type { get; set; }

	public uint TypeSize { get; set; }

	public uint Unknown1 { get; set; }

	public uint Unknown2 { get; set; }

	public short Value291 { get; set; }

	public System.Collections.Generic.List<byte[]> Values { get; } = new System.Collections.Generic.List<byte[]>();

	public static readonly uint[] TypeSizes = new uint[] { 0, 0, 2, 1, 2, 4, 8, 1, 2, 4, 8, 4, 8, 0, 0, 0 };

	public override string ToString()
	{
		return $"{this.Id} | {this.Name}";
	}
}