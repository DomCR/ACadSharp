namespace ACadSharp.Tables.Collections;

/// <summary>
/// Represents a collection of <see cref="Layer"/> entries.
/// </summary>
public class LayersTable : Table<Layer>
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.TableLayer;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.LAYER_CONTROL_OBJ;

	internal LayersTable()
	{ }

	internal LayersTable(CadDocument document) : base(document)
	{
	}

	/// <inheritdoc/>
	public override Layer GetDefaultEntry()
	{
		return this[Layer.DefaultName];
	}

	protected override string[] getDefaultEntries()
	{ return new string[] { Layer.DefaultName }; }
}