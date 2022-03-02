using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using ACadSharp.Tables;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgBlockRecordTemplate : DwgTableEntryTemplate<BlockRecord>
	{
		public ulong? FirstEntityHandle { get; set; }

		public ulong? LastEntityHandle { get; set; }

		public ulong? BeginBlockHandle { get; set; }

		public ulong? EndBlockHandle { get; set; }

		public ulong? LayoutHandle { get; set; }

		public List<ulong> OwnedObjectsHandlers { get; set; } = new List<ulong>();

		public List<ulong> InsertHandles { get; set; } = new List<ulong>();

		public string LayerName { get; set; }

		public DwgBlockRecordTemplate(BlockRecord block) : base(block) { }

		public override bool AddHandle(int dxfcode, ulong handle)
		{
			bool value = base.AddHandle(dxfcode, handle);
			if (value)
				return value;

			switch (dxfcode)
			{
				case 340:
					LayoutHandle = handle;
					value = true;
					break;
				default:
					break;
			}

			return value;
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (builder.TryGetCadObject(this.EndBlockHandle, out BlockEnd blockEnd))
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
		}

		public void SetBlockToRecord(CadDocumentBuilder builder)
		{
			if (!builder.TryGetCadObject(this.BeginBlockHandle, out Block block))
				return;

			this.CadObject.Name = block.Name;

			block.Flags = this.CadObject.BlockEntity.Flags;
			block.BasePoint = this.CadObject.BlockEntity.BasePoint;
			block.XrefPath = this.CadObject.BlockEntity.XrefPath;
			block.Comments = this.CadObject.BlockEntity.Comments;

			this.CadObject.BlockEntity = block;

		}
	}
}
