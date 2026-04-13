namespace ACadSharp.Tables;

internal class ViewportEntityHeader : TableEntry
{
	public override string Name { get => base.name; set => base.name = value; }

	public override string ObjectName { get; }

	public override ObjectType ObjectType { get => ObjectType.VP_ENT_HDR; }
}