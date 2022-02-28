using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.Exceptions;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using System.Diagnostics;

namespace ACadSharp.IO.DXF
{
	internal class DxfBlockMapSectionReader : DxfSectionReaderBase
	{
		public DxfBlockMapSectionReader(IDxfStreamReader reader, DxfDocumentBuilder builder, NotificationEventHandler notification = null)
			: base(reader, builder, notification)
		{
		}

		public override void Read()
		{
			//Advance to the first value in the section
			this._reader.ReadNext();

			//Loop until the section ends
			while (!this._reader.EndSectionFound)
			{

				if (this._reader.LastValueAsString == DxfFileToken.Block)
					this.readBlock();
				else
					throw new DxfException($"Unexpected token at the begining of a table: {this._reader.LastValueAsString}", this._reader.Line);
			}
		}

		private void readBlock()
		{
			Debug.Assert(this._reader.LastValueAsString == DxfFileToken.Block);

			//Read the table name
			this._reader.ReadNext();

			this.readCommonObjectData(out string name, out ulong handle, out ulong? ownerHandle);

			if (!this._builder.TryGetCadObject(ownerHandle, out BlockRecord record))
			{
				throw new System.Exception();
			}

			DwgEntityTemplate template = new DwgEntityTemplate(record.BlockEntity);

			Debug.Assert(this._reader.LastValueAsString == DxfSubclassMarker.Entity);

			this.readMapped<Entity>(template.CadObject, template);

			Debug.Assert(this._reader.LastValueAsString == DxfSubclassMarker.BlockBegin);

			this.readMapped<Block>(record.BlockEntity, template);

			switch (this._reader.LastValueAsString)
			{
				case DxfFileToken.EndBlock:
					this.readMapped<BlockEnd>(record.BlockEnd, template);
					break;
				default:
					Debug.Fail($"Unhandeled dxf block entity {this._reader.LastValueAsString} at line {this._reader.Line}.");
					break;
			}
		}
	}
}
