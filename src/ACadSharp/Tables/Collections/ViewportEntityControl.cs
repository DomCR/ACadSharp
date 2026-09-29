namespace ACadSharp.Tables.Collections;

internal class ViewportEntityControl : Table<ViewportEntityHeader>
{
	public override ObjectType ObjectType { get => ObjectType.VP_ENT_HDR_CTRL_OBJ; }

	public ViewportEntityControl(CadDocument document) : base(document)
	{
	}

	protected override string[] getDefaultEntries()
	{
		return new string[] { };
	}

	public override ViewportEntityHeader GetDefaultEntry()
	{
		return null;
	}
}