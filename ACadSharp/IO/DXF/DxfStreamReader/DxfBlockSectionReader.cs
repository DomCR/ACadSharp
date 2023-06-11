using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.Exceptions;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace ACadSharp.IO.DXF
{
	internal class DxfBlockSectionReader : DxfSectionReaderBase
	{
		public DxfBlockSectionReader(IDxfStreamReader reader, DxfDocumentBuilder builder)
			: base(reader, builder)
		{
		}

		public override void Read()
		{
			//Advance to the first value in the section
			this._reader.ReadNext();

			//Loop until the section ends
			while (this._reader.LastValueAsString != DxfFileToken.EndSection)
			{
				try
				{
					if (this._reader.LastValueAsString == DxfFileToken.Block)
						this.readBlock();
					else
						throw new DxfException($"Unexpected token at the BLOCKS table: {this._reader.LastValueAsString}", this._reader.Position);
				}
				catch (Exception ex)
				{
					if (!this._builder.Configuration.Failsafe)
						throw;

					this._builder.Notify($"Error while reading a block at line {this._reader.Position}", NotificationType.Error, ex);

					while (!(this._reader.LastDxfCode == DxfCode.Start && this._reader.LastValueAsString == DxfFileToken.EndSection)
							&& !(this._reader.LastDxfCode == DxfCode.Start && this._reader.LastValueAsString == DxfFileToken.Block))
					{
						this._reader.ReadNext();
					}
				}
			}
		}

		private void readBlock()
		{
			Debug.Assert(this._reader.LastValueAsString == DxfFileToken.Block);

			//Read the table name
			this._reader.ReadNext();

			Block blckEntity = null;
			CadEntityTemplate template = null;

			this.readCommonObjectData(out string name, out ulong handle, out ulong? ownerHandle, out ulong? xdictHandle, out List<ulong> reactors);

			if (this._builder.TryGetCadObject(ownerHandle, out BlockRecord record))
			{
				blckEntity = record.BlockEntity;
			}
			else
			{
				blckEntity = new Block();
				this._builder.Notify($"Block Record {ownerHandle} not found for Block {handle} | {name}", NotificationType.Warning);
			}

			//Assign the handle to the entity
			blckEntity.Handle = handle;
			template = new CadEntityTemplate(blckEntity);
			template.OwnerHandle = ownerHandle;
			template.XDictHandle = xdictHandle;
			template.ReactorsHandles = reactors;

			Debug.Assert(this._reader.LastValueAsString == DxfSubclassMarker.Entity);

			this.readMapped<Entity>(blckEntity, template);

			Debug.Assert(this._reader.LastValueAsString == DxfSubclassMarker.BlockBegin);

			//this.readMapped<Block>(blckEntity, template);
			this.readBlockBegin(blckEntity);

			if (record == null && this._builder.DocumentToBuild.BlockRecords.TryGetValue(blckEntity.Name, out record))
			{
				record.BlockEntity = blckEntity;
				this._builder.Notify($"Block record find by name {blckEntity.Name}", NotificationType.None);
			}
			else if (record == null)
			{
				throw new DxfException($"Could not find the block record for {blckEntity.Name} and handle {blckEntity.Handle}");
			}

			while (this._reader.LastValueAsString != DxfFileToken.EndBlock)
			{
				CadEntityTemplate entityTemplate = null;

				try
				{
					entityTemplate = this.readEntity();
				}
				catch (Exception ex)
				{
					if (!this._builder.Configuration.Failsafe)
						throw;

					this._builder.Notify($"Error while reading a block with name {record.Name} at line {this._reader.Position}", NotificationType.Error, ex);

					while (this._reader.LastDxfCode != DxfCode.Start)
						this._reader.ReadNext();
				}

				if (entityTemplate == null)
					continue;

				//Add the object and the template to the builder
				this._builder.AddTemplate(entityTemplate);
				record.Entities.Add(entityTemplate.CadObject);
			}

			this.readBlockEnd(record.BlockEnd);
			this._builder.AddTemplate(template);
		}

		private void readBlockBegin(Block blckEntity)
		{
			this._reader.ReadNext();

			while (this._reader.LastDxfCode != DxfCode.Start
				&& this._reader.LastDxfCode != DxfCode.Subclass)
			{
				switch (this._reader.LastCode)
				{
					case 1:
						blckEntity.XrefPath = this._reader.LastValueAsString;
						break;
					case 4:
						blckEntity.Comments = this._reader.LastValueAsString;
						break;
					case 2:
					case 3:
						if (blckEntity.Owner == null && this._builder.TryGetTableEntry(this._reader.LastValueAsString, out BlockRecord record))
						{
							record.BlockEntity = blckEntity;
							this._builder.Notify($"Block record find by name {blckEntity.Name}", NotificationType.None);
						}
						else if (blckEntity.Owner == null)
						{
							throw new DxfException($"Could not find the block record for {blckEntity.Name} and handle {blckEntity.Handle}");
						}
						break;
					case 70:
						blckEntity.Flags = (BlockTypeFlags)this._reader.LastValueAsShort;
						break;
					case 10:
						blckEntity.BasePoint = new XYZ(this._reader.LastValueAsDouble, blckEntity.BasePoint.Y, blckEntity.BasePoint.Z);
						break;
					case 20:
						blckEntity.BasePoint = new XYZ(blckEntity.BasePoint.X, this._reader.LastValueAsDouble, blckEntity.BasePoint.Z);
						break;
					case 30:
						blckEntity.BasePoint = new XYZ(blckEntity.BasePoint.X, blckEntity.BasePoint.Y, this._reader.LastValueAsDouble);
						break;
					default:
						this._builder.Notify($"Unhandeled dxf code : {this._reader.LastCode} with value : {this._reader.LastValue} for subclass {DxfSubclassMarker.BlockBegin}");
						break;
				}

				this._reader.ReadNext();
			}
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
