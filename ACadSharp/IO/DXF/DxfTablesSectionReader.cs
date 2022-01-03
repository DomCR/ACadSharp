using ACadSharp.Blocks;
using ACadSharp.Exceptions;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.IO.DXF
{
	internal class DxfTablesSectionReader : DxfSectionReaderBase
	{
		public DxfTablesSectionReader(IDxfStreamReader reader, DxfDocumentBuilder builder, NotificationEventHandler notification = null)
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

				if (this._reader.LastValueAsString == DxfFileToken.TableEntry)
					this.readTable();
				else
					throw new DxfException($"Unexpected token at the begining of a table: {this._reader.LastValueAsString}", this._reader.Line);


				if (this._reader.LastValueAsString == DxfFileToken.EndTable)
					this._reader.ReadNext();
				else
					throw new DxfException($"Unexpected token at the end of a table: {this._reader.LastValueAsString}", this._reader.Line);
			}
		}

		private void readTable()
		{
			Debug.Assert(this._reader.LastValueAsString == DxfFileToken.TableEntry);

			//Read the table name
			this._reader.ReadNext();

			string name = null;
			ulong? handle = null;
			ulong? ownerHandle = null;
			int nentries = 0;

			//Loop until the common data end
			while (this._reader.LastDxfCode != DxfCode.Start)
			{
				switch (this._reader.LastCode)
				{
					//Table name
					case 2:
						name = this._reader.LastValueAsString;
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
					//Soft - pointer ID / handle to owner BLOCK_RECORD object
					case 330:
						ownerHandle = this._reader.LastValueAsHandle;
						break;
					//Subclass marker(AcDbSymbolTable)
					case 100:
						Debug.Assert(this._reader.LastValueAsString == DxfSubclassMarker.Table
							|| this._reader.LastValueAsString == DxfSubclassMarker.DimensionStyleTable);

						break;
					case 71:
					//Number of entries for dimension style table
					case 340:
						//Dimension table has the handles of the styles at the begining
						break;
					//Maximum number of entries in table
					case 70:
						nentries = this._reader.LastValueAsInt;
						break;
					default:
						this._notification?.Invoke(null, new NotificationEventArgs($"Unhandeled dxf code {this._reader.LastCode} at line {this._reader.Line}."));
						break;
				}

				this._reader.ReadNext();
			}

			DwgTemplate template = null;

			switch (name)
			{
				case DxfFileToken.TableAppId:
					template = new DwgTableTemplate<AppId>(this._builder.DocumentToBuild.AppIds);
					this.readEntries(name, handle.Value, (DwgTableTemplate<AppId>)template);
					break;
				case DxfFileToken.TableBlockRecord:
					template = new DwgBlockCtrlObjectTemplate(this._builder.DocumentToBuild.BlockRecords);
					this.readEntries(name, handle.Value, (DwgBlockCtrlObjectTemplate)template);
					break;
				case DxfFileToken.TableVport:
					template = new DwgTableTemplate<VPort>(this._builder.DocumentToBuild.VPorts);
					this.readEntries(name, handle.Value, (DwgTableTemplate<VPort>)template);
					break;
				case DxfFileToken.TableLinetype:
					template = new DwgTableTemplate<LineType>(this._builder.DocumentToBuild.LineTypes);
					this.readEntries(name, handle.Value, (DwgTableTemplate<LineType>)template);
					break;
				case DxfFileToken.TableLayer:
					template = new DwgTableTemplate<Layer>(this._builder.DocumentToBuild.Layers);
					this.readEntries(name, handle.Value, (DwgTableTemplate<Layer>)template);
					break;
				case DxfFileToken.TableStyle:
					template = new DwgTableTemplate<TextStyle>(this._builder.DocumentToBuild.TextStyles);
					this.readEntries(name, handle.Value, (DwgTableTemplate<TextStyle>)template);
					break;
				case DxfFileToken.TableView:
					template = new DwgTableTemplate<View>(this._builder.DocumentToBuild.Views);
					this.readEntries(name, handle.Value, (DwgTableTemplate<View>)template);
					break;
				case DxfFileToken.TableUcs:
					template = new DwgTableTemplate<UCS>(this._builder.DocumentToBuild.UCSs);
					this.readEntries(name, handle.Value, (DwgTableTemplate<UCS>)template);
					break;
				case DxfFileToken.TableDimstyle:
					template = new DwgTableTemplate<DimensionStyle>(this._builder.DocumentToBuild.DimensionStyles);
					this.readEntries(name, handle.Value, (DwgTableTemplate<DimensionStyle>)template);
					break;
				default:
					throw new DxfException($"Unknown table name {name}");
			}

			//Add the object and the template to the builder
			this._builder.Templates[template.CadObject.Handle] = template;
		}

		private void readEntries<T>(string tableName, ulong tableHandle, DwgTableTemplate<T> tableTemplate)
			where T : TableEntry
		{
			tableTemplate.CadObject.Handle = tableHandle;

			//Read all the entries until the end of the table
			while (this._reader.LastValueAsString != DxfFileToken.EndTable)
			{
				ulong handle = 0;

				//Read the common entry data
				while (this._reader.LastDxfCode != DxfCode.Subclass)
				{
					switch (this._reader.LastCode)
					{
						//Entity type (table name)
						case 0:
							Debug.Assert(this._reader.LastValueAsString == tableName);
							break;
						//Handle (all except DIMSTYLE)
						case 5:
						//Handle (DIMSTYLE table only)
						case 105:
							handle = this._reader.LastValueAsHandle;
							tableTemplate.EntryHandles.Add(this._reader.LastValueAsHandle);
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
						//Soft - pointer ID / handle to owner object
						case 330:
							if (tableName != "DIMSTYLE")
								Debug.Assert(this._reader.LastValueAsHandle == tableTemplate.CadObject.Handle);
							break;
						default:
							Debug.Fail($"Unhandeled dxf code {this._reader.LastCode} at line {this._reader.Line}.");
							break;
					}

					this._reader.ReadNext();
				}

				Debug.Assert(this._reader.LastValueAsString == DxfSubclassMarker.TableRecord);
				this._reader.ReadNext();

				bool assignHandle = true;
				DwgTemplate template = null;

				//Get the entry
				switch (tableName)
				{
					case DxfFileToken.TableAppId:
						template = this.readAppid();
						break;
					case DxfFileToken.TableBlockRecord:
						Block block = new Block();
						template = new DwgBlockTemplate(block);
						this.readRaw(template, DxfSubclassMarker.BlockRecord, this.readBlockRecord, readUntilStart);
						this._builder.BlockRecords[(template.CadObject as Block).Name] = template as DwgBlockTemplate;

						//Assign the handle to the record
						block.Record.Handle = handle;
						assignHandle = false;
						break;
					case DxfFileToken.TableDimstyle:
						template = new DwgDimensionStyleTemplate(new DimensionStyle());
						this.readRaw(template, DxfSubclassMarker.DimensionStyle, this.readDimStyle, readUntilStart);
						break;
					case DxfFileToken.TableLayer:
						Layer layer = new Layer();
						template = new DwgLayerTemplate(layer);
						this.readRaw(template, DxfSubclassMarker.Layer, this.readLayer, readUntilStart);
						break;
					case DxfFileToken.TableLinetype:
						template = new DwgTableEntryTemplate<LineType>(new LineType());
						this.readRaw(template, DxfSubclassMarker.Linetype, this.readLineType, readUntilStart);
						_builder.NotificationHandler?.Invoke(template.CadObject, new NotificationEventArgs($"Line type not fully read"));
						break;
					case DxfFileToken.TableStyle:
						TextStyle style = TextStyle.Default;
						template = new DwgTableEntryTemplate<TextStyle>(style);
						this.readRaw(template, DxfSubclassMarker.TextStyle, this.readTextStyle, readUntilStart);
						break;
					case DxfFileToken.TableUcs:
						template = new DwgTemplate<UCS>(new UCS());
						this.readRaw(template, DxfSubclassMarker.Ucs, this.readUcs, readUntilStart);
						break;
					case DxfFileToken.TableView:
						template = new DwgTableEntryTemplate<View>(new View());
						this.readRaw(template, DxfSubclassMarker.View, this.readView, readUntilStart);
						_builder.NotificationHandler?.Invoke(template.CadObject, new NotificationEventArgs($"View not implemented"));
						break;
					case DxfFileToken.TableVport:
						template = new DwgVPortTemplate(new VPort());
						this.readRaw(template, DxfSubclassMarker.VPort, this.readVPort, readUntilStart);
						break;
					default:
						Debug.Fail($"Unhandeled table {tableName}.");
						break;
				}

				if (assignHandle)
				{
					//Setup the common fields
					template.CadObject.Handle = handle;
				}

				//Add the object and the template to the builder
				this._builder.Templates[template.CadObject.Handle] = template;
			}
		}

		private DwgTemplate readAppid()
		{
			AppId appId = new AppId();
			DwgTemplate template = new DwgTemplate<AppId>(appId);

			Debug.Assert(this._reader.LastDxfCode == DxfCode.Subclass);
			Debug.Assert(this._reader.LastValueAsString == DxfSubclassMarker.ApplicationId);

			//Jump the SubclassMarker
			this._reader.ReadNext();

			while (this._reader.LastDxfCode != DxfCode.Start)
			{
				switch (this._reader.LastCode)
				{
					case 2:
						appId.Name = this._reader.LastValueAsString;
						break;
					case 70:
						appId.Flags = (StandardFlags)this._reader.LastValueAsShort;
						break;
					default:
						Debug.Fail($"Unhandeled dxf code {this._reader.LastCode} at line {this._reader.Line}.");
						break;
				}

				this._reader.ReadNext();
			}

			return template;
		}

		private bool readBlockRecord(DwgTemplate template)
		{
			DwgBlockTemplate blockTemplate = template as DwgBlockTemplate;

			switch (this._reader.LastCode)
			{
				//Block name
				case 2:
					blockTemplate.CadObject.Name = this._reader.LastValueAsString;
					return true;
				//Hard-pointer ID/handle to associated LAYOUT object
				case 340:
					blockTemplate.LayoutHandle = this._reader.LastValueAsHandle;
					return true;
				//Block insertion units
				case 70:
				case 1070:
					blockTemplate.CadObject.Record.Units = (Types.Units.UnitsType)this._reader.LastValueAsShort;
					return true;
				//Block explodability
				case 280:
					blockTemplate.CadObject.Record.IsExplodable = this._reader.LastValueAsBool;
					return true;
				//Block scalability
				case 281:
					blockTemplate.CadObject.Record.CanScale = this._reader.LastValueAsBool;
					return true;
				case 310:
					blockTemplate.CadObject.Record.Preview = this._reader.LastValueAsBinaryChunk;
					return true;
				default:
					break;
			}

			return false;
		}

		private bool readLineType(DwgTemplate template)
		{
			DwgTableEntryTemplate<LineType> ltypeTemplate = template as DwgTableEntryTemplate<LineType>;

			switch (this._reader.LastCode)
			{
				//Pointer to STYLE object (one per element if code 74 > 0)
				case 340:
				default:
					break;
			}

			return false;
		}

		private bool readView(DwgTemplate template)
		{
			switch (this._reader.LastCode)
			{
				//Pointer to STYLE object (one per element if code 74 > 0)
				case 340:
				default:
					break;
			}

			return false;
		}

		public bool readTextStyle(DwgTemplate template)
		{
			DwgTableEntryTemplate<TextStyle> styleTemplate = template as DwgTableEntryTemplate<TextStyle>;

			switch (this._reader.LastCode)
			{
				default:
					break;
			}

			return false;
		}

		public bool readUcs(DwgTemplate template)
		{
			DwgTableEntryTemplate<TextStyle> styleTemplate = template as DwgTableEntryTemplate<TextStyle>;

			switch (this._reader.LastCode)
			{
				default:
					break;
			}

			return false;
		}

		public bool readDimStyle(DwgTemplate template)
		{
			DwgDimensionStyleTemplate dimStyleTemplate = template as DwgDimensionStyleTemplate;

			switch (this._reader.LastCode)
			{
				//DIMTXSTY
				case 340:
					dimStyleTemplate.DIMTXSTY = this._reader.LastValueAsHandle;
					return true;
				//DIMLDRBLK
				case 341:
					dimStyleTemplate.DIMLDRBLK = this._reader.LastValueAsHandle;
					return true;
				//DIMBLK
				case 342:
					dimStyleTemplate.DIMBLK = this._reader.LastValueAsHandle;
					return true;
				//DIMBLK1
				case 343:
					dimStyleTemplate.DIMBLK1 = this._reader.LastValueAsHandle;
					return true;
				default:
					break;
			}

			return false;
		}

		public bool readLayer(DwgTemplate template)
		{
			DwgLayerTemplate layerTemplate = template as DwgLayerTemplate;

			switch (this._reader.LastCode)
			{
				//Hard-pointer ID/handle to Material object
				case 347:
					break;
				//333	Soft - pointer ID / handle to shade plot object(optional)
				case 380:
					break;
				//348	Hard - pointer ID / handle to visual style object(optional)
				case 348:
					break;
				default:
					break;
			}

			return false;
		}

		public bool readVPort(DwgTemplate template)
		{
			DwgVPortTemplate vportTemplate = template as DwgVPortTemplate;

			switch (this._reader.LastCode)
			{
				//Soft - pointer ID / handle to background object(optional)
				case 332:
					vportTemplate.BackgroundHandle = this._reader.LastValueAsHandle;
					return true;
				//Soft - pointer ID / handle to shade plot object(optional)
				case 333:
					_builder.NotificationHandler?.Invoke(
						template.CadObject,
						new NotificationEventArgs($"Code not implemented for type\n" +
						$"\tcode : {this._reader.LastCode}\n" +
						$"\ttype : {template.CadObject.ObjectType}"));
					return true;
				//Hard - pointer ID / handle to visual style object(optional)
				case 348:
					vportTemplate.StyelHandle = this._reader.LastValueAsHandle;
					return true;
				default:
					break;
			}

			return false;
		}
	}
}
