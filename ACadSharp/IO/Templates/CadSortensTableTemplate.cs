using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadSortensTableTemplate : CadTemplate<SortEntitiesTable>
	{
		public ulong? BlockOwnerHandle { get; set; }

		public List<(ulong, ulong)> Values { get; } = new List<(ulong, ulong)>();

		public CadSortensTableTemplate() : base(new SortEntitiesTable()) { }

		public CadSortensTableTemplate(SortEntitiesTable cadObject) : base(cadObject) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (builder.TryGetCadObject(BlockOwnerHandle, out CadObject owner))
			{
				// Not always a block
				if (owner is BlockRecord record)
				{
					this.CadObject.BlockOwner = record;
				}
				else if (owner is null)
				{
					builder.Notify($"Block owner for SortEntitiesTable {this.CadObject.Handle} not found", NotificationType.Warning);
				}
				else
				{
					builder.Notify($"Block owner for SortEntitiesTable {this.CadObject.Handle} is not a block {owner.GetType().FullName} | {owner.Handle}", NotificationType.Warning);
				}
			}

			foreach ((ulong, ulong) pair in Values)
			{
				if (builder.TryGetCadObject(pair.Item2, out Entity entity))
				{
					SortEntitiesTable.Sorter sorter = new SortEntitiesTable.Sorter
					{
						SortHandle = pair.Item1,
						Entity = entity
					};
					this.CadObject.Sorters.Add(sorter);
				}
				else
				{
					builder.Notify($"Entity in SortEntitiesTable {this.CadObject.Handle} not found {pair.Item2}", NotificationType.Warning);
				}
			}
		}
	}
}
