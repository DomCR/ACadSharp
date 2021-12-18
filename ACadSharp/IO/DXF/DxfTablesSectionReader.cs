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
	internal class DxfTablesSectionReader
	{
		private readonly IDxfStreamReader _reader;
		private readonly DxfDocumentBuilder _builder;
		private readonly NotificationEventHandler _notification;

		public DxfTablesSectionReader(
			IDxfStreamReader reader,
			DxfDocumentBuilder builder,
			NotificationEventHandler notification = null)
		{
			this._reader = reader;
			this._builder = builder;
			this._notification = notification;
		}

		public void Read()
		{
			//Advance to the first value in the section
			this._reader.ReadNext();

			//Loop until the section ends
			while (!this._reader.EndSectionFound)
			{
				this.readTable();

				if (this._reader.LastValueAsString == DxfFileToken.EndTable)
					this._reader.ReadNext();
			}
		}

		/// <summary>
		/// Read the tables in the document.
		/// </summary>
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
				case DxfFileToken.TableVport:
					template = new DwgTableTemplate<VPort>(this._builder.DocumentToBuild.VPorts);
					this.readEntries(name, handle.Value, (DwgTableTemplate<VPort>)template);
					break;
				case DxfFileToken.TableLinetype:
					template = new DwgTableTemplate<LineType>(this._builder.DocumentToBuild.LineTypes);
					this.readEntries(name, handle.Value, (DwgTableTemplate<LineType>)template);
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
							//template.TableName = this._reader.LastValueAsString;
							Debug.Assert(this._reader.LastValueAsString == tableName);
							break;
						//Handle (all except DIMSTYLE)
						case 5:
						//Handle (all except DIMSTYLE)
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
						//Soft - pointer ID / handle to owner BLOCK_RECORD object
						case 330:
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

				DwgTemplate template = this.readEntry(tableName, handle);

				//Add the object and the template to the builder
				this._builder.Templates[template.CadObject.Handle] = template;
			}
		}

		private DwgTemplate readEntry(string tableName, ulong handle)
		{
			TableEntry entry = null;
			DwgTemplate template = null;

			//Get the entry
			switch (tableName)
			{
				case DxfFileToken.TableAppId:
					template = this.readAppid();
					break;
				case DxfFileToken.TableBlockRecord:
					//entry = new BlockRecord();
					break;
				case DxfFileToken.TableDimstyle:
					entry = new DimensionStyle();
					break;
				case DxfFileToken.TableLayer:
					entry = Layer.Default;
					break;
				case DxfFileToken.TableLinetype:
					DwgTableEntryTemplate<LineType> ltypeTemplate = new DwgTableEntryTemplate<LineType>(new LineType());
					template = this.readRaw(ltypeTemplate, DxfSubclassMarker.Linetype, this.readLineType);
					break;
				case DxfFileToken.TableStyle:
					//entry = new TextStyle();
					break;
				case DxfFileToken.TableUcs:
					entry = new UCS();
					break;
				case DxfFileToken.TableView:
					//entry = new View();
					break;
				case DxfFileToken.TableVport:
					DwgVPortTemplate vportTemplate = new DwgVPortTemplate(new VPort());
					template = this.readRaw(vportTemplate, DxfSubclassMarker.VPort, this.readVPort);
					break;
				default:
					Debug.Fail($"Unhandeled table {tableName}.");
					break;
			}

			//Setup the common fields
			template.CadObject.Handle = handle;

			return template;
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
				switch (this._reader.LastValueAsInt)
				{
					case 2:
						appId.Name = this._reader.LastValueAsString;
						break;
					case 70:
						//appId.Flags = (StandardFlags)this._reader.LastValueAsShort;
						break;
					default:
						Debug.Fail($"Unhandeled dxf code {this._reader.LastCode} at line {this._reader.Line}.");
						break;
				}

				this._reader.ReadNext();
			}

			return template;
		}

		public bool readLineType(int dxfCode, DwgTemplate template)
		{
			switch (dxfCode)
			{
				default:
					break;
			}

			return false;
		}

		public bool readVPort(int dxfCode, DwgTemplate template)
		{
			DwgVPortTemplate vportTemplate = template as DwgVPortTemplate;

			switch (dxfCode)
			{
				//332	Soft - pointer ID / handle to background object(optional)
				case 332:
					break;
				//333	Soft - pointer ID / handle to shade plot object(optional)
				case 333:
					break;
				//348	Hard - pointer ID / handle to visual style object(optional)
				case 348:
					vportTemplate.StyelHandle = this._reader.LastValueAsHandle;
					break;
				default:
					break;
			}

			return false;
		}

		private DwgTemplate readVPort()
		{
			VPort vport = new VPort();
			DwgVPortTemplate template = new DwgVPortTemplate(vport);

			Debug.Assert(this._reader.LastDxfCode == DxfCode.Subclass);
			Debug.Assert(this._reader.LastValueAsString == DxfSubclassMarker.VPort);

			//Jump the SubclassMarker
			this._reader.ReadNext();

			while (this._reader.LastDxfCode != DxfCode.Start)
			{
				switch (this._reader.LastCode)
				{
					case 2:
						vport.Name = this._reader.LastValueAsString;
						break;
					case 70:
						vport.Flags = (StandardFlags)this._reader.LastValueAsShort;
						break;
					default:
						vport.AssignDxfValue(this._reader.LastDxfCode, this._reader.LastValue);
						Debug.Fail($"Unhandeled dxf code {this._reader.LastCode} at line {this._reader.Line}.");
						break;
				}

				this._reader.ReadNext();
			}

			return template;
		}

		private void readRaw(CadObject cadObject, string subclass)
		{
			Dictionary<DxfCode, object> map = new Dictionary<DxfCode, object>();

			Debug.Assert(this._reader.LastDxfCode == DxfCode.Subclass);
			Debug.Assert(this._reader.LastValueAsString == subclass);

			while (this._reader.LastDxfCode != DxfCode.Start)
			{

				//Check if the dxf code is registered
				if (map.ContainsKey(this._reader.LastDxfCode))
				{
					//Set the value
					map[this._reader.LastDxfCode] = this._reader.LastValue;
				}
				else
				{
					this._notification?.Invoke(null, new NotificationEventArgs($"{cadObject.GetType().Name}: Unhandeled dxf code {this._reader.LastCode} at line {this._reader.Line}."));
				}

				this._reader.ReadNext();
			}

			//Build the table based on the map
			cadObject?.Build(map);
		}

		private DwgTemplate readRaw(DwgTemplate template, string subclass, processCode process)
		{
			Dictionary<DxfCode, object> map = new Dictionary<DxfCode, object>();

			Debug.Assert(this._reader.LastDxfCode == DxfCode.Subclass);
			Debug.Assert(this._reader.LastValueAsString == subclass);

			while (this._reader.LastDxfCode != DxfCode.Start)
			{
				if (process(this._reader.LastCode, template))
					continue;

				//Add the value
				map.Add(this._reader.LastDxfCode, this._reader.LastValue);

				this._reader.ReadNext();
			}

			//Build the table based on the map
			template.CadObject.Build(map);

			return template;
		}

		private delegate bool processCode(int dxfCode, DwgTemplate template);
	}
}
