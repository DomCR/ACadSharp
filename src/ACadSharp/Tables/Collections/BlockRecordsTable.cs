namespace ACadSharp.Tables.Collections;

/// <summary>
/// Represents a collection of <see cref="BlockRecord"/> entries.
/// </summary>
public class BlockRecordsTable : Table<BlockRecord>
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.TableBlockRecord;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.BLOCK_CONTROL_OBJ;

	internal BlockRecordsTable()
	{ }

	internal BlockRecordsTable(CadDocument document) : base(document)
	{
	}

	/// <inheritdoc/>
	public override void Add(BlockRecord item)
	{
		if (item.IsAnonymous && this.Contains(item.Name))
		{
			if (this[item.Name].Equals(item))
			{
				throw new System.ArgumentException($"The BlockRecord with name {item.Name} has already been added.");
			}

			item.Name = this.createName(BlockRecord.AnonymousPrefix);
		}

		base.Add(item);
	}

	/// <inheritdoc/>
	public override BlockRecord GetDefaultEntry()
	{
		return null;
	}

	protected override string[] getDefaultEntries()
	{ return new string[] { BlockRecord.ModelSpaceName, BlockRecord.PaperSpaceName }; }
}