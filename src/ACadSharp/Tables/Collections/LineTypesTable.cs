namespace ACadSharp.Tables.Collections;

/// <summary>
/// Represents a collection of <see cref="LineType"/> entries.
/// </summary>
public class LineTypesTable : Table<LineType>
{
	/// <summary>
	/// Get the ByBlock entry in the table
	/// </summary>
	public LineType ByBlock { get { return this[LineType.ByBlockName]; } }

	/// <summary>
	/// Get the ByLayer entry in the table
	/// </summary>
	public LineType ByLayer { get { return this[LineType.ByLayerName]; } }

	/// <summary>
	/// Get the Continuous entry in the table
	/// </summary>
	public LineType Continuous { get { return this[LineType.ContinuousName]; } }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.TableLinetype;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.LTYPE_CONTROL_OBJ;

	internal LineTypesTable() : base()
	{
	}

	internal LineTypesTable(CadDocument document) : base(document)
	{
	}

	/// <inheritdoc/>
	public override LineType GetDefaultEntry()
	{
		return this[LineType.ByLayerName];
	}

	protected override string[] getDefaultEntries()
	{
		return new string[]
		{
				LineType.ByLayerName,
				LineType.ByBlockName,
				LineType.ContinuousName
		};
	}
}