using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.Exceptions;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using System.Collections.Generic;
using System.Diagnostics;

namespace ACadSharp.IO.DXF
{
	internal class DxfBlockSectionReader : DxfSectionReaderBase
	{
		public DxfBlockSectionReader(IDxfStreamReader reader, DxfDocumentBuilder builder, NotificationEventHandler notification = null)
			: base(reader, builder, notification)
		{
		}

		public override void Read()
		{
			//Advance to the first value in the section
			this._reader.ReadNext();

			//Loop until the section ends
			while (this._reader.LastValueAsString != DxfFileToken.EndSection)
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

			this.readCommonObjectData(out string name, out ulong handle, out ulong? ownerHandle, out ulong? xdictHandle, out List<ulong> reactors);

			if (!this._builder.TryGetCadObject(ownerHandle, out BlockRecord record))
			{
				throw new DxfException($"Block with handle {handle} and name {name} doesn't have a record");
			}

			//Assign the handle to the entity
			record.BlockEntity.Handle = handle;

			CadEntityTemplate template = new CadEntityTemplate(record.BlockEntity);
			template.OwnerHandle = ownerHandle;
			template.XDictHandle = xdictHandle;
			template.ReactorsHandles = reactors;

			Debug.Assert(this._reader.LastValueAsString == DxfSubclassMarker.Entity);

			this.readMapped<Entity>(record.BlockEntity, template);

			Debug.Assert(this._reader.LastValueAsString == DxfSubclassMarker.BlockBegin);

			this.readMapped<Block>(record.BlockEntity, template);

			while (this._reader.LastValueAsString != DxfFileToken.EndBlock)
			{
				CadEntityTemplate entityTemplate = this.readEntity();

				if (entityTemplate == null)
					continue;

				//Add the object and the template to the builder
				this._builder.AddTemplate(entityTemplate);
				record.Entities.Add(entityTemplate.CadObject);
			}

			this.readBlockEnd(record.BlockEnd);
			this._builder.AddTemplate(template);
		}

		private void readBlockEnd(BlockEnd block)
		{
			CadEntityTemplate template = new CadEntityTemplate(block);

			this.readCommonObjectData(template);

			this.readMapped<Entity>(block, template);

			this.readMapped<BlockEnd>(block, template);

			this._builder.AddTemplate(template);
		}
	}
}
