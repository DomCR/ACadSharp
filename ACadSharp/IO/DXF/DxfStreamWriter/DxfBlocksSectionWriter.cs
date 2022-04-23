using ACadSharp.Blocks;
using ACadSharp.Tables;

namespace ACadSharp.IO.DXF
{
	internal class DxfBlocksSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.BlocksSection; } }

		public DxfBlocksSectionWriter(IDxfStreamWriter writer, CadDocument document) : base(writer, document)
		{
		}

		protected override void writeSection()
		{
			foreach (BlockRecord b in _document.BlockRecords)
			{
				DxfMap map = DxfMap.Create<Block>();

				this._writer.Write(DxfCode.Start, b.BlockEntity.ObjectName);

				this.writeCommonObjectData(b.BlockEntity);

				this.writeMap(map, b.BlockEntity);

				DxfMap bendmap = DxfMap.Create<BlockEnd>();

				this._writer.Write(DxfCode.Start, b.BlockEnd.ObjectName);

				this.writeCommonObjectData(b.BlockEnd);

				this.writeMap(bendmap, b.BlockEnd);
			}
		}
	}
}
