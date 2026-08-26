namespace ACadSharp.Tables.Collections;

/// <summary>
/// Represents a collection of <see cref="AppId"/> entries.
/// </summary>
public class AppIdsTable : Table<AppId>
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.TableAppId;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.APPID_CONTROL_OBJ;

	internal AppIdsTable() : base()
	{
	}

	internal AppIdsTable(CadDocument document) : base(document)
	{
	}

	protected override string[] getDefaultEntries()
	{ return new string[] { AppId.DefaultName }; }

	protected override AppId getDefaultEntry()
	{
		return this[AppId.DefaultName];
	}
}