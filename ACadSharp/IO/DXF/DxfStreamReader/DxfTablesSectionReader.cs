using ACadSharp.Exceptions;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using ACadSharp.Types.Units;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
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
			while (this._reader.ValueAsString != DxfFileToken.EndSection)
			{

				if (this._reader.ValueAsString == DxfFileToken.TableEntry)
					this.readTable();
				else
					throw new DxfException($"Unexpected token at the begining of a table: {this._reader.ValueAsString}", this._reader.Position);


				if (this._reader.ValueAsString == DxfFileToken.EndTable)
					this._reader.ReadNext();
				else
					throw new DxfException($"Unexpected token at the end of a table: {this._reader.ValueAsString}", this._reader.Position);
			}
		}

		private void readTable()
		{
			Debug.Assert(this._reader.ValueAsString == DxfFileToken.TableEntry);

			//Read the table name
			this._reader.ReadNext();

			int nentries = 0;
			CadTemplate template = null;
			Dictionary<string, ExtendedData> edata = new Dictionary<string, ExtendedData>();

			this.readCommonObjectData(out string name, out ulong handle, out ulong? ownerHandle, out ulong? xdictHandle, out List<ulong> reactors);

			Debug.Assert(this._reader.ValueAsString == DxfSubclassMarker.Table);

			this._reader.ReadNext();

			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					//Maximum number of entries in table
					case 70:
						nentries = this._reader.ValueAsInt;
						break;
					case 100 when this._reader.ValueAsString == DxfSubclassMarker.DimensionStyleTable:
						while (this._reader.DxfCode != DxfCode.Start)
						{
							//template.CadObject has the code 71 for the count of entries
							//Also has 340 codes for each entry with the handles
							this._reader.ReadNext();
						}
						break;
					case 1001:
						this.readExtendedData(edata);
						break;
					default:
						this._builder.Notify($"Unhandeled dxf code {this._reader.Code} at line {this._reader.Position}.");
						break;
				}

				if (this._reader.DxfCode == DxfCode.Start)
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
			while (this._reader.ValueAsString != DxfFileToken.EndTable)
			{
				this.readCommonObjectData(out string name, out ulong handle, out ulong? ownerHandle, out ulong? xdictHandle, out List<ulong> reactors);

				Debug.Assert(this._reader.ValueAsString == DxfSubclassMarker.TableRecord, $"Expected: {DxfSubclassMarker.TableRecord} but was {this._reader.ValueAsString}");

				this._reader.ReadNext();

				CadTemplate template = null;

				//Get the entry
				switch (name)
				{
					case DxfFileToken.TableAppId:
						template = this.readTableEntry(new CadTableEntryTemplate<AppId>(new AppId()), this.readAppId);
						break;
					case DxfFileToken.TableBlockRecord:
						BlockRecord record = new BlockRecord();
						template = new CadBlockRecordTemplate(record);
						this.readMapped<BlockRecord>(record, template);
						break;
					case DxfFileToken.TableDimstyle:
						template = this.readTableEntry(new CadDimensionStyleTemplate(), this.readDimensionStyle);
						break;
					case DxfFileToken.TableLayer:
						template = this.readTableEntry(new CadLayerTemplate(), this.readLayer);
						break;
					case DxfFileToken.TableLinetype:
						template = this.readTableEntry(new CadLineTypeTemplate(), this.readLineType);
						break;
					case DxfFileToken.TableStyle:
						template = this.readTableEntry(new CadTableEntryTemplate<TextStyle>(new TextStyle()), this.readTextStyle);
						break;
					case DxfFileToken.TableUcs:
						template = this.readTableEntry(new CadUcsTemplate(), this.readUcs);
						break;
					case DxfFileToken.TableView:
						template = this.readTableEntry(new CadViewTemplate(), this.readView);
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
			while (this._reader.DxfCode != DxfCode.Start)
			{
				if (!readEntry(template))
				{
					this.readCommonTableEntryCodes(template, out bool isExtendedData);
					if (isExtendedData)
						continue;
				}

				if (this._reader.DxfCode != DxfCode.Start)
					this._reader.ReadNext();
			}

			return template;
		}

		private void readCommonTableEntryCodes<T>(CadTableEntryTemplate<T> template, out bool isExtendedData)
			where T : TableEntry
		{
			isExtendedData = false;
			switch (this._reader.Code)
			{
				case 2:
					template.CadObject.Name = this._reader.ValueAsString;
					break;
				case 70:
					template.CadObject.Flags = (StandardFlags)this._reader.ValueAsUShort;
					break;
				default:
					this.readCommonCodes(template, out isExtendedData);
					break;
			}
		}

		private bool readAppId(CadTableEntryTemplate<AppId> template)
		{
			switch (this._reader.Code)
			{
				default:
					return false;
			}
		}

		private bool readDimensionStyle(CadTableEntryTemplate<DimensionStyle> template)
		{
			CadDimensionStyleTemplate tmp = (CadDimensionStyleTemplate)template;

			switch (this._reader.Code)
			{
				case 3:
					template.CadObject.PostFix = this._reader.ValueAsString;
					return true;
				case 4:
					template.CadObject.AlternateDimensioningSuffix = this._reader.ValueAsString;
					return true;
				case 5:
					//5	DIMBLK(obsolete, now object ID)
					tmp.DIMBL_Name = this._reader.ValueAsString;
					return true;
				case 6:
					//6	DIMBLK1(obsolete, now object ID)
					tmp.DIMBLK1_Name = this._reader.ValueAsString;
					return true;
				case 7:
					//7	DIMBLK2(obsolete, now object ID)
					tmp.DIMBLK2_Name = this._reader.ValueAsString;
					return true;
				case 40:
					template.CadObject.ScaleFactor = this._reader.ValueAsDouble;
					return true;
				case 41:
					template.CadObject.ArrowSize = this._reader.ValueAsDouble;
					return true;
				case 42:
					template.CadObject.ExtensionLineOffset = this._reader.ValueAsDouble;
					return true;
				case 43:
					template.CadObject.DimensionLineIncrement = this._reader.ValueAsDouble;
					return true;
				case 44:
					template.CadObject.ExtensionLineExtension = this._reader.ValueAsDouble;
					return true;
				case 45:
					template.CadObject.Rounding = this._reader.ValueAsDouble;
					return true;
				case 46:
					template.CadObject.DimensionLineExtension = this._reader.ValueAsDouble;
					return true;
				case 47:
					template.CadObject.PlusTolerance = this._reader.ValueAsDouble;
					return true;
				case 48:
					template.CadObject.MinusTolerance = this._reader.ValueAsDouble;
					return true;
				case 49:
					template.CadObject.FixedExtensionLineLength = this._reader.ValueAsDouble;
					return true;
				case 50:
					template.CadObject.JoggedRadiusDimensionTransverseSegmentAngle = this._reader.ValueAsDouble;
					return true;
				case 69:
					template.CadObject.TextBackgroundFillMode = (DimensionTextBackgroundFillMode)this._reader.ValueAsShort;
					return true;
				case 71:
					template.CadObject.GenerateTolerances = this._reader.ValueAsBool;
					return true;
				case 72:
					template.CadObject.LimitsGeneration = this._reader.ValueAsBool;
					return true;
				case 73:
					template.CadObject.TextInsideHorizontal = this._reader.ValueAsBool;
					return true;
				case 74:
					template.CadObject.TextOutsideHorizontal = this._reader.ValueAsBool;
					return true;
				case 75:
					template.CadObject.SuppressFirstExtensionLine = this._reader.ValueAsBool;
					return true;
				case 76:
					template.CadObject.SuppressSecondExtensionLine = this._reader.ValueAsBool;
					return true;
				case 77:
					template.CadObject.TextVerticalAlignment = (DimensionTextVerticalAlignment)this._reader.ValueAsShort;
					return true;
				case 78:
					template.CadObject.ZeroHandling = (ZeroHandling)this._reader.ValueAsShort;
					return true;
				case 79:
					template.CadObject.AngularZeroHandling = (ZeroHandling)this._reader.ValueAsShort;
					return true;
				case 90:
					template.CadObject.ArcLengthSymbolPosition = (ArcLengthSymbolPosition)(int)this._reader.ValueAsShort;
					return true;
				case 140:
					template.CadObject.TextHeight = this._reader.ValueAsDouble;
					return true;
				case 141:
					template.CadObject.CenterMarkSize = this._reader.ValueAsDouble;
					return true;
				case 142:
					template.CadObject.TickSize = this._reader.ValueAsDouble;
					return true;
				case 143:
					template.CadObject.AlternateUnitScaleFactor = this._reader.ValueAsDouble;
					return true;
				case 144:
					template.CadObject.LinearScaleFactor = this._reader.ValueAsDouble;
					return true;
				case 145:
					template.CadObject.TextVerticalPosition = this._reader.ValueAsDouble;
					return true;
				case 146:
					template.CadObject.ToleranceScaleFactor = this._reader.ValueAsDouble;
					return true;
				case 147:
					template.CadObject.DimensionLineGap = this._reader.ValueAsDouble;
					return true;
				case 148:
					template.CadObject.AlternateUnitRounding = this._reader.ValueAsDouble;
					return true;
				case 170:
					template.CadObject.AlternateUnitDimensioning = this._reader.ValueAsBool;
					return true;
				case 171:
					template.CadObject.AlternateUnitDecimalPlaces = this._reader.ValueAsShort;
					return true;
				case 172:
					template.CadObject.TextOutsideExtensions = this._reader.ValueAsBool;
					return true;
				case 173:
					template.CadObject.SeparateArrowBlocks = this._reader.ValueAsBool;
					return true;
				case 174:
					template.CadObject.TextInsideExtensions = this._reader.ValueAsBool;
					return true;
				case 175:
					template.CadObject.SuppressOutsideExtensions = this._reader.ValueAsBool;
					return true;
				case 176:
					template.CadObject.DimensionLineColor = new Color(this._reader.ValueAsShort);
					return true;
				case 177:
					template.CadObject.ExtensionLineColor = new Color(this._reader.ValueAsShort);
					return true;
				case 178:
					template.CadObject.TextColor = new Color(this._reader.ValueAsShort);
					return true;
				case 179:
					template.CadObject.AngularDimensionDecimalPlaces = this._reader.ValueAsShort;
					return true;
				case 270:
					template.CadObject.LinearUnitFormat = (LinearUnitFormat)this._reader.ValueAsShort;
					return true;
				case 271:
					template.CadObject.DecimalPlaces = this._reader.ValueAsShort;
					return true;
				case 272:
					template.CadObject.ToleranceDecimalPlaces = this._reader.ValueAsShort;
					return true;
				case 273:
					template.CadObject.AlternateUnitFormat = (LinearUnitFormat)this._reader.ValueAsShort;
					return true;
				case 274:
					template.CadObject.AlternateUnitToleranceDecimalPlaces = this._reader.ValueAsShort;
					return true;
				case 275:
					template.CadObject.AngularUnit = (AngularUnitFormat)this._reader.ValueAsShort;
					return true;
				case 276:
					template.CadObject.FractionFormat = (FractionFormat)this._reader.ValueAsShort;
					return true;
				case 277:
					template.CadObject.LinearUnitFormat = (LinearUnitFormat)this._reader.ValueAsShort;
					return true;
				case 278:
					template.CadObject.DecimalSeparator = (char)this._reader.ValueAsShort;
					return true;
				case 279:
					template.CadObject.TextMovement = (TextMovement)this._reader.ValueAsShort;
					return true;
				case 280:
					template.CadObject.TextHorizontalAlignment = (DimensionTextHorizontalAlignment)this._reader.ValueAsShort;
					return true;
				case 281:
					template.CadObject.SuppressFirstDimensionLine = this._reader.ValueAsBool;
					return true;
				case 282:
					template.CadObject.SuppressSecondDimensionLine = this._reader.ValueAsBool;
					return true;
				case 283:
					template.CadObject.ToleranceAlignment = (ToleranceAlignment)this._reader.ValueAsShort;
					return true;
				case 284:
					template.CadObject.ToleranceZeroHandling = (ZeroHandling)this._reader.ValueAsShort;
					return true;
				case 285:
					template.CadObject.AlternateUnitZeroHandling = (ZeroHandling)this._reader.ValueAsShort;
					return true;
				case 286:
					template.CadObject.AlternateUnitToleranceZeroHandling = (ZeroHandling)(byte)this._reader.ValueAsShort;
					return true;
				case 287:
					template.CadObject.DimensionFit = (char)this._reader.ValueAsShort;
					return true;
				case 288:
					template.CadObject.CursorUpdate = this._reader.ValueAsBool;
					return true;
				case 289:
					template.CadObject.DimensionTextArrowFit = this._reader.ValueAsShort;
					return true;
				case 290:
					template.CadObject.IsExtensionLineLengthFixed = this._reader.ValueAsBool;
					return true;
				case 340:
					tmp.TextStyleHandle = this._reader.ValueAsHandle;
					return true;
				case 341:
					tmp.DIMLDRBLK = this._reader.ValueAsHandle;
					return true;
				case 342:
					tmp.DIMBLK = this._reader.ValueAsHandle;
					return true;
				case 343:
					tmp.DIMBLK1 = this._reader.ValueAsHandle;
					return true;
				case 344:
					tmp.DIMBLK2 = this._reader.ValueAsHandle;
					return true;
				case 371:
					template.CadObject.DimensionLineWeight = (LineweightType)this._reader.ValueAsShort;
					return true;
				case 372:
					template.CadObject.ExtensionLineWeight = (LineweightType)this._reader.ValueAsShort;
					return true;
				default:
					return false;
			}
		}

		private bool readLayer(CadTableEntryTemplate<Layer> template)
		{
			CadLayerTemplate tmp = (CadLayerTemplate)template;

			switch (this._reader.Code)
			{
				case 6:
					tmp.LineTypeName = this._reader.ValueAsString;
					return true;
				case 62:
					short index = this._reader.ValueAsShort;
					if (index < 0)
					{
						template.CadObject.IsOn = false;
						index = Math.Abs(index);
					}
					template.CadObject.Color = new Color(index);
					return true;
				case 290:
					template.CadObject.PlotFlag = this._reader.ValueAsBool;
					return true;
				case 347:
					tmp.MaterialHandle = this._reader.ValueAsHandle;
					return true;
				case 348:
					//Unknown code value, always 0
					return true;
				case 370:
					template.CadObject.LineWeight = (LineweightType)this._reader.ValueAsShort;
					return true;
				case 390:
					template.CadObject.PlotStyleName = this._reader.ValueAsHandle;
					return true;
				case 420:
					template.CadObject.Color = Color.FromTrueColor(this._reader.ValueAsInt);
					return true;
				case 430:
					tmp.TrueColorName = this._reader.ValueAsString;
					return true;
				default:
					return false;
			}
		}

		private bool readLineType(CadTableEntryTemplate<LineType> template)
		{
			CadLineTypeTemplate tmp = (CadLineTypeTemplate)template;

			switch (this._reader.Code)
			{
				case 3:
					template.CadObject.Description = this._reader.ValueAsString;
					return true;
				case 40:
					template.CadObject.PatternLen = this._reader.ValueAsDouble;
					return true;
				case 49:
					do
					{
						tmp.SegmentTemplates.Add(this.readLineTypeSegment());
					}
					while (this._reader.Code == 49);
					return true;
				case 72:
					template.CadObject.Alignment = (char)this._reader.ValueAsUShort;
					return true;
				case 73:
					//n segments 
					return true;
				default:
					return false;
			}
		}

		private SegmentTemplate readLineTypeSegment()
		{
			SegmentTemplate template = new SegmentTemplate();
			template.Segment.Length = this._reader.ValueAsDouble;

			//Jump the 49 code
			this._reader.ReadNext();

			while (this._reader.Code != 49 && this._reader.Code != 0)
			{
				switch (this._reader.Code)
				{
					case 9:
						template.Segment.Text = this._reader.ValueAsString;
						break;
					case 44:
						template.Segment.Offset = new CSMath.XY(this._reader.ValueAsDouble, template.Segment.Offset.Y);
						break;
					case 45:
						template.Segment.Offset = new CSMath.XY(template.Segment.Offset.X, this._reader.ValueAsDouble);
						break;
					case 46:
						template.Segment.Scale = this._reader.ValueAsDouble;
						break;
					case 50:
						template.Segment.Rotation = this._reader.ValueAsAngle;
						break;
					case 74:
						template.Segment.Shapeflag = (LinetypeShapeFlags)this._reader.ValueAsUShort;
						break;
					case 75:
						template.Segment.ShapeNumber = (short)this._reader.ValueAsInt;
						break;
					case 340:
						break;
					default:
						this._builder.Notify($"[LineTypeSegment] Unhandeled dxf code {this._reader.Code} with value {this._reader.ValueAsString}, positon {this._reader.Position}", NotificationType.None);
						break;
				}

				this._reader.ReadNext();
			}

			return template;
		}

		private bool readTextStyle(CadTableEntryTemplate<TextStyle> template)
		{
			switch (this._reader.Code)
			{
				case 3:
					template.CadObject.Filename = this._reader.ValueAsString;
					return true;
				case 4:
					template.CadObject.BigFontFilename = this._reader.ValueAsString;
					return true;
				case 40:
					template.CadObject.Height = this._reader.ValueAsDouble;
					return true;
				case 41:
					template.CadObject.Width = this._reader.ValueAsDouble;
					return true;
				case 42:
					template.CadObject.LastHeight = this._reader.ValueAsDouble;
					return true;
				case 50:
					template.CadObject.ObliqueAngle = this._reader.ValueAsAngle;
					return true;
				case 71:
					template.CadObject.MirrorFlag = (Entities.TextMirrorFlag)this._reader.ValueAsShort;
					return true;
				default:
					return false;
			}
		}

		private bool readUcs(CadTableEntryTemplate<UCS> template)
		{
			DxfClassMap map = DxfClassMap.Create<UCS>();

			switch (this._reader.Code)
			{
				default:
					return this.tryAssignCurrentValue(template.CadObject, map);
			}
		}

		private bool readView(CadTableEntryTemplate<View> template)
		{
			CadViewTemplate tmp = template as CadViewTemplate;
			DxfClassMap map = DxfClassMap.Create<View>();

			switch (this._reader.Code)
			{
				case 348:
					tmp.VisualStyleHandle = this._reader.ValueAsHandle;
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map);
			}
		}
	}
}
