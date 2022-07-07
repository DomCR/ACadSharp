using ACadSharp.Exceptions;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ACadSharp.IO.DXF
{
	internal class DxfTablesSectionReader : DxfSectionReaderBase
	{
		public DxfTablesSectionReader(IDxfStreamReader reader, DxfDocumentBuilder builder)
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

				if (this._reader.LastValueAsString == DxfFileToken.TableEntry)
					this.readTable();
				else
					throw new DxfException($"Unexpected token at the begining of a table: {this._reader.LastValueAsString}", this._reader.Position);


				if (this._reader.LastValueAsString == DxfFileToken.EndTable)
					this._reader.ReadNext();
				else
					throw new DxfException($"Unexpected token at the end of a table: {this._reader.LastValueAsString}", this._reader.Position);
			}
		}

		private void readTable()
		{
			Debug.Assert(this._reader.LastValueAsString == DxfFileToken.TableEntry);

			//Read the table name
			this._reader.ReadNext();

			int nentries = 0;
			CadTemplate template = null;
			Dictionary<string, ExtendedData> edata = new Dictionary<string, ExtendedData>();

			this.readCommonObjectData(out string name, out ulong handle, out ulong? ownerHandle, out ulong? xdictHandle, out List<ulong> reactors);

			Debug.Assert(this._reader.LastValueAsString == DxfSubclassMarker.Table);

			this._reader.ReadNext();

			while (this._reader.LastDxfCode != DxfCode.Start)
			{
				switch (this._reader.LastCode)
				{
					//Maximum number of entries in table
					case 70:
						nentries = this._reader.LastValueAsInt;
						break;
					case 100 when this._reader.LastValueAsString == DxfSubclassMarker.DimensionStyleTable:
						while (this._reader.LastDxfCode != DxfCode.Start)
						{
							//Dimstyle has the code 71 for the count of entries
							//Also has 340 codes for each entry with the handles
							this._reader.ReadNext();
						}
						break;
					case 1001:
						this.readExtendedData(edata);
						break;
					default:
						this._builder.Notify(new NotificationEventArgs($"Unhandeled dxf code {this._reader.LastCode} at line {this._reader.Position}."));
						break;
				}

				if (this._reader.LastDxfCode == DxfCode.Start)
					break;

				this._reader.ReadNext();
			}

			switch (name)
			{
				case DxfFileToken.TableAppId:
					template = new CadTableTemplate<AppId>(new AppIdsTable());
					this.readEntries((CadTableTemplate<AppId>)template);
					template.CadObject.Handle = handle;
					this._builder.DocumentToBuild.RegisterCollection((AppIdsTable)template.CadObject);
					break;
				case DxfFileToken.TableBlockRecord:
					template = new CadBlockCtrlObjectTemplate(new BlockRecordsTable());
					this.readEntries((CadBlockCtrlObjectTemplate)template);
					template.CadObject.Handle = handle;
					this._builder.DocumentToBuild.RegisterCollection((BlockRecordsTable)template.CadObject);
					break;
				case DxfFileToken.TableVport:
					template = new CadTableTemplate<VPort>(new VPortsTable());
					this.readEntries((CadTableTemplate<VPort>)template);
					template.CadObject.Handle = handle;
					this._builder.DocumentToBuild.RegisterCollection((VPortsTable)template.CadObject);
					break;
				case DxfFileToken.TableLinetype:
					template = new CadTableTemplate<LineType>(new LineTypesTable());
					this.readEntries((CadTableTemplate<LineType>)template);
					template.CadObject.Handle = handle;
					this._builder.DocumentToBuild.RegisterCollection((LineTypesTable)template.CadObject);
					break;
				case DxfFileToken.TableLayer:
					template = new CadTableTemplate<Layer>(new LayersTable());
					this.readEntries((CadTableTemplate<Layer>)template);
					template.CadObject.Handle = handle;
					this._builder.DocumentToBuild.RegisterCollection((LayersTable)template.CadObject);
					break;
				case DxfFileToken.TableStyle:
					template = new CadTableTemplate<TextStyle>(new TextStylesTable());
					this.readEntries((CadTableTemplate<TextStyle>)template);
					template.CadObject.Handle = handle;
					this._builder.DocumentToBuild.RegisterCollection((TextStylesTable)template.CadObject);
					break;
				case DxfFileToken.TableView:
					template = new CadTableTemplate<View>(new ViewsTable());
					this.readEntries((CadTableTemplate<View>)template);
					template.CadObject.Handle = handle;
					this._builder.DocumentToBuild.RegisterCollection((ViewsTable)template.CadObject);
					break;
				case DxfFileToken.TableUcs:
					template = new CadTableTemplate<UCS>(new UCSTable());
					this.readEntries((CadTableTemplate<UCS>)template);
					template.CadObject.Handle = handle;
					this._builder.DocumentToBuild.RegisterCollection((UCSTable)template.CadObject);
					break;
				case DxfFileToken.TableDimstyle:
					template = new CadTableTemplate<DimensionStyle>(new DimensionStylesTable());
					this.readEntries((CadTableTemplate<DimensionStyle>)template);
					template.CadObject.Handle = handle;
					this._builder.DocumentToBuild.RegisterCollection((DimensionStylesTable)template.CadObject);
					break;
				default:
					throw new DxfException($"Unknown table name {name}");
			}

			Debug.Assert(ownerHandle == null || ownerHandle.Value == 0);

			template.OwnerHandle = ownerHandle;
			template.XDictHandle = xdictHandle;
			template.ReactorsHandles = reactors;
			template.EDataTemplateByAppName = edata;

			//Add the object and the template to the builder
			this._builder.AddTableTemplate((ICadTableTemplate)template);
		}

		private void readEntries<T>(CadTableTemplate<T> tableTemplate)
			where T : TableEntry
		{
			//Read all the entries until the end of the table
			while (this._reader.LastValueAsString != DxfFileToken.EndTable)
			{
				this.readCommonObjectData(out string name, out ulong handle, out ulong? ownerHandle, out ulong? xdictHandle, out List<ulong> reactors);

				Debug.Assert(this._reader.LastValueAsString == DxfSubclassMarker.TableRecord, $"Expected: {DxfSubclassMarker.TableRecord} but was {this._reader.LastValueAsString}");

				this._reader.ReadNext();

				CadTemplate template = null;

				//Get the entry
				switch (name)
				{
					case DxfFileToken.TableAppId:
						AppId appid = new AppId();
						template = new CadTableEntryTemplate<AppId>(appid);
						this.readMapped<AppId>(appid, template);
						break;
					case DxfFileToken.TableBlockRecord:
						BlockRecord record = new BlockRecord();
						template = new CadBlockRecordTemplate(record);
						this.readMapped<BlockRecord>(record, template);
						break;
					case DxfFileToken.TableDimstyle:
						DimensionStyle dimStyle = new DimensionStyle();
						template = new CadDimensionStyleTemplate(dimStyle);
						this.readMapped<DimensionStyle>(dimStyle, template);
						break;
					case DxfFileToken.TableLayer:
						Layer layer = new Layer();
						template = new CadLayerTemplate(layer);
						this.readMapped<Layer>(layer, template);
						break;
					case DxfFileToken.TableLinetype:
						LineType ltype = new LineType();
						template = new CadLineTypeTemplate(ltype);
						this.readMapped<LineType>(ltype, template);
						break;
					case DxfFileToken.TableStyle:
						TextStyle style = new TextStyle();
						template = new CadTableEntryTemplate<TextStyle>(style);
						this.readMapped<TextStyle>(style, template);
						break;
					case DxfFileToken.TableUcs:
						UCS ucs = new UCS();
						template = new CadUcsTemplate(ucs);
						this.readMapped<UCS>(ucs, template);
						break;
					case DxfFileToken.TableView:
						View view = new View();
						template = new CadViewTemplate(view);
						this.readMapped<View>(view, template);
						break;
					case DxfFileToken.TableVport:
						VPort vport = new VPort();
						template = new CadVPortTemplate(vport);
						this.readMapped<VPort>(vport, template);
						break;
					default:
						Debug.Fail($"Unhandeled table {name}.");
						break;
				}

				//Setup the common fields
				template.CadObject.Handle = handle;
				template.OwnerHandle = ownerHandle;
				template.XDictHandle = xdictHandle;
				template.ReactorsHandles = reactors;

				tableTemplate.EntryHandles.Add(template.CadObject.Handle);

				//Add the object and the template to the builder
				this._builder.AddTemplate(template);
			}
		}
	}
}
