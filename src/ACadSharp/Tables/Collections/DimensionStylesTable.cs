namespace ACadSharp.Tables.Collections;

/// <summary>
/// Represents a collection of <see cref="DimensionStyle"/> entries.
/// </summary>
public class DimensionStylesTable : Table<DimensionStyle>
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.TableDimstyle;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.DIMSTYLE_CONTROL_OBJ;

	internal DimensionStylesTable() : base()
	{
	}

	internal DimensionStylesTable(CadDocument document) : base(document)
	{
	}

	protected override string[] getDefaultEntries()
	{ return new string[] { DimensionStyle.DefaultName }; }

	protected override DimensionStyle getDefaultEntry()
	{
		return this[DimensionStyle.DefaultName];
	}
}