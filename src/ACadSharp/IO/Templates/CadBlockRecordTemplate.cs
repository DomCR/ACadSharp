using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSUtilities.Extensions;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadBlockRecordTemplate : CadTableEntryTemplate<BlockRecord>, ICadOwnerTemplate
	{
		public ulong? FirstEntityHandle { get; set; }

		public ulong? LastEntityHandle { get; set; }

		public ulong? BeginBlockHandle { get; set; }

		public ulong? EndBlockHandle { get; set; }

		public ulong? LayoutHandle { get; set; }

		public HashSet<ulong> OwnedObjectsHandlers { get; set; } = new();

		public List<ulong> InsertHandles { get; set; } = new();

		public CadBlockEntityTemplate BlockEntityTemplate { get; set; }

		public CadBlockRecordTemplate() : base(new BlockRecord()) { }

		public CadBlockRecordTemplate(BlockRecord block) : base(block) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (builder.TryGetCadObject(this.LayoutHandle, out Layout layout))
			{
				this.CadObject.Layout = layout;
			}

			if (this.FirstEntityHandle.HasValue && this.FirstEntityHandle.Value != 0)
			{
				foreach (Entity e in this.getEntitiesCollection<Entity>(builder, this.FirstEntityHandle.Value, this.LastEntityHandle.Value))
				{
					this.addEntity(builder, e);
				}
			}
			else
			{
				if (this.BlockEntityTemplate != null)
				{
					this.OwnedObjectsHandlers.UnionWith(this.BlockEntityTemplate.OwnedObjectsHandlers);
				}

				foreach (ulong handle in this.OwnedObjectsHandlers)
				{
					if (builder.TryGetCadObject(handle, out Entity child))
					{
						this.addEntity(builder, child);
					}
				}
			}
		}

		public void SetBlockToRecord(CadDocumentBuilder builder, DWG.DwgHeaderHandlesCollection headerHandles)
		{
			if (builder.TryGetCadObject(this.BeginBlockHandle, out Block block))
			{
				if (!block.Name.IsNullOrEmpty())
				{
					this.CadObject.Name = block.Name;
				}

				block.Flags = this.CadObject.BlockEntity.Flags;
				block.BasePoint = this.CadObject.BlockEntity.BasePoint;
				block.XRefPath = this.CadObject.BlockEntity.XRefPath;
				block.Comments = this.CadObject.BlockEntity.Comments;
				block.IsUnloaded = this.CadObject.BlockEntity.IsUnloaded;

				this.CadObject.BlockEntity = block;
			}

			if (builder.TryGetCadObject(this.EndBlockHandle, out BlockEnd blockEnd))
			{
				this.CadObject.BlockEnd = blockEnd;
			}

			this.ensureCorrectNaming(builder, headerHandles.MODEL_SPACE, BlockRecord.ModelSpaceName);
			this.ensureCorrectNaming(builder, headerHandles.PAPER_SPACE, BlockRecord.PaperSpaceName);
		}

		private void ensureCorrectNaming(CadDocumentBuilder builder, ulong? handle, string expected)
		{
			if (this.CadObject.Handle == handle
				&& !this.CadObject.Name.Equals(expected, System.StringComparison.InvariantCultureIgnoreCase))
			{
				builder.Notify($"Invalid name for {this.CadObject.Name} changed to {expected}", NotificationType.Warning);
				this.CadObject.Name = expected;
			}
		}

		private void addEntity(CadDocumentBuilder builder, Entity entity)
		{
			if (!builder.KeepUnknownEntities && entity is UnknownEntity)
			{
				return;
			}

			this.CadObject.Entities.Add(entity);
		}
	}
}
