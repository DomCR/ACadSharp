using ACadSharp.Exceptions;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using static ACadSharp.IO.Templates.CadLineTypeTemplate;

namespace ACadSharp.IO.DXF
{
	internal class DxfTablesSectionReader : DxfSectionReaderBase
	{
		public delegate bool ReadEntryDelegate<T>(CadTableEntryTemplate<T> template) where T : TableEntry;

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
						template = this.readTableEntry<AppId>(new CadTableEntryTemplate<AppId>(new AppId()), this.readAppId);
						break;
					case DxfFileToken.TableBlockRecord:
						BlockRecord record = new BlockRecord();
						template = new CadBlockRecordTemplate(record);
						this.readMapped<BlockRecord>(record, template);
						break;
					case DxfFileToken.TableDimstyle:
						template = this.readTableEntry<DimensionStyle>(new CadDimensionStyleTemplate(), this.readDimensionStyle);
						break;
					case DxfFileToken.TableLayer:
						template = this.readTableEntry<Layer>(new CadLayerTemplate(), this.readLayer);
						break;
					case DxfFileToken.TableLinetype:
						template = this.readTableEntry<LineType>(new CadLineTypeTemplate(), this.readLineType);
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

		private CadTemplate readTableEntry<T>(CadTableEntryTemplate<T> template, ReadEntryDelegate<T> readEntry)
			where T : TableEntry
		{
			while (this._reader.LastDxfCode != DxfCode.Start)
			{
				if (!readEntry(template))
				{
					this.readCommonTableEntryCodes(template, out bool isExtendedData);
					if (isExtendedData)
						continue;
				}

				if (this._reader.LastDxfCode != DxfCode.Start)
					this._reader.ReadNext();
			}

			return template;
		}

		private void readCommonTableEntryCodes<T>(CadTableEntryTemplate<T> template, out bool isExtendedData)
			where T : TableEntry
		{
			isExtendedData = false;
			switch (this._reader.LastCode)
			{
				case 2:
					template.CadObject.Name = this._reader.LastValueAsString;
					break;
				case 70:
					template.CadObject.Flags = (StandardFlags)this._reader.LastValueAsUShort;
					break;
				default:
					this.readCommonCodes(template, out isExtendedData);
					break;
			}
		}

		private bool readAppId(CadTableEntryTemplate<AppId> template)
		{
			switch (this._reader.LastCode)
			{
				default:
					return false;
			}
		}

		private bool readDimensionStyle(CadTableEntryTemplate<DimensionStyle> template)
		{
			CadDimensionStyleTemplate tmp = (CadDimensionStyleTemplate)template;

			switch (this._reader.LastCode)
			{
				case 3:
					template.CadObject.PostFix = this._reader.LastValueAsString;
					return true;
				case 4:
					template.CadObject.AlternateDimensioningSuffix = this._reader.LastValueAsString;
					return true;
				case 71:
					template.CadObject.GenerateTolerances = this._reader.LastValueAsBool;
					return true;
				case 72:
					template.CadObject.LimitsGeneration = this._reader.LastValueAsBool;
					return true;
				case 73:
					template.CadObject.TextInsideHorizontal = this._reader.LastValueAsBool;
					return true;
				default:
					return false;
			}
		}

		private bool readLayer(CadTableEntryTemplate<Layer> template)
		{
			CadLayerTemplate tmp = (CadLayerTemplate)template;

			switch (this._reader.LastCode)
			{
				case 6:
					tmp.LineTypeName = this._reader.LastValueAsString;
					return true;
				case 62:
					short index = this._reader.LastValueAsShort;
					if (index < 0)
					{
						template.CadObject.IsOn = false;
						index = Math.Abs(index);
					}

					template.CadObject.Color = new Color(index);
					return true;
				case 290:
					template.CadObject.PlotFlag = this._reader.LastValueAsBool;
					return true;
				case 347:
					tmp.MaterialHandle = this._reader.LastValueAsHandle;
					return true;
				case 348:
					//Unknown code value, always 0
					return true;
				case 370:
					template.CadObject.LineWeight = (LineweightType)this._reader.LastValueAsShort;
					return true;
				case 390:
					template.CadObject.PlotStyleName = this._reader.LastValueAsHandle;
					return true;
				case 420:
					template.CadObject.Color = Color.FromTrueColor(this._reader.LastValueAsInt);
					return true;
				case 430:
					tmp.TrueColorName = this._reader.LastValueAsString;
					return true;
				default:
					return false;
			}
		}

		private bool readLineType(CadTableEntryTemplate<LineType> template)
		{
			CadLineTypeTemplate tmp = (CadLineTypeTemplate)template;

			switch (this._reader.LastCode)
			{
				case 3:
					template.CadObject.Description = this._reader.LastValueAsString;
					return true;
				case 40:
					template.CadObject.PatternLen = this._reader.LastValueAsDouble;
					return true;
				case 49:
					do
					{
						tmp.SegmentTemplates.Add(this.readLineTypeSegment());
					}
					while (this._reader.LastCode == 49);
					return true;
				case 72:
					template.CadObject.Alignment = (char)this._reader.LastValueAsUShort;
					return true;
				case 73:
					//n segments 
					return true;
				default:
					return false;
			}
		}

		private CadLineTypeTemplate.SegmentTemplate readLineTypeSegment()
		{
			SegmentTemplate template = new SegmentTemplate();
			template.Segment.Length = this._reader.LastValueAsDouble;

			//Jump the 49 code
			this._reader.ReadNext();

			while (this._reader.LastCode != 49 && this._reader.LastCode != 0)
			{
				switch (this._reader.LastCode)
				{
					case 9:
						template.Segment.Text = this._reader.LastValueAsString;
						break;
					case 44:
						template.Segment.Offset = new CSMath.XY(this._reader.LastValueAsDouble, template.Segment.Offset.Y);
						break;
					case 45:
						template.Segment.Offset = new CSMath.XY(template.Segment.Offset.X, this._reader.LastValueAsDouble);
						break;
					case 46:
						template.Segment.Scale = this._reader.LastValueAsDouble;
						break;
					case 50:
						template.Segment.Rotation = this._reader.LastValueAsDouble * MathUtils.DegToRad;
						break;
					case 74:
						template.Segment.Shapeflag = (LinetypeShapeFlags)this._reader.LastValueAsUShort;
						break;
					case 75:
						template.Segment.ShapeNumber = (short)this._reader.LastValueAsInt;
						break;
					case 340:
						break;
					default:
						this._builder.Notify($"[LineTypeSegment] Unhandeled dxf code {this._reader.LastCode} with value {this._reader.LastValueAsString}, positon {this._reader.Position}", NotificationType.None);
						break;
				}

				this._reader.ReadNext();
			}

			return template;
		}
	}
}
