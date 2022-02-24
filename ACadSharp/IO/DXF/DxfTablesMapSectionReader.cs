using ACadSharp.Exceptions;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using System;
using System.Diagnostics;

namespace ACadSharp.IO.DXF
{
	internal class DxfTablesMapSectionReader : DxfSectionReaderBase
	{
		public DxfTablesMapSectionReader(IDxfStreamReader reader, DxfDocumentBuilder builder, NotificationEventHandler notification = null)
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

			int nentries = 0;

			this.readCommonObjectData(out string name, out ulong handle, out ulong? ownerHandle);

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
					default:
						this._notification?.Invoke(null, new NotificationEventArgs($"Unhandeled dxf code {this._reader.LastCode} at line {this._reader.Line}."));
						Debug.Fail($"Unhandeled dxf code {this._reader.LastCode} at line {this._reader.Line}.");
						break;
				}

				this._reader.ReadNext();
			}

			DwgTemplate template = null;

			switch (name)
			{
				case DxfFileToken.TableAppId:
					template = new DwgTableTemplate<AppId>(this._builder.DocumentToBuild.AppIds);
					this.readEntries(name, handle, (DwgTableTemplate<AppId>)template);
					break;
				case DxfFileToken.TableBlockRecord:
					template = new DwgBlockCtrlObjectTemplate(this._builder.DocumentToBuild.BlockRecords);
					this.readEntries(name, handle, (DwgBlockCtrlObjectTemplate)template);
					break;
				case DxfFileToken.TableVport:
					template = new DwgTableTemplate<VPort>(this._builder.DocumentToBuild.VPorts);
					this.readEntries(name, handle, (DwgTableTemplate<VPort>)template);
					break;
				case DxfFileToken.TableLinetype:
					template = new DwgTableTemplate<LineType>(this._builder.DocumentToBuild.LineTypes);
					this.readEntries(name, handle, (DwgTableTemplate<LineType>)template);
					break;
				case DxfFileToken.TableLayer:
					template = new DwgTableTemplate<Layer>(this._builder.DocumentToBuild.Layers);
					this.readEntries(name, handle, (DwgTableTemplate<Layer>)template);
					break;
				case DxfFileToken.TableStyle:
					template = new DwgTableTemplate<TextStyle>(this._builder.DocumentToBuild.TextStyles);
					this.readEntries(name, handle, (DwgTableTemplate<TextStyle>)template);
					break;
				case DxfFileToken.TableView:
					template = new DwgTableTemplate<View>(this._builder.DocumentToBuild.Views);
					this.readEntries(name, handle, (DwgTableTemplate<View>)template);
					break;
				case DxfFileToken.TableUcs:
					template = new DwgTableTemplate<UCS>(this._builder.DocumentToBuild.UCSs);
					this.readEntries(name, handle, (DwgTableTemplate<UCS>)template);
					break;
				case DxfFileToken.TableDimstyle:
					template = new DwgTableTemplate<DimensionStyle>(this._builder.DocumentToBuild.DimensionStyles);
					this.readEntries(name, handle, (DwgTableTemplate<DimensionStyle>)template);
					break;
				default:
					throw new DxfException($"Unknown table name {name}");
			}

			template.CadObject.Handle = handle;

			Debug.Assert(ownerHandle == null || ownerHandle.Value == 0);

			//Add the object and the template to the builder
			this._builder.Templates[template.CadObject.Handle] = template;
		}


		private void readEntries<T>(string tableName, ulong tableHandle, DwgTableTemplate<T> tableTemplate)
			where T : TableEntry
		{
			//Read all the entries until the end of the table
			while (this._reader.LastValueAsString != DxfFileToken.EndTable)
			{
				this.readCommonObjectData(out _, out ulong handle, out ulong? ownerHandle);

				Debug.Assert(this._reader.LastValueAsString == DxfSubclassMarker.TableRecord);

				this._reader.ReadNext();

				bool assignHandle = true;
				DwgTemplate template = null;

				//Get the entry
				switch (tableName)
				{
					case DxfFileToken.TableAppId:
						AppId appid = new AppId();
						template = new DwgTableEntryTemplate<AppId>(appid);
						this.readRaw(appid, template);
						break;
					case DxfFileToken.TableBlockRecord:
					case DxfFileToken.TableDimstyle:
					case DxfFileToken.TableLayer:
						Debug.Fail($"Unhandeled table {tableName}.");
						break;
					case DxfFileToken.TableLinetype:
						LineType ltype = new LineType();
						template = new DwgTableEntryTemplate<LineType>(ltype);
						this.readRaw(ltype, template);
						break;
					case DxfFileToken.TableStyle:
					case DxfFileToken.TableUcs:
					case DxfFileToken.TableView:
						Debug.Fail($"Unhandeled table {tableName}.");
						break;
					case DxfFileToken.TableVport:
						VPort vport = new VPort();
						template = new DwgVPortTemplate(vport);
						this.readRaw(vport, template);
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
	}
}
