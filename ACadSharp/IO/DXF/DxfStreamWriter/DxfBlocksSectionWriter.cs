using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.Tables;
using System.Linq;

namespace ACadSharp.IO.DXF
{
	internal class DxfBlocksSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.BlocksSection; } }

		public DxfBlocksSectionWriter(IDxfStreamWriter writer, CadDocument document, CadObjectHolder objectHolder) : base(writer, document, objectHolder) { }

		protected override void writeSection()
		{
			foreach (BlockRecord b in this._document.BlockRecords)
			{
				this.writeBlock(b.BlockEntity);
				this.processEntities(b);
				this.writeBlockEnd(b.BlockEnd);
			}
		}

		private void writeBlock(Block block)
		{
			DxfClassMap map = DxfClassMap.Create<Block>();

			this._writer.Write(DxfCode.Start, block.ObjectName);

			this.writeCommonObjectData(block);

			this.writeCommonEntityData(block);

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.BlockBegin);

			if (!string.IsNullOrEmpty(block.XrefPath))
			{
				this._writer.Write(1, block.XrefPath, map);
			}
			this._writer.Write(2, block.Name, map);
			this._writer.Write(70, (short)block.Flags, map);

			this._writer.Write(10, block.BasePoint, map);
		
			this._writer.Write(3, block.Name, map);
			this._writer.Write(4, block.Comments, map);
		}

		private void processEntities(BlockRecord b)
		{
			if (b.Name == BlockRecord.ModelSpaceName || b.Name == BlockRecord.PaperSpaceName)
			{
				foreach (Entity e in b.Entities.Concat(b.Viewports))
				{
					this.Holder.Entities.Enqueue(e);
				}
			}
			else
			{
				foreach (Entity e in b.Entities.Concat(b.Viewports))
				{
					this.writeEntity(e);
				}
			}
		}

		private void writeBlockEnd(BlockEnd block)
		{
			this._writer.Write(DxfCode.Start, block.ObjectName);

			this.writeCommonObjectData(block);

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Entity);

			this._writer.Write(8, block.Layer.Name);

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.BlockEnd);
		}
	}
}
