using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgBlockTemplate : DwgTableEntryTemplate<Block>
	{
		public ulong? FirstEntityHandle { get; set; }
		public ulong? LastEntityHandle { get; set; }
		public ulong? BeginBlockHandle { get; set; }
		public ulong? EndBlockHandle { get; set; }
		public ulong? LayoutHandle { get; set; }
		public List<ulong> OwnedObjectsHandlers { get; set; } = new List<ulong>();
		public List<ulong> InsertHandles { get; set; } = new List<ulong>();

		public string LayerName { get; set; }

		public DwgBlockTemplate(Block block) : base(block) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (builder.TryGetCadObject(this.BeginBlockHandle, out BlockBegin blockBegin))
			{
				this.CadObject.BlockBegin = blockBegin;
			}

			if (builder.TryGetCadObject(this.BeginBlockHandle, out BlockEnd blockEnd))
			{
				this.CadObject.BlockEnd = blockEnd;
			}

			//TODO: Is necessary to reassign the layout??
			//if (this.LayoutHandle.HasValue && builder.TryGetCadObject<Layout>(this.LayoutHandle.Value, out Layout layout))
			//{
			//	layout.AssociatedBlock = this.CadObject;
			//}

			if (this.FirstEntityHandle.HasValue)
			{
				var entities = this.getEntitiesCollection<Entity>(builder, FirstEntityHandle.Value, LastEntityHandle.Value);
				this.CadObject.Entities.AddRange(entities);
			}
			else
			{
				foreach (ulong handle in this.OwnedObjectsHandlers)
				{
					if (builder.TryGetCadObject<Entity>(handle, out Entity child))
					{
						this.CadObject.Entities.Add(child);
					}
				}
			}

			//TODO: DwgBlockTemplate Process EndBlockHandle ?? 
		}
	}
}
