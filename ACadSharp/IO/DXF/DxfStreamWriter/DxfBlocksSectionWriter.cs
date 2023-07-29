using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.Tables;
using System;
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
				writeBlock(b);
			}
		}

		private void writeBlock(BlockRecord block)
		{
			DxfMap map = DxfMap.Create<Block>();
			DxfClassMap blockMap = map.SubClasses[block.SubclassMarker];

			this._writer.Write(DxfCode.Start, block.BlockEntity.ObjectName);

			this.writeCommonObjectData(block.BlockEntity);

			//this.writeMap(map, b.BlockEntity);
			this.writeCommonEntity(block.BlockEntity);

			this._writer.Write(DxfCode.BlockName, block.Name, blockMap);

			this.processEntities(block);

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
					this.writeMappedObject(e);
				}
			}
		}

		private void writeBlockEnd(BlockEnd block)
		{

			DxfMap bendmap = DxfMap.Create<BlockEnd>();

			this._writer.Write(DxfCode.Start, block.ObjectName);

			this.writeCommonObjectData(block);

			this.writeMap(bendmap, block);
		}
	}
}
