using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.Tables;
using System;

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
				DxfMap map = DxfMap.Create<Block>();

				this._writer.Write(DxfCode.Start, b.BlockEntity.ObjectName);

				this.writeCommonObjectData(b.BlockEntity);

				this.writeMap(map, b.BlockEntity);

				this.processEntities(b);

				DxfMap bendmap = DxfMap.Create<BlockEnd>();

				this._writer.Write(DxfCode.Start, b.BlockEnd.ObjectName);

				this.writeCommonObjectData(b.BlockEnd);

				this.writeMap(bendmap, b.BlockEnd);
			}
		}

		private void processEntities(BlockRecord b)
		{
			if (b.Name == BlockRecord.ModelSpaceName)
			{
				foreach (Entity e in b.Entities)
				{
					this.Holder.Entities.Enqueue(e);
				}
			}
			else
			{
				foreach (Entity e in b.Entities)
				{
					this.writeEntity(e);
				}
			}
		}
	}
}
