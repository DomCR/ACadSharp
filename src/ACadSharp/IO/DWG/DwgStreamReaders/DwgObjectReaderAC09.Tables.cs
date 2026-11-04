using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG;

internal partial class DwgObjectReaderAC09
{
	/// <summary>
	/// Address of the BLOCK entity of each block definition by the index of the block in the table.
	/// </summary>
	private readonly Dictionary<int, uint> _blockAddresses = new();

	/// <summary>
	/// Handles of the repeated table entries and the handle of the first entry with the same name.
	/// </summary>
	private readonly Dictionary<ulong, ulong> _repeatedEntries = new();

	private CadBlockRecordTemplate _modelSpaceTemplate;

	private CadBlockRecordTemplate _paperSpaceTemplate;

	private void readTables()
	{
		this._builder.LineTypesTable = (LineTypesTable)this.readTable(new LineTypesTable(), DxfFileToken.TableLinetype, this.readLineType).CadObject;
		this._builder.Layers = (LayersTable)this.readTable(new LayersTable(), DxfFileToken.TableLayer, this.readLayer).CadObject;
		this._builder.TextStyles = (TextStylesTable)this.readTable(new TextStylesTable(), DxfFileToken.TableStyle, this.readTextStyle).CadObject;
		this._builder.Views = (ViewsTable)this.readTable(new ViewsTable(), DxfFileToken.TableView, this.readView).CadObject;
		this._builder.UCSs = (UCSTable)this.readTable(new UCSTable(), DxfFileToken.TableUcs, this.readUcs).CadObject;
		this._builder.VPorts = (VPortsTable)this.readTable(new VPortsTable(), DxfFileToken.TableVport, this.readVPort).CadObject;
		this._builder.AppIds = (AppIdsTable)this.readTable(new AppIdsTable(), DxfFileToken.TableAppId, this.readAppId).CadObject;
		this._builder.DimensionStyles = (DimensionStylesTable)this.readTable(new DimensionStylesTable(), DxfFileToken.TableDimstyle, this.readDimensionStyle).CadObject;

		this.readBlockRecords();

		//The header variables reference the entries by their index
		foreach (string name in new string[]
		{
			nameof(DwgHeaderHandlesCollection.CLAYER),
			nameof(DwgHeaderHandlesCollection.CELTYPE),
			nameof(DwgHeaderHandlesCollection.TEXTSTYLE),
			nameof(DwgHeaderHandlesCollection.UCSNAME_MSPACE),
			nameof(DwgHeaderHandlesCollection.UCSNAME_PSPACE),
		})
		{
			ulong? handle = this._builder.HeaderHandles.GetHandle(name);
			if (handle.HasValue && this._repeatedEntries.TryGetValue(handle.Value, out ulong first))
			{
				this._builder.HeaderHandles.SetHandle(name, first);
			}
		}
	}

	private CadTableTemplate<T> readTable<T>(Table<T> table, string name, Func<int, CadTemplate> readEntry)
		where T : TableEntry
	{
		CadTableTemplate<T> template = new CadTableTemplate<T>(table);
		this.readTable(template, name, readEntry);
		return template;
	}

	private void readTable<T>(CadTableTemplate<T> template, string name, Func<int, CadTemplate> readEntry)
		where T : TableEntry
	{
		template.CadObject.Handle = this._fileHeader.GetTableHandle(name);
		this._builder.AddTemplate(template);

		if (!this._fileHeader.Tables.TryGetValue(name, out DwgFileHeaderAC09.TableDescriptor descriptor)
			|| descriptor.IsEmpty)
		{
			return;
		}

		Dictionary<string, ulong> names = new(StringComparer.OrdinalIgnoreCase);
		for (int i = 0; i < descriptor.Count; i++)
		{
			long position = descriptor.Seeker + i * descriptor.ItemSize;

			CadTemplate entry = null;
			try
			{
				this._reader.Position = position;
				entry = readEntry(i);
			}
			catch (Exception ex)
			{
				if (!this._builder.Configuration.Failsafe)
					throw;

				this._builder.Notify($"Could not read the entry {i} of the table {name}", NotificationType.Error, ex);
			}

			if (entry == null)
				continue;

			this.checkCrc(position, descriptor.ItemSize - 2, $"the entry {i} of the table {name}");

			entry.CadObject.Handle = this._fileHeader.GetEntryHandle(name, i);

			//The anonymous blocks share the name, the other entries are expected to be unique
			T tableEntry = (T)entry.CadObject;
			if (tableEntry is not BlockRecord && !string.IsNullOrEmpty(tableEntry.Name))
			{
				if (names.TryGetValue(tableEntry.Name, out ulong first))
				{
					this._builder.Notify($"Repeated entry {tableEntry.Name} in the table {name}", NotificationType.Warning);
					this._repeatedEntries.Add(tableEntry.Handle, first);
					continue;
				}

				names.Add(tableEntry.Name, tableEntry.Handle);
			}

			entry.OwnerHandle = template.CadObject.Handle;
			template.EntryHandles.Add(entry.CadObject.Handle);

			this._builder.AddTemplate(entry);
		}
	}

	/// <summary>
	/// Read the flags and the name that start every table entry.
	/// </summary>
	/// <returns>False if the entry is deleted or has no name.</returns>
	private bool readEntryCommonData(TableEntry entry)
	{
		return this.readEntryCommonData(entry, out _);
	}

	/// <summary>
	/// Read the flags and the name that start every table entry.
	/// </summary>
	/// <returns>False if the entry is deleted or has no name.</returns>
	private bool readEntryCommonData(TableEntry entry, out byte flags)
	{
		//RC : Flags 70, 0x80 for the deleted entries
		flags = this._reader.ReadByte();
		//TF 32 : Entry name 2
		string name = this._reader.ReadFixedText(32);

		//R11+:
		if (this.R11Plus)
		{
			//RS : Number of references
			short used = this._reader.ReadShort();
		}

		entry.Flags = (StandardFlags)(flags & 0x7F);

		if (!string.IsNullOrWhiteSpace(name))
		{
			entry.Name = name;
		}

		return (flags & 0x80) == 0 && !string.IsNullOrWhiteSpace(name);
	}

	private CadTemplate readLineType(int index)
	{
		LineType ltype = new LineType();
		CadLineTypeTemplate template = new CadLineTypeTemplate(ltype);

		if (!this.readEntryCommonData(ltype))
			return null;

		//TF 48 : Description 3
		ltype.Description = this._reader.ReadFixedText(48);
		//RC : Alignment 72
		byte alignment = this._reader.ReadByte();
		if (alignment != 0)
		{
			ltype.Alignment = (char)alignment;
		}

		//RC : Number of dashes 73
		byte ndashes = this._reader.ReadByte();
		//RD : Pattern length 40
		template.TotalLen = this._reader.ReadDouble();

		//RD x 12 : Dash lengths 49
		for (int i = 0; i < 12; i++)
		{
			double length = this._reader.ReadDouble();
			if (i < ndashes)
			{
				CadLineTypeTemplate.SegmentTemplate segment = new CadLineTypeTemplate.SegmentTemplate();
				segment.Segment.Length = length;
				template.SegmentTemplates.Add(segment);
			}
		}

		return template;
	}

	private CadTemplate readLayer(int index)
	{
		Layer layer = new Layer();
		CadLayerTemplate template = new CadLayerTemplate(layer);

		if (!this.readEntryCommonData(layer))
			return null;

		//RS : Color 62, a negative color turns the layer off
		short color = this._reader.ReadShort();
		layer.IsOn = color >= 0;
		layer.Color = new Color(Math.Abs(color));

		//RS : Linetype index 6
		short ltypeIndex = this._reader.ReadShort();
		template.LineTypeHandle = this.getEntryHandle(DxfFileToken.TableLinetype, ltypeIndex);
		if (!template.LineTypeHandle.HasValue)
		{
			template.LineTypeName = LineType.ContinuousName;
		}

		return template;
	}

	private CadTemplate readTextStyle(int index)
	{
		TextStyle style = new TextStyle();
		CadTableEntryTemplate<TextStyle> template = new CadTableEntryTemplate<TextStyle>(style);

		//The styles that load a shape file have no name
		if (!this.readEntryCommonData(style, out byte flags)
			&& ((flags & 0x80) != 0 || !style.Flags.HasFlag(StyleFlags.IsShape)))
			return null;

		//RD : Fixed height 40
		style.Height = this._reader.ReadDouble();
		//RD : Width factor 41
		style.Width = this._reader.ReadDouble();
		//RD : Oblique angle 50
		style.ObliqueAngle = this._reader.ReadDouble();
		//RC : Generation 71
		style.MirrorFlag = (TextMirrorFlag)this._reader.ReadByte();
		//RD : Last height used 42
		style.LastHeight = this._reader.ReadDouble();
		//TF 64 : Font file name 3
		style.Filename = this._reader.ReadFixedText(64);
		//TF 64 : Big font file name 4
		style.BigFontFilename = this._reader.ReadFixedText(64);

		return template;
	}

	private CadTemplate readView(int index)
	{
		View view = new View();
		CadViewTemplate template = new CadViewTemplate(view);

		if (!this.readEntryCommonData(view))
			return null;

		//RD : View height 40
		view.Height = this._reader.ReadDouble();
		//2RD : View center 10
		view.Center = this._reader.Read2RawDouble();
		//RD : View width 41
		view.Width = this._reader.ReadDouble();
		//3RD : View direction 11
		view.Direction = this._reader.Read3RawDouble();

		//R11+:
		if (this.R11Plus)
		{
			//RS : Flag 3d
			short flag3d = this._reader.ReadShort();
			//3RD : Target 12
			view.Target = this._reader.Read3RawDouble();
			//RS : View mode 71
			view.ViewMode = (ViewModeType)this._reader.ReadShort();
			//RD : Lens length 42
			view.LensLength = this._reader.ReadDouble();
			//RD : Front clip 43
			view.FrontClipping = this._reader.ReadDouble();
			//RD : Back clip 44
			view.BackClipping = this._reader.ReadDouble();
			//RD : Twist angle 50
			view.Angle = this._reader.ReadDouble();
		}

		return template;
	}

	private CadTemplate readUcs(int index)
	{
		UCS ucs = new UCS();
		CadUcsTemplate template = new CadUcsTemplate(ucs);

		if (!this.readEntryCommonData(ucs))
			return null;

		//3RD : Origin 10
		ucs.Origin = this._reader.Read3RawDouble();
		//3RD : X direction 11
		ucs.XAxis = this._reader.Read3RawDouble();
		//3RD : Y direction 12
		ucs.YAxis = this._reader.Read3RawDouble();

		return template;
	}

	private CadTemplate readVPort(int index)
	{
		VPort vport = new VPort();
		CadVPortTemplate template = new CadVPortTemplate(vport);

		if (!this.readEntryCommonData(vport))
			return null;

		//2RD : Lower left 10
		vport.BottomLeft = this._reader.Read2RawDouble();
		//2RD : Upper right 11
		vport.TopRight = this._reader.Read2RawDouble();
		//3RD : View target 17
		vport.Target = this._reader.Read3RawDouble();
		//3RD : View direction 16
		vport.Direction = this._reader.Read3RawDouble();
		//RD : View twist 51
		vport.TwistAngle = this._reader.ReadDouble();
		//RD : View height 40
		vport.ViewHeight = this._reader.ReadDouble();
		//2RD : View center 12
		vport.Center = this._reader.Read2RawDouble();
		//RD : Aspect ratio 41
		vport.AspectRatio = this._reader.ReadDouble();
		//RD : Lens length 42
		vport.LensLength = this._reader.ReadDouble();
		//RD : Front clip 43
		vport.FrontClippingPlane = this._reader.ReadDouble();
		//RD : Back clip 44
		vport.BackClippingPlane = this._reader.ReadDouble();
		//RS : View mode 71
		vport.ViewMode = (ViewModeType)this._reader.ReadShort();
		//RS : Circle zoom 72
		vport.CircleZoomPercent = this._reader.ReadShort();
		//RS : Fast zoom 73
		short fastZoom = this._reader.ReadShort();
		//RS : UCS icon 74
		short ucsIcon = this._reader.ReadShort();
		//RS : Snap on 75
		vport.SnapOn = this._reader.ReadShort() != 0;
		//RS : Grid on 76
		vport.ShowGrid = this._reader.ReadShort() != 0;
		//RS : Snap style 77
		vport.IsometricSnap = this._reader.ReadShort() != 0;
		//RS : Snap isopair 78
		vport.SnapIsoPair = this._reader.ReadShort();
		//RD : Snap rotation 50
		vport.SnapRotation = this._reader.ReadDouble();
		//2RD : Snap base 13
		vport.SnapBasePoint = this._reader.Read2RawDouble();
		//2RD : Snap spacing 14
		vport.SnapSpacing = this._reader.Read2RawDouble();
		//2RD : Grid spacing 15
		vport.GridSpacing = this._reader.Read2RawDouble();

		return template;
	}

	private CadTemplate readAppId(int index)
	{
		AppId appId = new AppId();
		CadTableEntryTemplate<AppId> template = new CadTableEntryTemplate<AppId>(appId);

		if (!this.readEntryCommonData(appId))
			return null;

		return template;
	}

	private CadTemplate readDimensionStyle(int index)
	{
		DimensionStyle style = new DimensionStyle();
		CadDimensionStyleTemplate template = new CadDimensionStyleTemplate(style);

		if (!this.readEntryCommonData(style))
			return null;

		//RD : DIMSCALE 40
		double scale = this._reader.ReadDouble();
		if (scale >= 0)
		{
			style.ScaleFactor = scale;
		}
		//RD : DIMASZ 41
		style.ArrowSize = this._reader.ReadDouble();
		//RD : DIMEXO 42
		style.ExtensionLineOffset = this._reader.ReadDouble();
		//RD : DIMDLI 43
		style.DimensionLineIncrement = this._reader.ReadDouble();
		//RD : DIMEXE 44
		style.ExtensionLineExtension = this._reader.ReadDouble();
		//RD : DIMRND 45
		style.Rounding = this._reader.ReadDouble();
		//RD : DIMDLE 46
		style.DimensionLineExtension = this._reader.ReadDouble();
		//RD : DIMTP 47
		style.PlusTolerance = this._reader.ReadDouble();
		//RD : DIMTM 48
		style.MinusTolerance = this._reader.ReadDouble();
		//RD : DIMTXT 140
		style.TextHeight = this._reader.ReadDouble();
		//RD : DIMCEN 141
		style.CenterMarkSize = this._reader.ReadDouble();
		//RD : DIMTSZ 142
		style.TickSize = this._reader.ReadDouble();
		//RD : DIMALTF 143
		style.AlternateUnitScaleFactor = this._reader.ReadDouble();
		//RD : DIMLFAC 144
		style.LinearScaleFactor = this._reader.ReadDouble();
		//RD : DIMTVP 145
		style.TextVerticalPosition = this._reader.ReadDouble();
		//RC : DIMTOL 71
		style.GenerateTolerances = this._reader.ReadByte() != 0;
		//RC : DIMLIM 72
		style.LimitsGeneration = this._reader.ReadByte() != 0;
		//RC : DIMTIH 73
		style.TextInsideHorizontal = this._reader.ReadByte() != 0;
		//RC : DIMTOH 74
		style.TextOutsideHorizontal = this._reader.ReadByte() != 0;
		//RC : DIMSE1 75
		style.SuppressFirstExtensionLine = this._reader.ReadByte() != 0;
		//RC : DIMSE2 76
		style.SuppressSecondExtensionLine = this._reader.ReadByte() != 0;
		//RC : DIMTAD 77
		style.TextVerticalAlignment = (DimensionTextVerticalAlignment)this._reader.ReadByte();
		//RC : DIMZIN 78
		style.ZeroHandling = (ZeroHandling)this._reader.ReadByte();
		//RC : DIMALT 170
		style.AlternateUnitDimensioning = this._reader.ReadByte() != 0;
		//RC : DIMALTD 171
		style.AlternateUnitDecimalPlaces = this._reader.ReadByte();
		//RC : DIMTOFL 172
		style.TextOutsideExtensions = this._reader.ReadByte() != 0;
		//RC : DIMSAH 173
		style.SeparateArrowBlocks = this._reader.ReadByte() != 0;
		//RC : DIMTIX 174
		style.TextInsideExtensions = this._reader.ReadByte() != 0;
		//RC : DIMSOXD 175
		style.SuppressOutsideExtensions = this._reader.ReadByte() != 0;
		//TF 16 : DIMPOST 3
		style.PostFix = this._reader.ReadFixedText(16);
		//TF 16 : DIMAPOST 4
		style.AlternateDimensioningSuffix = this._reader.ReadFixedText(16);
		//TF 16 : DIMBLK 5
		string dimblk = this._reader.ReadFixedText(16);
		//TF 16 : DIMBLK1 6
		template.DIMBLK1_Name = this.nullIfEmpty(this._reader.ReadFixedText(16));
		//TF 66 : DIMBLK2 7
		template.DIMBLK2_Name = this.nullIfEmpty(this._reader.ReadFixedText(66));
		//RS : DIMCLRD 176
		style.DimensionLineColor = new Color(this._reader.ReadShort());
		//RS : DIMCLRE 177
		style.ExtensionLineColor = new Color(this._reader.ReadShort());
		//RS : DIMCLRT 178
		style.TextColor = new Color(this._reader.ReadShort());
		//RC : DIMUPT
		byte dimupt = this._reader.ReadByte();
		//RD : DIMTFAC 146
		style.ToleranceScaleFactor = this._reader.ReadDouble();
		//RD : DIMGAP 147
		style.DimensionLineGap = this._reader.ReadDouble();

		return template;
	}

	private void readBlockRecords()
	{
		CadBlockCtrlObjectTemplate template = new CadBlockCtrlObjectTemplate(new BlockRecordsTable());
		this._builder.BlockRecords = (BlockRecordsTable)template.CadObject;

		//The model and the paper space are not in the block table of these drawings
		this._modelSpaceTemplate = this.createSpaceRecord(BlockRecord.ModelSpaceName, this._fileHeader.ModelSpaceHandle, template);
		this._paperSpaceTemplate = this.createSpaceRecord(BlockRecord.PaperSpaceName, this._fileHeader.PaperSpaceHandle, template);
		template.ModelSpaceHandle = this._modelSpaceTemplate.CadObject.Handle;
		template.PaperSpaceHandle = this._paperSpaceTemplate.CadObject.Handle;

		this.readTable(template, DxfFileToken.TableBlockRecord, this.readBlockRecord);
	}

	private CadBlockRecordTemplate createSpaceRecord(string name, ulong handle, CadBlockCtrlObjectTemplate table)
	{
		CadBlockRecordTemplate template = new CadBlockRecordTemplate(new BlockRecord(name));
		template.CadObject.Handle = handle;
		template.OwnerHandle = table.CadObject.Handle;

		this._builder.BlockRecordTemplates.Add(template);
		this._builder.AddTemplate(template);

		return template;
	}

	private CadTemplate readBlockRecord(int index)
	{
		BlockRecord record = new BlockRecord();
		CadBlockRecordTemplate template = new CadBlockRecordTemplate(record);

		if (!this.readEntryCommonData(record, out byte flags))
			return null;

		//The block record keeps the flags in the block entity
		record.Flags = (BlockTypeFlags)(flags & 0x7F);

		//RL : Address of the BLOCK entity in the blocks section
		this._blockAddresses.Add(index, (uint)this._reader.ReadInt());

		//Some drawings have the paper space in the table
		if (record.Name.Equals(BlockRecord.PaperSpaceName, StringComparison.OrdinalIgnoreCase))
		{
			this._blockRecords.Add(index, this._paperSpaceTemplate);
			return null;
		}
		else if (record.Name.Equals(BlockRecord.ModelSpaceName, StringComparison.OrdinalIgnoreCase))
		{
			this._blockRecords.Add(index, this._modelSpaceTemplate);
			return null;
		}

		this._blockRecords.Add(index, template);
		this._builder.BlockRecordTemplates.Add(template);

		return template;
	}

	private ulong? getEntryHandle(string table, int index)
	{
		if (index < 0
			|| !this._fileHeader.Tables.TryGetValue(table, out DwgFileHeaderAC09.TableDescriptor descriptor)
			|| index >= descriptor.Count)
		{
			return null;
		}

		ulong handle = this._fileHeader.GetEntryHandle(table, index);
		if (this._repeatedEntries.TryGetValue(handle, out ulong first))
		{
			return first;
		}

		return handle;
	}

	private string nullIfEmpty(string value)
	{
		return string.IsNullOrEmpty(value) ? null : value;
	}
}
