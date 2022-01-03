using ACadSharp.Blocks;
using ACadSharp.Exceptions;
using ACadSharp.IO.Templates;
using CSMath;
using System;
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
			while (!this._reader.EndSectionFound)
			{
				if (this._reader.LastValueAsString == DxfFileToken.Block)
					this.readBlock();
				else
					throw new DxfException($"Unexpected token at the begining of a block: {this._reader.LastValueAsString}", this._reader.Line);
			}
		}

		private void readBlock()
		{
			Debug.Assert(this._reader.LastValueAsString == DxfFileToken.Block);

			this._reader.ReadNext();

			//read the block begin entity
			DwgBlockTemplate template = this.readBlockBegin();

			//Loop until the block end
			while (this._reader.LastValueAsString != DxfFileToken.EndBlock)
			{
				DwgTemplate entityTemplate = this.readEntity();

				Debug.Assert(entityTemplate.OwnerHandle == template.CadObject.Record.Handle);

				//Add the handle to the template 
				template.OwnedObjectsHandlers.Add(entityTemplate.CadObject.Handle);

				_builder.Templates.Add(entityTemplate.CadObject.Handle, entityTemplate);
			}

			this._reader.ReadNext();

			//Read the endlbock data
			while (this._reader.LastDxfCode != DxfCode.Start)
			{
				//TODO: implement the dxf endblock reader

				this._reader.ReadNext();
			}
		}

		private DwgBlockTemplate readBlockBegin()
		{
			DwgBlockTemplate template = null;

			ulong? handle = 0;
			ulong? ownerHandle = 0;
			string name = null;
			string description = null;
			string layerName = null;
			BlockTypeFlags flags = BlockTypeFlags.None;
			XYZ basePoint = new XYZ();

			//Loop until the data end
			while (this._reader.LastDxfCode != DxfCode.Start)
			{
				switch (this._reader.LastCode)
				{
					//Absent or zero indicates entity is in model space. 1 indicates entity is in paper space (optional).
					case 67:
						break;
					//Handle
					case 5:
						handle = this._reader.LastValueAsHandle;
						break;
					//Start of application - defined group
					case 102:
						//TODO: read dictionary groups for entities
						do
						{
							this._reader.ReadNext();
						}
						while (this._reader.LastDxfCode != DxfCode.ControlString);
						break;
					//Soft-pointer ID/handle to owner object
					case 330:
						ownerHandle = this._reader.LastValueAsHandle;
						break;
					//Subclass marker (AcDbBlockBegin)
					case 100:
						Debug.Assert(DxfSubclassMarker.BlockBegin == this._reader.LastValueAsString
							|| DxfSubclassMarker.Entity == this._reader.LastValueAsString);
						break;
					//Block name
					case 2:
					case 3:
						if (!string.IsNullOrEmpty(name))
							break;
						else if (string.IsNullOrEmpty(this._reader.LastValueAsString))
							break;
						name = this._reader.LastValueAsString;
						break;
					//Block-type flags (bit-coded values, may be combined)
					case 70:
						flags = (BlockTypeFlags)this._reader.LastValueAsShort;
						break;
					//Base point
					case 10:
						basePoint = new XYZ(this._reader.LastValueAsDouble, basePoint.Y, basePoint.Z);
						break;
					case 20:
						basePoint = new XYZ(basePoint.X, this._reader.LastValueAsDouble, basePoint.Z);
						break;
					case 30:
						basePoint = new XYZ(basePoint.X, basePoint.Y, this._reader.LastValueAsDouble);
						break;
					//Block description (optional)
					case 1:
						description = this._reader.LastValueAsString;
						break;
					//Block description (optional)
					case 4:
						description = this._reader.LastValueAsString;
						break;
					//Layer name
					case 8:
						layerName = this._reader.LastValueAsString;
						break;
					default:
						Debug.Fail($"Unhandeled dxf code {this._reader.LastCode} at line {this._reader.Line}.");
						break;
				}

				this._reader.ReadNext();
			}

			template = this._builder.BlockRecords[name];

			template.CadObject.Handle = handle.Value;
			template.OwnerHandle = ownerHandle.Value;
			template.CadObject.Name = name;
			template.CadObject.Comments = description;
			template.CadObject.Flags = flags;
			template.CadObject.BasePoint = basePoint;
			template.LayerName = layerName;

			return template;
		}
	}
}
