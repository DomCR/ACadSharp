namespace ACadSharp.Tables.Collections;

/// <summary>
/// Represents a collection of <see cref="UCS"/> entries.
/// </summary>
public class UCSTable : Table<UCS>
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.TableUcs;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.UCS_CONTROL_OBJ;

	internal UCSTable() : base()
	{
	}

	internal UCSTable(CadDocument document) : base(document)
	{
	}

	/// <inheritdoc/>
	public override UCS GetDefaultEntry()
	{
		return null;
	}

	protected override string[] getDefaultEntries()
	{ return new string[] { }; }
}