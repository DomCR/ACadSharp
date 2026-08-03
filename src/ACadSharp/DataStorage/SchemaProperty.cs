namespace ACadSharp.DataStorage;

internal class SchemaProperty
{
	public static readonly uint[] TypeSizes = new uint[] { 0, 0, 2, 1, 2, 4, 8, 1, 2, 4, 8, 4, 8, 0, 0, 0 };

	public string Name { get; set; }

	public uint NameIndex { get; set; }

	public SchemaPropertyFlags PropertyFlags { get; set; }

	public uint PropertyValueCount { get; set; }

	public uint? Type { get; set; }

	public uint TypeSize { get; set; }

	public uint Unknown1 { get; set; }

	public uint Unknown2 { get; set; }

	public byte[,] Values { get; set; }

	public override string ToString()
	{
		return this.Name;
	}
}
