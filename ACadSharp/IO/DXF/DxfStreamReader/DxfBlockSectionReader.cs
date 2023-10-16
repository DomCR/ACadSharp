using ACadSharp.Blocks;
using ACadSharp.Exceptions;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using System;
using System.Diagnostics;

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
			while (this._reader.ValueAsString != DxfFileToken.EndSection)
			{
				try
				{
					if (this._reader.ValueAsString == DxfFileToken.Block)
						this.readBlock();
					else
						throw new DxfException($"Unexpected token at the BLOCKS table: {this._reader.ValueAsString}", this._reader.Position);
				}
				catch (Exception ex)
				{
					if (!this._builder.Configuration.Failsafe)
						throw;

					this._builder.Notify($"Error while reading a block at line {this._reader.Position}", NotificationType.Error, ex);

					while (!(this._reader.DxfCode == DxfCode.Start && this._reader.ValueAsString == DxfFileToken.EndSection)
							&& !(this._reader.DxfCode == DxfCode.Start && this._reader.ValueAsString == DxfFileToken.Block))
					{
						this._reader.ReadNext();
					}
				}
			}
		}

		private void readBlock()
		{
			Debug.Assert(this._reader.ValueAsString == DxfFileToken.Block);

			//Read the table name
			this._reader.ReadNext();

			DxfMap map = DxfMap.Create<Block>();

			Block blckEntity = new Block();
			CadEntityTemplate template = new CadEntityTemplate(blckEntity);

			string name = null;
			BlockRecord record = null;

			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 2:
					case 3:
						name = this._reader.ValueAsString;
						if (record == null && this._builder.TryGetTableEntry(name, out record))
						{
							record.BlockEntity = blckEntity;
						}
						else if (record == null)
						{
							this._builder.Notify($"Block record [{name}] not found at line {this._reader.Position}", NotificationType.Warning);
						}
						break;
					case 330:
						if (record == null && this._builder.TryGetCadObject(this._reader.ValueAsHandle, out record))
						{
							record.BlockEntity = blckEntity;
						}
						else if (record == null)
						{
							this._builder.Notify($"Block record with handle [{this._reader.ValueAsString}] not found at line {this._reader.Position}", NotificationType.Warning);
						}
						break;
					default:
						if (!this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.BlockBegin]))
						{
							this.readCommonEntityCodes(template, out bool isExtendedData, map);
							if (isExtendedData)
								continue;
						}
						break;
				}

				this._reader.ReadNext();
			}

			if (record == null)
			{
				//record = new BlockRecord(name);
				//record.BlockEntity = blckEntity;

				//this._builder.DocumentToBuild.BlockRecords.Add(record);
				throw new DxfException($"Could not find the block record for {name} and handle {blckEntity.Handle}");
			}

			while (this._reader.ValueAsString != DxfFileToken.EndBlock)
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

					while (this._reader.DxfCode != DxfCode.Start)
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

		private void readBlockEnd(BlockEnd block)
		{
			DxfMap map = DxfMap.Create<BlockEnd>();
			CadEntityTemplate template = new CadEntityTemplate(block);

			if (this._reader.DxfCode == DxfCode.Start)
			{
				this._reader.ReadNext();
			}

			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					default:
						if (!this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.BlockEnd]))
						{
							this.readCommonEntityCodes(template, out bool isExtendedData, map);
							if (isExtendedData)
								continue;
						}
						break;
				}

				this._reader.ReadNext();
			}

			this._builder.AddTemplate(template);
		}
	}
}
