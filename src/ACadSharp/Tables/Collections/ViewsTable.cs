namespace ACadSharp.Tables.Collections;

/// <summary>
/// Represents a collection of <see cref="View"/> entries.
/// </summary>
public class ViewsTable : Table<View>
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.TableView;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.VIEW_CONTROL_OBJ;

	internal ViewsTable() : base()
	{
	}

	internal ViewsTable(CadDocument document) : base(document)
	{
	}

	protected override string[] getDefaultEntries()
	{ return new string[] { }; }

	protected override View getDefaultEntry()
	{
		return null;
	}
}