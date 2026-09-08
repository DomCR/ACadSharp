namespace ACadSharp.Tables.Collections;

/// <summary>
/// Represents a collection of <see cref="TextStyle"/> entries.
/// </summary>
public class TextStylesTable : Table<TextStyle>
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.TableStyle;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.STYLE_CONTROL_OBJ;

	internal TextStylesTable() : base()
	{
	}

	internal TextStylesTable(CadDocument document) : base(document)
	{
	}

	/// <inheritdoc/>
	public override TextStyle GetDefaultEntry()
	{
		return this[TextStyle.DefaultName];
	}

	protected override string[] getDefaultEntries()
	{ return new string[] { TextStyle.DefaultName }; }
}