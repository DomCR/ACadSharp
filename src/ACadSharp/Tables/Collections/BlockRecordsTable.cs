namespace ACadSharp.Tables.Collections
{
	public class BlockRecordsTable : Table<BlockRecord>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.BLOCK_CONTROL_OBJ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableBlockRecord;

		protected override string[] defaultEntries { get { return new string[] { BlockRecord.ModelSpaceName, BlockRecord.PaperSpaceName }; } }

		internal BlockRecordsTable() { }

		internal BlockRecordsTable(CadDocument document) : base(document) { }

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
	}
}