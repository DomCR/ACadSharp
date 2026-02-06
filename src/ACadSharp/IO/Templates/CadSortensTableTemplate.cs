using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadSortensTableTemplate : CadTemplate<SortEntitiesTable>
	{
		public ulong? BlockOwnerHandle { get; set; }

		public List<(ulong?, ulong?)> Values { get; } = new List<(ulong?, ulong?)>();

		public CadSortensTableTemplate() : base(new SortEntitiesTable()) { }

		public CadSortensTableTemplate(SortEntitiesTable cadObject) : base(cadObject) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (builder.TryGetCadObject(this.BlockOwnerHandle, out CadObject owner))
			{
				// Not always a block
				if (owner is BlockRecord record)
				{
					this.CadObject.BlockOwner = record;
				}
				else if (owner is null)
				{
					builder.Notify($"Block owner for SortEntitiesTable {this.CadObject.Handle} not found", NotificationType.Warning);
					return;
				}
				else
				{
					builder.Notify($"Block owner for SortEntitiesTable {this.CadObject.Handle} is not a block {owner.GetType().FullName} | {owner.Handle}", NotificationType.Warning);
					return;
				}
			}

			foreach ((ulong?, ulong?) pair in this.Values)
			{
				if (builder.TryGetCadObject(pair.Item2, out Entity entity))
				{
					this.CadObject.Add(entity, pair.Item1.Value);
				}
				else
				{
					builder.Notify($"Entity in SortEntitiesTable {this.CadObject.Handle} not found {pair.Item2}", NotificationType.Warning);
				}
			}
		}
	}
}
