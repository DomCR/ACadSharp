using ACadSharp.Blocks;
using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.Entities.AecObjects;
using ACadSharp.Entities.Mechanical;
using ACadSharp.Objects;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using ACadSharp.XData;
using CSUtilities.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ACadSharp.IO.DWG;

internal partial class DwgObjectWriter : DwgSectionIO
{
	/// <summary>
	/// Key : handle | Value : Offset
	/// </summary>
	public Dictionary<ulong, long> Map { get; } = new Dictionary<ulong, long>();

	public override string SectionName => DwgSectionDefinition.AcDbObjects;

	public bool WriteDynamicParameters { get; }

	public bool WriteShapes { get; }

	public bool WriteXData { get; }

	public bool WriteXRecords { get; }

	private CadDocument _document;

	private MemoryStream _msmain;

	private Entity _next;

	private Queue<NonGraphicalObject> _objects = new();

	private Entity _prev;

	private Stream _stream;

	private IDwgStreamWriter _writer;

	public DwgObjectWriter(Stream stream, CadDocument document, Encoding encoding,
		bool writeXRecords = true,
		bool writeXData = true,
		bool writeShapes = true,
		bool writeDynamicParameters = true) : base(document.Header.Version)
	{
		this._stream = stream;
		this._document = document;

		this._msmain = new MemoryStream();
		this._writer = DwgStreamWriterBase.GetMergedWriter(document.Header.Version, this._msmain, encoding);
		this.WriteXRecords = writeXRecords;
		this.WriteXData = writeXData;
		this.WriteShapes = writeShapes;
		this.WriteDynamicParameters = writeDynamicParameters;
	}

	public void Write()
	{
		//For R18 and later the section data (right after the page header) starts with a
		//RL value of 0x0dca (meaning unknown).
		if (this.R2004Plus)
		{
			byte[] arr = LittleEndianConverter.Instance.GetBytes((int)0xDCA);
			this._stream.Write(arr, 0, arr.Length);
		}

		this.enqueueValidObject(this._document.RootDictionary);

		this.writeBlockControl();
		this.writeTable(this._document.Layers);
		this.writeTable(this._document.TextStyles);
		this.writeLTypeControlObject();
		this.writeTable(this._document.Views);
		this.writeTable(this._document.UCSs);
		this.writeTable(this._document.VPorts);
		this.writeTable(this._document.AppIds);
		//For some reason the dimension must be writen the last
		this.writeTable(this._document.DimensionStyles);

		if (this.R2004Pre)
		{
			this.writeTable(this._document.VEntityControl);
		}

		this.writeBlockEntities();
		this.writeObjects();
	}

	private void enqueueValidObject(NonGraphicalObject obj)
	{
		if (!obj.IsValid())
		{
			this.notify($"Invalid object {obj.GetType().FullName} with handle {obj.Handle}", NotificationType.Warning);
			return;
		}

		this._objects.Enqueue(obj);
	}

	private Entity[] getCompatibleEntities(IEnumerable<Entity> entities)
	{
		return entities.Where(e => this.isEntitySupported(e)).ToArray();
	}

	private byte getEntMode(Entity entity)
	{
		if (entity.Owner == null)
		{
			return 0;
		}

		if (entity.Owner.Handle == this._document.PaperSpace.Handle)
		{
			return 0b01;
		}

		if (entity.Owner.Handle == this._document.ModelSpace.Handle)
		{
			return 0b10;
		}

		return 0;
	}

	private bool isEntitySupported(Entity entity)
	{
		switch (entity)
		{
			case UnknownEntity:
				return false;
			case Shape:
				return this.WriteShapes;
			case TableEntity when !this.R2010Plus:
			case Wall:
			case MechanicalEntity:
			case ProxyEntity:
			case Solid3D:
			case CadBody:
			case Region:
				this.notify($"Entity type not implemented {entity.GetType().FullName}", NotificationType.NotImplemented);
				return false;
			default:
				return true;
		}
	}

	private void registerObject(CadObject cadObject)
	{
		this._writer.WriteSpearShift();

		//Set the position to the entity to find
		long position = this._stream.Position;
		CRC8StreamHandler crc = new CRC8StreamHandler(this._stream, 0xC0C1);

		//MS : Size of object, not including the CRC
		uint size = (uint)this._msmain.Length;
		long sizeb = (this._msmain.Length << 3) - this._writer.SavedPositionInBits;
		this.writeSize(crc, size);

		//R2010+:
		if (this.R2010Plus)
		{
			//MC : Size in bits of the handle stream (unsigned, 0x40 is not interpreted as sign).
			//This includes the padding bits at the end of the handle stream
			//(the padding bits make sure the object stream ends on a byte boundary).
			this.writeSizeInBits(crc, (ulong)sizeb);
		}

		//Write the object in the stream
		crc.Write(this._msmain.GetBuffer(), 0, (int)this._msmain.Length);
		this._stream.Write(LittleEndianConverter.Instance.GetBytes(crc.Seed), 0, 2);

		this.Map.Add(cadObject.Handle, position);
	}

	private void writeAppId(AppId app)
	{
		this.writeCommonNonEntityData(app);

		//Common:
		//Entry name TV 2
		this._writer.WriteVariableText(app.Name);

		this.writeXrefDependantBit(app);

		//Unknown RC 71 Undoc'd 71-group; doesn't even appear in DXF or an entget if it's 0.
		this._writer.WriteByte(0);

		//External reference block handle(hard pointer)	??
		this._writer.HandleReference(DwgReferenceType.HardPointer, 0);

		this.registerObject(app);
	}

	private void writeBlockBegin(Block block)
	{
		this.writeCommonEntityData(block);

		//Common:
		//Entry name TV 2
		this._writer.WriteVariableText(block.Name);

		this.registerObject(block);
	}

	private void writeBlockControl()
	{
		this.writeCommonNonEntityData(this._document.BlockRecords);

		//Common:
		//Numentries BL 70 Doesn't count *MODEL_SPACE and *PAPER_SPACE.
		this._writer.WriteBitLong(this._document.BlockRecords.Count - 2);

		foreach (var item in this._document.BlockRecords)
		{
			if (item.Name.Equals(BlockRecord.ModelSpaceName, StringComparison.OrdinalIgnoreCase)
				|| item.Name.Equals(BlockRecord.PaperSpaceName, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			//numentries handles of blockheaders in the file (soft owner)
			this._writer.HandleReference(DwgReferenceType.SoftOwnership, item);
		}

		//*MODEL_SPACE and *PAPER_SPACE(hard owner).
		this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.ModelSpace);
		this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.PaperSpace);

		this.registerObject(this._document.BlockRecords);

		this.writeEntries(this._document.BlockRecords);
	}

	private void writeBlockEnd(BlockEnd blkEnd)
	{
		this.writeCommonEntityData(blkEnd);

		this.registerObject(blkEnd);
	}

	private void writeBlockEntities()
	{
		foreach (BlockRecord blkRecord in this._document.BlockRecords)
		{
			this.writeBlockBegin(blkRecord.BlockEntity);

			this._prev = null;
			this._next = null;
			Entity[] arr = getCompatibleEntities(blkRecord.Entities);
			for (int i = 0; i < arr.Length; i++)
			{
				this._prev = arr.ElementAtOrDefault(i - 1);
				Entity e = arr[i];
				this._next = arr.ElementAtOrDefault(i + 1);

				this.writeEntity(e);
			}

			this._prev = null;
			this._next = null;

			this.writeBlockEnd(blkRecord.BlockEnd);
		}
	}

	private void writeBlockHeader(BlockRecord record)
	{
		Entity[] entities = getCompatibleEntities(record.Entities);

		this.writeCommonNonEntityData(record);

		//Common:
		//Entry name TV 2
		if (record.Flags.HasFlag(BlockTypeFlags.Anonymous))
		{
			//Warning: anonymous blocks do not write the full name, only *{type character}
			this._writer.WriteVariableText(record.Name.Substring(0, 2));
		}
		else if (record.Layout != null)
		{
			var processedBlockName = new string(record.Name.Where(c => !char.IsDigit(c)).ToArray());
			this._writer.WriteVariableText(processedBlockName);
		}
		else
		{
			this._writer.WriteVariableText(record.Name);
		}

		this.writeXrefDependantBit(record);

		//Anonymous B 1 if this is an anonymous block (1 bit)
		this._writer.WriteBit(record.Flags.HasFlag(BlockTypeFlags.Anonymous));

		//Hasatts B 1 if block contains attdefs (2 bit)
		this._writer.WriteBit(record.HasAttributes);

		//Blkisxref B 1 if block is xref (4 bit)
		this._writer.WriteBit(record.Flags.HasFlag(BlockTypeFlags.XRef));

		//Xrefoverlaid B 1 if an overlaid xref (8 bit)
		this._writer.WriteBit(record.Flags.HasFlag(BlockTypeFlags.XRefOverlay));

		//R2000+:
		if (this.R2000Plus)
		{
			//Loaded Bit B 0 indicates loaded for an xref
			this._writer.WriteBit(record.IsUnloaded);
		}

		//R2004+:
		if (this.R2004Plus
			&& !record.Flags.HasFlag(BlockTypeFlags.XRef)
			&& !record.Flags.HasFlag(BlockTypeFlags.XRefOverlay))
		{
			//Owned Object Count BL Number of objects owned by this object.
			_writer.WriteBitLong(entities.Length);
		}

		//Common:
		//Base pt 3BD 10 Base point of block.
		this._writer.Write3BitDouble(record.BlockEntity.BasePoint);
		//Xref pname TV 1 Xref pathname. That's right: DXF 1 AND 3!
		//3 1 appears in a tblnext/ search elist; 3 appears in an entget.
		this._writer.WriteVariableText(record.BlockEntity.XRefPath);

		//R2000+:
		if (this.R2000Plus)
		{
			//Insert Count RC A sequence of zero or more non-zero RC’s, followed by a terminating 0 RC.The total number of these indicates how many insert handles will be present.
			foreach (var item in this._document.Entities.OfType<Insert>()
				.Where(i => i.Block?.Name == record?.Name))
			{
				this._writer.WriteByte(1);
			}

			this._writer.WriteByte(0);

			//Block Description TV 4 Block description.
			this._writer.WriteVariableText(record.BlockEntity.Comments);

			//Size of preview data BL Indicates number of bytes of data following.
			this._writer.WriteBitLong(0);
		}

		//R2007+:
		if (this.R2007Plus)
		{
			//Insert units BS 70
			this._writer.WriteBitShort((short)record.Units);
			//Explodable B 280
			this._writer.WriteBit(record.IsExplodable);
			//Block scaling RC 281
			this._writer.WriteByte((byte)(record.CanScale ? 1u : 0u));
		}

		//NULL(hard pointer)
		this._writer.HandleReference(DwgReferenceType.HardPointer, 0);
		//BLOCK entity. (hard owner)
		//Block begin object
		this._writer.HandleReference(DwgReferenceType.HardOwnership, record.BlockEntity);

		//R13-R2000:
		if (this._version >= ACadVersion.AC1012 && this._version <= ACadVersion.AC1015
				&& !record.Flags.HasFlag(BlockTypeFlags.XRef)
				&& !record.Flags.HasFlag(BlockTypeFlags.XRefOverlay))
		{
			if (entities.Any())
			{
				//first entity in the def. (soft pointer)
				this._writer.HandleReference(DwgReferenceType.SoftPointer, entities.First());
				//last entity in the def. (soft pointer)
				this._writer.HandleReference(DwgReferenceType.SoftPointer, entities.Last());
			}
			else
			{
				this._writer.HandleReference(DwgReferenceType.SoftPointer, 0);
				this._writer.HandleReference(DwgReferenceType.SoftPointer, 0);
			}
		}

		//R2004+:
		if (this.R2004Plus)
		{
			foreach (var item in entities)
			{
				//H[ENTITY(hard owner)] Repeats “Owned Object Count” times.
				this._writer.HandleReference(DwgReferenceType.HardOwnership, item);
			}
		}

		//Common:
		//ENDBLK entity. (hard owner)
		this._writer.HandleReference(DwgReferenceType.HardOwnership, record.BlockEnd);

		//R2000+:
		if (this.R2000Plus)
		{
			foreach (var item in this._document.Entities.OfType<Insert>()
				.Where(i => i.Block?.Name == record.Name))
			{
				this._writer.HandleReference(DwgReferenceType.SoftPointer, item);
			}

			//Layout Handle H(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, record.Layout);
		}

		this.registerObject(record);
	}

	private void writeBlockRecord(BlockRecord blkRecord)
	{
		this.writeBlockHeader(blkRecord);
	}

	private void writeCommonData(CadObject cadObject)
	{
		//Reset the current stream to re-write a new object in it
		this._writer.ResetStream();

		switch (cadObject.ObjectType)
		{
			case ObjectType.LAYOUT when this.R2004Pre:
			case ObjectType.UNLISTED:
				if (this._document.Classes.TryGetByName(cadObject.ObjectName, out DxfClass dxfClass))
				{
					this._writer.WriteObjectType(dxfClass.ClassNumber);
				}
				else
				{
					this.notify($"Dxf Class not found for {cadObject.ObjectType} fullname: {cadObject.GetType().FullName}", NotificationType.Warning);
					return;
				}
				break;
			case ObjectType.INVALID:
			case ObjectType.UNDEFINED:
				this.notify($"CadObject type: {cadObject.ObjectType} fullname: {cadObject.GetType().FullName}", NotificationType.NotImplemented);
				return;
			default:
				this._writer.WriteObjectType(cadObject.ObjectType);
				break;
		}

		if (this._version >= ACadVersion.AC1015 && this._version < ACadVersion.AC1024)
		{
			//Obj size RL size of object in bits, not including end handles
			this._writer.SavePositonForSize();
		}

		//Common:
		//Handle H 5 code 0, length followed by the handle bytes.
		this._writer.Main.HandleReference(cadObject);

		//Extended object data, if any
		this.writeExtendedData(cadObject.ExtendedData);
	}

	private void writeCommonEntityData(Entity entity)
	{
		this.writeCommonData(entity);

		//Graphic present Flag B 1 if a graphic is present
		this._writer.WriteBit(false);

		//R13 - R14 Only:
		if (this._version >= ACadVersion.AC1012 && this._version <= ACadVersion.AC1014)
		{
			this._writer.SavePositonForSize();
		}

		this.writeEntityMode(entity);
	}

	private void writeCommonNonEntityData(CadObject cadObject)
	{
		this.writeCommonData(cadObject);

		//R13-R14 Only:
		//Obj size RL size of object in bits, not including end handles
		if (this.R13_14Only)
			this._writer.SavePositonForSize();

		//[Owner ref handle (soft pointer)]
		this._writer.HandleReference(DwgReferenceType.SoftPointer, cadObject.Owner.Handle);

		//write the cad object reactors
		this.writeReactorsAndDictionaryHandle(cadObject);
	}

	private void writeDimensionStyle(DimensionStyle dimStyle)
	{
		this.writeCommonNonEntityData(dimStyle);

		//Common:
		//Entry name TV 2
		this._writer.WriteVariableText(dimStyle.Name);

		this.writeXrefDependantBit(dimStyle);

		//R13 & R14 Only:
		if (this.R13_14Only)
		{
			//DIMTOL B 71
			this._writer.WriteBit(dimStyle.GenerateTolerances);
			//DIMLIM B 72
			this._writer.WriteBit(dimStyle.LimitsGeneration);
			//DIMTIH B 73
			this._writer.WriteBit(dimStyle.TextOutsideHorizontal);
			//DIMTOH B 74
			this._writer.WriteBit(dimStyle.SuppressFirstExtensionLine);
			//DIMSE1 B 75
			this._writer.WriteBit(dimStyle.SuppressSecondExtensionLine);
			//DIMSE2 B 76
			this._writer.WriteBit(dimStyle.TextInsideHorizontal);
			//DIMALT B 170
			this._writer.WriteBit(dimStyle.AlternateUnitDimensioning);
			//DIMTOFL B 172
			this._writer.WriteBit(dimStyle.TextOutsideExtensions);
			//DIMSAH B 173
			this._writer.WriteBit(dimStyle.SeparateArrowBlocks);
			//DIMTIX B 174
			this._writer.WriteBit(dimStyle.TextInsideExtensions);
			//DIMSOXD B 175
			this._writer.WriteBit(dimStyle.SuppressOutsideExtensions);
			//DIMALTD RC 171
			this._writer.WriteByte((byte)dimStyle.AlternateUnitDecimalPlaces);
			//DIMZIN RC 78
			this._writer.WriteByte((byte)dimStyle.ZeroHandling);
			//DIMSD1 B 281
			this._writer.WriteBit(dimStyle.SuppressFirstDimensionLine);
			//DIMSD2 B 282
			this._writer.WriteBit(dimStyle.SuppressSecondDimensionLine);
			//DIMTOLJ RC 283
			this._writer.WriteByte((byte)dimStyle.ToleranceAlignment);
			//DIMJUST RC 280
			this._writer.WriteByte((byte)dimStyle.TextHorizontalAlignment);
			//DIMFIT RC 287
			this._writer.WriteByte((byte)dimStyle.DimensionFit);
			//DIMUPT B 288
			this._writer.WriteBit(dimStyle.CursorUpdate);
			//DIMTZIN RC 284
			this._writer.WriteByte((byte)dimStyle.ToleranceZeroHandling);
			//DIMALTZ RC 285
			this._writer.WriteByte((byte)dimStyle.AlternateUnitZeroHandling);
			//DIMALTTZ RC 286
			this._writer.WriteByte((byte)dimStyle.AlternateUnitToleranceZeroHandling);
			//DIMTAD RC 77
			this._writer.WriteByte((byte)dimStyle.TextVerticalAlignment);
			//DIMUNIT BS 270
			this._writer.WriteBitShort(dimStyle.DimensionUnit);
			//DIMAUNIT BS 275
			this._writer.WriteBitShort((short)dimStyle.AngularUnit);
			//DIMDEC BS 271
			this._writer.WriteBitShort(dimStyle.DecimalPlaces);
			//DIMTDEC BS 272
			this._writer.WriteBitShort(dimStyle.ToleranceDecimalPlaces);
			//DIMALTU BS 273
			this._writer.WriteBitShort((short)dimStyle.AlternateUnitFormat);
			//DIMALTTD BS 274
			this._writer.WriteBitShort(dimStyle.AlternateUnitToleranceDecimalPlaces);
			//DIMSCALE BD 40
			this._writer.WriteBitDouble(dimStyle.ScaleFactor);
			//DIMASZ BD 41
			this._writer.WriteBitDouble(dimStyle.ArrowSize);
			//DIMEXO BD 42
			this._writer.WriteBitDouble(dimStyle.ExtensionLineOffset);
			//DIMDLI BD 43
			this._writer.WriteBitDouble(dimStyle.DimensionLineIncrement);
			//DIMEXE BD 44
			this._writer.WriteBitDouble(dimStyle.ExtensionLineExtension);
			//DIMRND BD 45
			this._writer.WriteBitDouble(dimStyle.Rounding);
			//DIMDLE BD 46
			this._writer.WriteBitDouble(dimStyle.DimensionLineExtension);
			//DIMTP BD 47
			this._writer.WriteBitDouble(dimStyle.PlusTolerance);
			//DIMTM BD 48
			this._writer.WriteBitDouble(dimStyle.MinusTolerance);
			//DIMTXT BD 140
			this._writer.WriteBitDouble(dimStyle.TextHeight);
			//DIMCEN BD 141
			this._writer.WriteBitDouble(dimStyle.CenterMarkSize);
			//DIMTSZ BD 142
			this._writer.WriteBitDouble(dimStyle.TickSize);
			//DIMALTF BD 143
			this._writer.WriteBitDouble(dimStyle.AlternateUnitScaleFactor);
			//DIMLFAC BD 144
			this._writer.WriteBitDouble(dimStyle.LinearScaleFactor);
			//DIMTVP BD 145
			this._writer.WriteBitDouble(dimStyle.TextVerticalPosition);
			//DIMTFAC BD 146
			this._writer.WriteBitDouble(dimStyle.ToleranceScaleFactor);
			//DIMGAP BD 147
			this._writer.WriteBitDouble(dimStyle.DimensionLineGap);

			//DIMPOST T 3
			this._writer.WriteVariableText(dimStyle.PostFix);
			//DIMAPOST T 4
			this._writer.WriteVariableText(dimStyle.AlternateDimensioningSuffix);

			//DIMBLK T 5
			this._writer.WriteVariableText(dimStyle.ArrowBlock?.Name);
			//DIMBLK1 T 6
			this._writer.WriteVariableText(dimStyle.DimArrow1?.Name);
			//DIMBLK2 T 7
			this._writer.WriteVariableText(dimStyle.DimArrow2?.Name);

			//DIMCLRD BS 176
			this._writer.WriteCmColor(dimStyle.DimensionLineColor);
			//DIMCLRE BS 177
			this._writer.WriteCmColor(dimStyle.ExtensionLineColor);
			//DIMCLRT BS 178
			this._writer.WriteCmColor(dimStyle.TextColor);
		}

		//R2000+:
		if (this.R2000Plus)
		{
			//DIMPOST TV 3
			this._writer.WriteVariableText(dimStyle.PostFix);
			//DIMAPOST TV 4
			this._writer.WriteVariableText(dimStyle.AlternateDimensioningSuffix);
			//DIMSCALE BD 40
			this._writer.WriteBitDouble(dimStyle.ScaleFactor);
			//DIMASZ BD 41
			this._writer.WriteBitDouble(dimStyle.ArrowSize);
			//DIMEXO BD 42
			this._writer.WriteBitDouble(dimStyle.ExtensionLineOffset);
			//DIMDLI BD 43
			this._writer.WriteBitDouble(dimStyle.DimensionLineIncrement);
			//DIMEXE BD 44
			this._writer.WriteBitDouble(dimStyle.ExtensionLineExtension);
			//DIMRND BD 45
			this._writer.WriteBitDouble(dimStyle.Rounding);
			//DIMDLE BD 46
			this._writer.WriteBitDouble(dimStyle.DimensionLineExtension);
			//DIMTP BD 47
			this._writer.WriteBitDouble(dimStyle.PlusTolerance);
			//DIMTM BD 48
			this._writer.WriteBitDouble(dimStyle.MinusTolerance);
		}

		//R2007+:
		if (this.R2007Plus)
		{
			//DIMFXL BD 49
			this._writer.WriteBitDouble(dimStyle.FixedExtensionLineLength);
			//DIMJOGANG BD 50
			this._writer.WriteBitDouble(dimStyle.JoggedRadiusDimensionTransverseSegmentAngle);
			//DIMTFILL BS 69
			this._writer.WriteBitShort((short)dimStyle.TextBackgroundFillMode);
			//DIMTFILLCLR CMC 70
			this._writer.WriteCmColor(dimStyle.TextBackgroundColor);
		}

		//R2000+:
		if (this.R2000Plus)
		{
			//DIMTOL B 71
			this._writer.WriteBit(dimStyle.GenerateTolerances);
			//DIMLIM B 72
			this._writer.WriteBit(dimStyle.LimitsGeneration);
			//DIMTIH B 73
			this._writer.WriteBit(dimStyle.TextInsideHorizontal);
			//DIMTOH B 74
			this._writer.WriteBit(dimStyle.TextOutsideHorizontal);
			//DIMSE1 B 75
			this._writer.WriteBit(dimStyle.SuppressFirstExtensionLine);
			//DIMSE2 B 76
			this._writer.WriteBit(dimStyle.SuppressSecondExtensionLine);
			//DIMTAD BS 77
			this._writer.WriteBitShort((short)dimStyle.TextVerticalAlignment);
			//DIMZIN BS 78
			this._writer.WriteBitShort((short)dimStyle.ZeroHandling);
			//DIMAZIN BS 79
			this._writer.WriteBitShort((short)dimStyle.AngularZeroHandling);
		}

		//R2007 +:
		if (this.R2007Plus)
		{
			//DIMARCSYM BS 90
			this._writer.WriteBitShort((short)dimStyle.ArcLengthSymbolPosition);
		}

		//R2000 +:
		if (this.R2000Plus)
		{
			//DIMTXT BD 140
			this._writer.WriteBitDouble(dimStyle.TextHeight);
			//DIMCEN BD 141
			this._writer.WriteBitDouble(dimStyle.CenterMarkSize);
			//DIMTSZ BD 142
			this._writer.WriteBitDouble(dimStyle.TickSize);
			//DIMALTF BD 143
			this._writer.WriteBitDouble(dimStyle.AlternateUnitScaleFactor);
			//DIMLFAC BD 144
			this._writer.WriteBitDouble(dimStyle.LinearScaleFactor);
			//DIMTVP BD 145
			this._writer.WriteBitDouble(dimStyle.TextVerticalPosition);
			//DIMTFAC BD 146
			this._writer.WriteBitDouble(dimStyle.ToleranceScaleFactor);
			//DIMGAP BD 147
			this._writer.WriteBitDouble(dimStyle.DimensionLineGap);
			//DIMALTRND BD 148
			this._writer.WriteBitDouble(dimStyle.AlternateUnitRounding);
			//DIMALT B 170
			this._writer.WriteBit(dimStyle.AlternateUnitDimensioning);
			//DIMALTD BS 171
			this._writer.WriteBitShort(dimStyle.AlternateUnitDecimalPlaces);
			//DIMTOFL B 172
			this._writer.WriteBit(dimStyle.TextOutsideExtensions);
			//DIMSAH B 173
			this._writer.WriteBit(dimStyle.SeparateArrowBlocks);
			//DIMTIX B 174
			this._writer.WriteBit(dimStyle.TextInsideExtensions);
			//DIMSOXD B 175
			this._writer.WriteBit(dimStyle.SuppressOutsideExtensions);
			//DIMCLRD BS 176
			this._writer.WriteCmColor(dimStyle.DimensionLineColor);
			//DIMCLRE BS 177
			this._writer.WriteCmColor(dimStyle.ExtensionLineColor);
			//DIMCLRT BS 178
			this._writer.WriteCmColor(dimStyle.TextColor);
			//DIMADEC BS 179
			this._writer.WriteBitShort(dimStyle.AngularDecimalPlaces);
			//DIMDEC BS 271
			this._writer.WriteBitShort(dimStyle.DecimalPlaces);
			//DIMTDEC BS 272
			this._writer.WriteBitShort(dimStyle.ToleranceDecimalPlaces);
			//DIMALTU BS 273
			this._writer.WriteBitShort((short)dimStyle.AlternateUnitFormat);
			//DIMALTTD BS 274
			this._writer.WriteBitShort(dimStyle.AlternateUnitToleranceDecimalPlaces);
			//DIMAUNIT BS 275
			this._writer.WriteBitShort((short)dimStyle.AngularUnit);
			//DIMFRAC BS 276
			this._writer.WriteBitShort((short)dimStyle.FractionFormat);
			//DIMLUNIT BS 277
			this._writer.WriteBitShort((short)dimStyle.LinearUnitFormat);
			//DIMDSEP BS 278
			this._writer.WriteBitShort((short)dimStyle.DecimalSeparator);
			//DIMTMOVE BS 279
			this._writer.WriteBitShort((short)dimStyle.TextMovement);
			//DIMJUST BS 280
			this._writer.WriteBitShort((short)dimStyle.TextHorizontalAlignment);
			//DIMSD1 B 281
			this._writer.WriteBit(dimStyle.SuppressFirstDimensionLine);
			//DIMSD2 B 282
			this._writer.WriteBit(dimStyle.SuppressSecondDimensionLine);
			//DIMTOLJ BS 283
			this._writer.WriteBitShort((short)dimStyle.ToleranceAlignment);
			//DIMTZIN BS 284
			this._writer.WriteBitShort((short)dimStyle.ToleranceZeroHandling);
			//DIMALTZ BS 285
			this._writer.WriteBitShort((short)dimStyle.AlternateUnitZeroHandling);
			//DIMALTTZ BS 286
			this._writer.WriteBitShort((short)dimStyle.AlternateUnitToleranceZeroHandling);
			//DIMUPT B 288
			this._writer.WriteBit(dimStyle.CursorUpdate);
			//DIMFIT BS 287
			this._writer.WriteBitShort(3);
		}

		//R2007+:
		if (this.R2007Plus)
		{
			//DIMFXLON B 290
			this._writer.WriteBit(dimStyle.IsExtensionLineLengthFixed);
		}

		//R2010+:
		if (this.R2010Plus)
		{
			//DIMTXTDIRECTION B 295
			this._writer.WriteBit(dimStyle.TextDirection == TextDirection.RightToLeft);
			//DIMALTMZF BD ?
			this._writer.WriteBitDouble(dimStyle.AltMzf);
			//DIMALTMZS T ?
			this._writer.WriteVariableText(dimStyle.AltMzs);
			//DIMMZF BD ?
			this._writer.WriteBitDouble(dimStyle.Mzf);
			//DIMMZS T ?
			this._writer.WriteVariableText(dimStyle.Mzs);
		}

		//R2000+:
		if (this.R2000Plus)
		{
			//DIMLWD BS 371
			this._writer.WriteBitShort((short)dimStyle.DimensionLineWeight);
			//DIMLWE BS 372
			this._writer.WriteBitShort((short)dimStyle.ExtensionLineWeight);
		}

		//Common:
		//Unknown B 70 Seems to set the 0 - bit(1) of the 70 - group.
		this._writer.WriteBit(false);

		//Handle refs H Dimstyle control(soft pointer)
		//[Reactors(soft pointer)]
		//xdicobjhandle(hard owner)

		//External reference block handle(hard pointer)
		this._writer.HandleReference(DwgReferenceType.HardPointer, 0);

		//340 shapefile(DIMTXSTY)(hard pointer)
		this._writer.HandleReference(DwgReferenceType.HardPointer, dimStyle.Style);

		//R2000+:
		if (this.R2000Plus)
		{
			//341 leader block(DIMLDRBLK) (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, dimStyle.LeaderArrow);
			//342 dimblk(DIMBLK)(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, dimStyle.ArrowBlock);
			//343 dimblk1(DIMBLK1)(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, dimStyle.DimArrow1);
			//344 dimblk2(DIMBLK2)(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, dimStyle.DimArrow2);
		}

		//R2007+:
		if (this.R2007Plus)
		{
			//345 dimltype(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, dimStyle.LineType);
			//346 dimltex1(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, dimStyle.LineTypeExt1);
			//347 dimltex2(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, dimStyle.LineTypeExt2);
		}

		this.registerObject(dimStyle);
	}

	private void writeEntityMode(Entity entity)
	{
		//FE: Entity mode(entmode). Generally, this indicates whether or not the owner
		//relative handle reference is present.The values go as follows:

		//00 : The owner relative handle reference is present.
		//Applies to the following:
		//VERTEX, ATTRIB, and SEQEND.
		//BLOCK, ENDBLK, and the defining entities in all
		//block defs except *MODEL_SPACE and *PAPER_SPACE.

		//01 : PSPACE entity without a owner relative handle ref.
		//10 : MSPACE entity without a owner relative handle ref.
		//11 : Not used.
		byte entmode = this.getEntMode(entity);
		this._writer.Write2Bits(entmode);
		if (entmode == 0)
		{
			this._writer.HandleReference(DwgReferenceType.SoftPointer, entity.Owner);
		}

		this.writeReactorsAndDictionaryHandle(entity);

		//R13-R14 Only:
		if (this.R13_14Only)
		{
			//8 LAYER (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, entity.Layer);

			//Isbylayerlt B 1 if bylayer linetype, else 0
			bool isbylayerlt = entity.LineType.Name == LineType.ByLayerName;
			this._writer.WriteBit(isbylayerlt);
			if (isbylayerlt)
				//6 [LTYPE (hard pointer)] (present if Isbylayerlt is 0)
				this._writer.HandleReference(DwgReferenceType.HardPointer, entity.LineType);
		}

		//R13-R2000 Only:
		//previous/next handles present if Nolinks is 0.
		//Nolinks B 1 if major links are assumed +1, -1, else 0 For R2004+this always has value 1 (links are not used)
		if (!this.R2004Plus)
		{
			bool hasLinks = this._prev != null
						&& this._prev.Handle == entity.Handle - 1
						&& this._next != null
						&& this._next.Handle == entity.Handle + 1;

			this._writer.WriteBit(hasLinks);

			if (!hasLinks)
			{
				//[PREVIOUS ENTITY (relative soft pointer)]
				this._writer.HandleReference(DwgReferenceType.SoftPointer, this._prev);
				//[NEXT ENTITY (relative soft pointer)]
				this._writer.HandleReference(DwgReferenceType.SoftPointer, this._next);
			}
		}

		//Color	CMC(B)	62
		this._writer.WriteEnColor(entity.Color, entity.Transparency, entity.BookColor != null);

		//R2004+:
		if ((this._version >= ACadVersion.AC1018) && entity.BookColor != null)
		{
			//[Color book color handle (hard pointer)]
			this._writer.HandleReference(DwgReferenceType.HardPointer, entity.BookColor);
		}

		//Ltype scale	BD	48
		this._writer.WriteBitDouble(entity.LineTypeScale);

		if (!(this._version >= ACadVersion.AC1015))
		{
			//Common:
			//Invisibility BS 60
			this._writer.WriteBitShort((short)(entity.IsInvisible ? 1 : 0));

			return;
		}

		//R2000+:
		//8 LAYER (hard pointer)
		this._writer.HandleReference(DwgReferenceType.HardPointer, entity.Layer);

		if (entity.LineType.Name == LineType.ByLayerName)
		{
			//Ltype flags BB 00 = bylayer,
			this._writer.Write2Bits(0b00);
		}
		else if (entity.LineType.Name == LineType.ByBlockName)
		{
			//01 = byblock,
			this._writer.Write2Bits(0b01);
		}
		else if (entity.LineType.Name == LineType.ContinuousName)
		{
			//10 = continous,
			this._writer.Write2Bits(0b10);
		}
		else
		{
			//11 = linetype handle present at end of object
			this._writer.Write2Bits(0b11);
			//6 [LTYPE (hard pointer)] present if linetype flags were 11
			this._writer.HandleReference(DwgReferenceType.HardPointer, entity.LineType);
		}

		//R2007+:
		if (this.R2007Plus)
		{
			//Material flags BB 00 = bylayer, 01 = byblock, 11 = material handle present
			if (entity.Material == null)
			{
				this._writer.Write2Bits(0b00);
			}
			else
			{
				this._writer.Write2Bits(0b11);
				this._writer.HandleReference(DwgReferenceType.HardPointer, entity.Material);
			}

			//Shadow flags RC
			this._writer.WriteByte(0);
		}

		//R2000 +:
		//Plotstyle flags	BB	00 = bylayer, 01 = byblock, 11 = plotstyle handle present at end of object
		this._writer.Write2Bits(0b00);
		{
			//PLOTSTYLE (hard pointer) present if plotstyle flags were 11
		}

		//R2007 +:
		if (this._version > ACadVersion.AC1021)
		{
			//Material flags BB 00 = bylayer, 01 = byblock, 11 = material handle present at end of object
			this._writer.WriteBit(false);
			{
				//If has full visual style, the full visual style handle (hard pointer).
			}
			this._writer.WriteBit(false);
			{
				//If has full visual style, the full visual style handle (hard pointer).
			}
			//Shadow flags RC
			this._writer.WriteBit(false);
			{
				//If has full visual style, the full visual style handle (hard pointer).
			}
		}

		//Common:
		//Invisibility BS 60
		this._writer.WriteBitShort((short)(entity.IsInvisible ? 1 : 0));

		//R2000+:
		//Lineweight RC 370
		this._writer.WriteByte(CadUtils.ToIndex(entity.LineWeight));
	}

	private void writeEntries<T>(Table<T> table)
		where T : TableEntry
	{
		foreach (var entry in table)
		{
			switch (entry)
			{
				case AppId app:
					this.writeAppId(app);
					break;
				case BlockRecord blkRecord:
					this.writeBlockRecord(blkRecord);
					break;
				case Layer layer:
					this.writeLayer(layer);
					break;
				case LineType ltype:
					this.writeLineType(ltype);
					break;
				case TextStyle tstyle:
					this.writeTextStyle(tstyle);
					break;
				case UCS ucs:
					this.writeUCS(ucs);
					break;
				case View view:
					this.writeView(view);
					break;
				case DimensionStyle dstyle:
					this.writeDimensionStyle(dstyle);
					break;
				case VPort vport:
					this.writeVPort(vport);
					break;
				default:
					this.notify($"Table entry not implemented : {entry.GetType().FullName}", NotificationType.NotImplemented);
					break;
			}
		}
	}

	private void writeExtendedData(ExtendedDataDictionary data)
	{
		if (this.WriteXData)
		{
			//EED size BS size of extended entity data, if any
			foreach (var item in data)
			{
				this.writeExtendedDataEntry(item.Key, item.Value);
			}
		}

		this._writer.WriteBitShort(0);
	}

	private void writeExtendedDataEntry(AppId app, ExtendedData entry)
	{
		using (MemoryStream mstream = new MemoryStream())
		{
			foreach (ExtendedDataRecord record in entry.Records)
			{
				//Each data item has a 1-byte code (DXF group code minus 1000) followed by the value.
				mstream.WriteByte((byte)(record.Code - 1000));

				switch (record)
				{
					case ExtendedDataBinaryChunk binaryChunk:
						mstream.WriteByte((byte)binaryChunk.Value.Length);
						mstream.Write(binaryChunk.Value, 0, binaryChunk.Value.Length);
						break;
					case ExtendedDataControlString control:
						mstream.WriteByte((byte)(control.Value == '}' ? 1 : 0));
						break;
					case ExtendedDataInteger16 s16:
						mstream.Write(LittleEndianConverter.Instance.GetBytes(s16.Value), 0, 2);
						break;
					case ExtendedDataInteger32 s32:
						mstream.Write(LittleEndianConverter.Instance.GetBytes(s32.Value), 0, 4);
						break;
					case ExtendedDataReal real:
						mstream.Write(LittleEndianConverter.Instance.GetBytes(real.Value), 0, 8);
						break;
					case ExtendedDataScale scale:
						mstream.Write(LittleEndianConverter.Instance.GetBytes(scale.Value), 0, 8);
						break;
					case ExtendedDataDistance dist:
						mstream.Write(LittleEndianConverter.Instance.GetBytes(dist.Value), 0, 8);
						break;
					case ExtendedDataDirection dir:
						mstream.Write(LittleEndianConverter.Instance.GetBytes(dir.Value.X), 0, 8);
						mstream.Write(LittleEndianConverter.Instance.GetBytes(dir.Value.Y), 0, 8);
						mstream.Write(LittleEndianConverter.Instance.GetBytes(dir.Value.Z), 0, 8);
						break;
					case ExtendedDataDisplacement disp:
						mstream.Write(LittleEndianConverter.Instance.GetBytes(disp.Value.X), 0, 8);
						mstream.Write(LittleEndianConverter.Instance.GetBytes(disp.Value.Y), 0, 8);
						mstream.Write(LittleEndianConverter.Instance.GetBytes(disp.Value.Z), 0, 8);
						break;
					case ExtendedDataCoordinate coord:
						mstream.Write(LittleEndianConverter.Instance.GetBytes(coord.Value.X), 0, 8);
						mstream.Write(LittleEndianConverter.Instance.GetBytes(coord.Value.Y), 0, 8);
						mstream.Write(LittleEndianConverter.Instance.GetBytes(coord.Value.Z), 0, 8);
						break;
					case ExtendedDataWorldCoordinate wcoord:
						mstream.Write(LittleEndianConverter.Instance.GetBytes(wcoord.Value.X), 0, 8);
						mstream.Write(LittleEndianConverter.Instance.GetBytes(wcoord.Value.Y), 0, 8);
						mstream.Write(LittleEndianConverter.Instance.GetBytes(wcoord.Value.Z), 0, 8);
						break;
					case IExtendedDataHandleReference handle:
						ulong h = handle.Value;
						if (handle.ResolveReference(this._document) == null)
						{
							h = 0;
						}
						mstream.Write(BigEndianConverter.Instance.GetBytes(h), 0, 8);
						break;
					case ExtendedDataString str:
						//same as ReadTextUnicode()
						if (this.R2007Plus)
						{
							mstream.Write(LittleEndianConverter.Instance.GetBytes((ushort)str.Value.Length + 1), 0, 2);
							byte[] bytes = Encoding.Unicode.GetBytes(str.Value);

							mstream.Write(bytes, 0, bytes.Length);
							mstream.WriteByte(0);
							mstream.WriteByte(0);
						}
						else
						{
							var encodingIndex = CadUtils.GetCodeIndex((CSUtilities.Text.CodePage)this._writer.Encoding.CodePage);
							byte[] bytes = this._writer.Encoding.GetBytes(string.IsNullOrEmpty(str.Value) ? string.Empty : str.Value);

							mstream.Write(LittleEndianConverter.Instance.GetBytes((ushort)str.Value.Length), 0, 2);
							mstream.WriteByte((byte)encodingIndex);
							mstream.Write(bytes, 0, bytes.Length);
						}
						break;
					default:
						throw new System.NotSupportedException($"ExtendedDataRecord of type {record.GetType().FullName} not supported.");
				}
			}

			this._writer.WriteBitShort((short)mstream.Length);

			this._writer.Main.HandleReference(DwgReferenceType.HardPointer, app.Handle);

			this._writer.WriteBytes(mstream.GetBuffer(), 0, (int)mstream.Length);
		}
	}

	private void writeLayer(Layer layer)
	{
		this.writeCommonNonEntityData(layer);

		//Common:
		//Entry name TV 2
		this._writer.WriteVariableText(layer.Name);

		this.writeXrefDependantBit(layer);

		//R13-R14 Only:
		if (this.R13_14Only)
		{
			//Frozen B 70 if frozen (1 bit)
			this._writer.WriteBit(layer.Flags.HasFlag(LayerFlags.Frozen));
			//On B 1 if off (bit set = off)
			this._writer.WriteBit(!layer.IsOn);
			//Frz in new B 70 if frozen by default in new viewports (2 bit)
			this._writer.WriteBit(layer.Flags.HasFlag(LayerFlags.FrozenNewViewports));
			//Locked B 70 if locked (4 bit)
			this._writer.WriteBit(layer.Flags.HasFlag(LayerFlags.Locked));
		}

		//R2000+:
		if (this.R2000Plus)
		{
			//and lineweight (mask with 0x03E0)
			short values = (short)(CadUtils.ToIndex(layer.LineWeight) << 5);

			//contains frozen (1 bit),
			if (layer.Flags.HasFlag(LayerFlags.Frozen))
				values |= 0b1;

			//on (2 bit)
			if (!layer.IsOn)
				values |= 0b10;

			//frozen by default in new viewports (4 bit)
			if (layer.Flags.HasFlag(LayerFlags.Frozen))
				values |= 0b100;

			//locked (8 bit)
			if (layer.Flags.HasFlag(LayerFlags.Locked))
				values |= 0b1000;

			//plotting flag (16 bit),
			if (layer.PlotFlag)
				values |= 0b10000;

			//Values BS 70,290,370
			this._writer.WriteBitShort(values);
		}

		//Common:
		//Color CMC 62
		this._writer.WriteCmColor(layer.Color);

		//External reference block handle(hard pointer)
		this._writer.HandleReference(DwgReferenceType.HardPointer, null);

		//R2000+:
		if (this.R2000Plus)
		{
			//H 390 Plotstyle (hard pointer), by default points to PLACEHOLDER with handle 0x0f.
			this._writer.HandleReference(DwgReferenceType.HardPointer, 0);
		}

		//R2007+:
		if (this.R2007Plus)
		{
			//H 347 Material
			this._writer.HandleReference(DwgReferenceType.HardPointer, 0);
		}

		//Common:
		//H 6 linetype (hard pointer)
		this._writer.HandleReference(DwgReferenceType.HardPointer, layer.LineType.Handle);

		if (R2013Plus)
		{
			//H Unknown handle (hard pointer). Always seems to be NULL.
			this._writer.HandleReference(DwgReferenceType.HardPointer, 0);
		}

		this.registerObject(layer);
	}

	private void writeLineType(LineType ltype)
	{
		this.writeCommonNonEntityData(ltype);

		//Common:
		//Entry name TV 2
		this._writer.WriteVariableText(ltype.Name);

		this.writeXrefDependantBit(ltype);

		//Description TV 3
		this._writer.WriteVariableText(ltype.Description);
		//Pattern Len BD 40
		this._writer.WriteBitDouble(ltype.PatternLength);
		//Alignment RC 72 Always 'A'.
		this._writer.WriteByte((byte)ltype.Alignment);

		//Numdashes RC 73 The number of repetitions of the 49...74 data.
		this._writer.WriteByte((byte)ltype.Segments.Count());

		bool hasTextSegments = false;
		foreach (LineType.Segment segment in ltype.Segments)
		{
			if (segment.Flags.HasFlag(LineTypeShapeFlags.Text))
			{
				hasTextSegments = true;
				break;
			}
		}

		Encoding textEncoding = this.R2007Plus ? Encoding.Unicode : this._writer.Encoding;

		byte[] textArea = null;
		int textCursor = 0;
		byte[] textTerminator = textEncoding.GetBytes("\0");

		if (this._version <= ACadVersion.AC1018)
		{
			textArea = new byte[256];
			if (this._version <= ACadVersion.AC1014)
				textCursor = 1;
		}
		else if (this.R2007Plus && hasTextSegments)
		{
			textArea = new byte[512];
		}

		foreach (LineType.Segment segment in ltype.Segments)
		{
			if (segment.Flags.HasFlag(LineTypeShapeFlags.Text))
			{
				if (textArea == null || string.IsNullOrEmpty(segment.Text))
				{
					segment.ShapeNumber = 0;
				}
				else
				{
					byte[] textBytes = textEncoding.GetBytes(segment.Text);
					int required = textBytes.Length + textTerminator.Length;

					if (textCursor + required <= textArea.Length)
					{
						segment.ShapeNumber = (short)textCursor;
						Buffer.BlockCopy(textBytes, 0, textArea, textCursor, textBytes.Length);
						textCursor += textBytes.Length;
						Buffer.BlockCopy(textTerminator, 0, textArea, textCursor, textTerminator.Length);
						textCursor += textTerminator.Length;
					}
					else
					{
						segment.ShapeNumber = 0;
					}
				}
			}

			//Dash length BD 49 Dash or dot specifier.
			this._writer.WriteBitDouble(segment.Length);
			//Complex shapecode BS 75 Shape number if shapeflag is 2, or index into the string area if shapeflag is 4.
			this._writer.WriteBitShort(segment.ShapeNumber);

			//X - offset RD 44 (0.0 for a simple dash.)
			//Y - offset RD 45(0.0 for a simple dash.)
			this._writer.WriteRawDouble(segment.Offset.X);
			this._writer.WriteRawDouble(segment.Offset.Y);

			//Scale BD 46 (1.0 for a simple dash.)
			this._writer.WriteBitDouble(segment.Scale);
			//Rotation BD 50 (0.0 for a simple dash.)
			this._writer.WriteBitDouble(segment.Rotation);
			//Shapeflag BS 74 bit coded:
			this._writer.WriteBitShort((short)segment.Flags);
		}

		//R2004 and earlier:
		if (this._version <= ACadVersion.AC1018)
		{
			byte[] buffer = textArea ?? new byte[256];
			for (int i = 0; i < buffer.Length; i++)
			{
				this._writer.WriteByte(buffer[i]);
			}
		}

		//R2007+:
		if (this.R2007Plus && hasTextSegments)
		{
			byte[] buffer = textArea ?? new byte[512];
			for (int i = 0; i < buffer.Length; i++)
			{
				this._writer.WriteByte(buffer[i]);
			}
		}

		//Common:
		//External reference block handle(hard pointer)
		this._writer.HandleReference(DwgReferenceType.HardPointer, 0);

		foreach (var segment in ltype.Segments)
		{
			//340 shapefile for dash/shape (1 each) (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, segment.Style);
		}

		this.registerObject(ltype);
	}

	private void writeLTypeControlObject()
	{
		this.writeCommonNonEntityData(this._document.LineTypes);

		//Common:
		//Numentries BL 70
		this._writer.WriteBitLong(this._document.LineTypes.Count - 2);

		foreach (LineType item in this._document.LineTypes)
		{
			if (item.Name.Equals(LineType.ByBlockName, StringComparison.OrdinalIgnoreCase)
				|| item.Name.Equals(LineType.ByLayerName, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			//numentries handles in the file (soft owner)
			this._writer.HandleReference(DwgReferenceType.SoftOwnership, item);
		}

		//the linetypes, ending with BYLAYER and BYBLOCK.
		this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.LineTypes.ByBlock);
		this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.LineTypes.ByLayer);

		this.registerObject(this._document.LineTypes);

		this.writeEntries(this._document.LineTypes);
	}

	private void writeReactorsAndDictionaryHandle(CadObject cadObject)
	{
		//Numreactors S number of reactors in this object
		cadObject.CleanReactors();

		this._writer.WriteBitLong(cadObject.Reactors.Count());
		foreach (var item in cadObject.Reactors)
		{
			//[Reactors (soft pointer)]
			this._writer.HandleReference(DwgReferenceType.SoftPointer, item);
		}

		bool noDictionary = cadObject.XDictionary == null;

		//R2004+:
		if (this.R2004Plus)
		{
			this._writer.WriteBit(noDictionary);
			if (!noDictionary)
			{
				this._writer.HandleReference(DwgReferenceType.HardOwnership, cadObject.XDictionary);
			}
		}
		else
		{
			//xdicobjhandle(hard owner)
			this._writer.HandleReference(DwgReferenceType.HardOwnership, cadObject.XDictionary);
		}

		//R2013+:
		if (this.R2013Plus)
		{
			//Has DS binary data B If 1 then this object has associated binary data stored in the data store
			this._writer.WriteBit(false);
		}

		if (!noDictionary)
		{
			this.enqueueValidObject(cadObject.XDictionary);
		}
	}

	private void writeSize(Stream stream, uint size)
	{
		// This value is only read in IDwgStreamReader.ReadModularShort()
		// this should do the trick to write the modular short

		if (size >= 0b1000000000000000)
		{
			stream.WriteByte((byte)(size & 0b11111111));
			stream.WriteByte((byte)(((size >> 8) & 0b1111111) | 0b10000000));
			stream.WriteByte((byte)((size >> 15) & 0b11111111));
			stream.WriteByte((byte)((size >> 23) & 0b11111111));
		}
		else
		{
			stream.WriteByte((byte)(size & 0b11111111));
			stream.WriteByte((byte)((size >> 8) & 0b11111111));
		}
	}

	private void writeSizeInBits(Stream stream, ulong size)
	{
		// This value is only read in IDwgStreamReader.ReadModularChar()
		// this should do the trick to write the modular char

		if (size == 0)
		{
			stream.WriteByte(0);
			return;
		}

		ulong shift = size >> 7;
		while (size != 0)
		{
			byte b = (byte)(size & 0b1111111);
			if (shift != 0)
			{
				b = (byte)(b | 0b10000000);
			}

			stream.WriteByte(b);
			size = shift;
			shift = size >> 7;
		}
	}

	private void writeTable<T>(Table<T> table)
		where T : TableEntry
	{
		this.writeCommonNonEntityData(table);

		//Common:
		//Numentries BL 70
		this._writer.WriteBitLong(table.Count);

		if (this.R2000Plus && table is DimensionStylesTable)
		{
			//Undocumented: this byte is found only in the DimensionStylesTable
			this._writer.WriteByte(0);
		}

		foreach (var item in table)
		{
			//numentries handles in the file (soft owner)
			this._writer.HandleReference(DwgReferenceType.SoftOwnership, item);
		}

		this.registerObject(table);

		this.writeEntries(table);
	}

	private void writeTextStyle(TextStyle style)
	{
		this.writeCommonNonEntityData(style);

		//Common:
		//Entry name TV 2
		if (style.IsShapeFile)
		{
			this._writer.WriteVariableText(string.Empty);
		}
		else
		{
			this._writer.WriteVariableText(style.Name);
		}

		this.writeXrefDependantBit(style);

		//shape file B 1 if a shape file rather than a font (1 bit)
		this._writer.WriteBit(style.Flags.HasFlag(StyleFlags.IsShape));

		//Vertical B 1 if vertical (4 bit of flag)
		this._writer.WriteBit(style.Flags.HasFlag(StyleFlags.VerticalText));
		//Fixed height BD 40
		this._writer.WriteBitDouble(style.Height);
		//Width factor BD 41
		this._writer.WriteBitDouble(style.Width);
		//Oblique ang BD 50
		this._writer.WriteBitDouble(style.ObliqueAngle);
		//Generation RC 71 Generation flags (not bit-pair coded).
		this._writer.WriteByte((byte)style.MirrorFlag);
		//Last height BD 42
		this._writer.WriteBitDouble(style.LastHeight);
		//Font name TV 3
		this._writer.WriteVariableText(style.Filename);
		//Bigfont name TV 4
		this._writer.WriteVariableText(style.BigFontFilename);

		this._writer.HandleReference(DwgReferenceType.HardPointer, this._document.TextStyles);

		this.registerObject(style);
	}

	private void writeUCS(UCS ucs)
	{
		this.writeCommonNonEntityData(ucs);

		//Common:
		//Entry name TV 2
		this._writer.WriteVariableText(ucs.Name);

		this.writeXrefDependantBit(ucs);

		//Origin 3BD 10
		this._writer.Write3BitDouble(ucs.Origin);
		//X - direction 3BD 11
		this._writer.Write3BitDouble(ucs.XAxis);
		//Y - direction 3BD 12
		this._writer.Write3BitDouble(ucs.YAxis);

		//R2000+:
		if (this.R2000Plus)
		{
			//Elevation BD 146
			this._writer.WriteBitDouble(ucs.Elevation);
			//OrthographicViewType BS 79	//dxf docs: 79	Always 0
			this._writer.WriteBitShort((short)ucs.OrthographicViewType);
			//OrthographicType BS 71
			this._writer.WriteBitShort((short)ucs.OrthographicType);
		}

		//Common:
		//Handle refs H ucs control object (soft pointer)
		this._writer.HandleReference(DwgReferenceType.SoftPointer, this._document.UCSs);

		//R2000 +:
		if (this.R2000Plus)
		{
			//Base UCS Handle H 346 hard pointer
			this._writer.HandleReference(DwgReferenceType.HardPointer, 0);
			//Named UCS Handle H -hard pointer, not present in DXF
			this._writer.HandleReference(DwgReferenceType.HardPointer, 0);
		}

		this.registerObject(ucs);
	}

	private void writeView(View view)
	{
		this.writeCommonNonEntityData(view);

		//Common:
		//Entry name TV 2
		this._writer.WriteVariableText(view.Name);

		this.writeXrefDependantBit(view);

		//View height BD 40
		this._writer.WriteBitDouble(view.Height);
		//View width BD 41
		this._writer.WriteBitDouble(view.Width);
		//View center 2RD 10(Not bit - pair coded.)
		this._writer.Write2RawDouble(view.Center);
		//Target 3BD 12
		this._writer.Write3BitDouble(view.Target);
		//View dir 3BD 11 DXF doc suggests from target toward camera.
		this._writer.Write3BitDouble(view.Direction);
		//Twist angle BD 50 Radians
		this._writer.WriteBitDouble(view.Angle);
		//Lens length BD 42
		this._writer.WriteBitDouble(view.LensLength);
		//Front clip BD 43
		this._writer.WriteBitDouble(view.FrontClipping);
		//Back clip BD 44
		this._writer.WriteBitDouble(view.BackClipping);

		//View mode X 71 4 bits: 0123
		//Note that only bits 0, 1, 2, and 4 of the 71 can be specified -- not bit 3 (8).
		//0 : 71's bit 0 (1)
		this._writer.WriteBit(view.ViewMode.HasFlag(ViewModeType.PerspectiveView));
		//1 : 71's bit 1 (2)
		this._writer.WriteBit(view.ViewMode.HasFlag(ViewModeType.FrontClipping));
		//2 : 71's bit 2 (4)
		this._writer.WriteBit(view.ViewMode.HasFlag(ViewModeType.BackClipping));
		//3 : OPPOSITE of 71's bit 4 (16)
		this._writer.WriteBit(view.ViewMode.HasFlag(ViewModeType.FrontClippingZ));

		//R2000+:
		if (this.R2000Plus)
		{
			//Render Mode RC 281
			this._writer.WriteByte((byte)view.RenderMode);
		}

		//R2007+:
		if (this.R2007Plus)
		{
			//Use default lights B ? Default value is true
			this._writer.WriteBit(true);
			//Default lighting RC ? Default value is 1
			this._writer.WriteByte(1);
			//Brightness BD ? Default value is 0
			this._writer.WriteBitDouble(0.0);
			//Contrast BD ? Default value is 0
			this._writer.WriteBitDouble(0.0);
			//Abient color CMC? Default value is indexed color 250
			this._writer.WriteCmColor(new Color(250));
		}

		//Common:
		//Pspace flag B 70 Bit 0(1) of the 70 - group.
		this._writer.WriteBit(view.Flags.HasFlag((StandardFlags)0b1));

		if (this.R2000Plus)
		{
			this._writer.WriteBit(view.IsUcsAssociated);
			if (view.IsUcsAssociated)
			{
				//Origin 3BD 10 This and next 4 R2000 items are present only if 72 value is 1.
				this._writer.Write3BitDouble(view.UcsOrigin);
				//X-direction 3BD 11
				this._writer.Write3BitDouble(view.UcsXAxis);
				//Y-direction 3BD 12
				this._writer.Write3BitDouble(view.UcsYAxis);
				//Elevation BD 146
				this._writer.WriteBitDouble(view.UcsElevation);
				//OrthographicViewType BS 79
				this._writer.WriteBitShort((short)view.UcsOrthographicType);
			}
		}

		//Common:
		//Handle refs H view control object (soft pointer)
		this._writer.HandleReference(DwgReferenceType.SoftPointer, this._document.Views);

		//R2007+:
		if (this.R2007Plus)
		{
			//Camera plottable B 73
			this._writer.WriteBit(view.IsPlottable);

			//Background handle H 332 soft pointer
			this._writer.HandleReference(DwgReferenceType.SoftPointer, 0);
			//Visual style H 348 hard pointer
			this._writer.HandleReference(DwgReferenceType.HardPointer, 0);
			//Sun H 361 hard owner
			this._writer.HandleReference(DwgReferenceType.HardOwnership, 0);
		}

		if (this.R2000Plus && view.IsUcsAssociated)
		{
			//TODO: Implement ucs reference for view
			//Base UCS Handle H 346 hard pointer
			this._writer.HandleReference(DwgReferenceType.HardPointer, 0);
			//Named UCS Handle H 345 hard pointer
			this._writer.HandleReference(DwgReferenceType.HardPointer, 0);
		}

		//R2007+:
		if (this.R2007Plus)
		{
			//Live section H 334 soft pointer
			this._writer.HandleReference(DwgReferenceType.SoftPointer, 0);
		}

		this.registerObject(view);
	}

	private void writeVPort(VPort vport)
	{
		this.writeCommonNonEntityData(vport);

		//Common:
		//Entry name TV 2
		this._writer.WriteVariableText(vport.Name);

		this.writeXrefDependantBit(vport);

		//View height BD 40
		this._writer.WriteBitDouble(vport.ViewHeight);
		//Aspect ratio BD 41 The number stored here is actually the aspect ratio times the view height (40),
		//so this number must be divided by the 40-value to produce the aspect ratio that entget gives.
		//(R13 quirk; R12 has just the aspect ratio.)
		this._writer.WriteBitDouble(vport.AspectRatio * vport.ViewHeight);
		//View Center 2RD 12 DCS. (If it's plan view, add the view target (17) to get the WCS coordinates.
		//Careful! Sometimes you have to SAVE/OPEN to update the .dwg file.) Note that it's WSC in R12.
		this._writer.Write2RawDouble(vport.Center);
		//View target 3BD 17
		this._writer.Write3BitDouble(vport.Target);
		//View dir 3BD 16
		this._writer.Write3BitDouble(vport.Direction);
		//View twist BD 51
		this._writer.WriteBitDouble(vport.TwistAngle);
		//Lens length BD 42
		this._writer.WriteBitDouble(vport.LensLength);
		//Front clip BD 43
		this._writer.WriteBitDouble(vport.FrontClippingPlane);
		//Back clip BD 44
		this._writer.WriteBitDouble(vport.BackClippingPlane);

		//View mode X 71 4 bits: 0123
		//Note that only bits 0, 1, 2, and 4 are given here; see UCSFOLLOW below for bit 3(8) of the 71.
		//0 : 71's bit 0 (1)
		this._writer.WriteBit(vport.ViewMode.HasFlag(ViewModeType.PerspectiveView));
		//1 : 71's bit 1 (2)
		this._writer.WriteBit(vport.ViewMode.HasFlag(ViewModeType.FrontClipping));
		//2 : 71's bit 2 (4)
		this._writer.WriteBit(vport.ViewMode.HasFlag(ViewModeType.BackClipping));
		//3 : OPPOSITE of 71's bit 4 (16)
		this._writer.WriteBit(vport.ViewMode.HasFlag(ViewModeType.FrontClippingZ));

		//R2000+:
		if (this.R2000Plus)
		{
			//Render Mode RC 281
			this._writer.WriteByte((byte)vport.RenderMode);
		}

		//R2007+:
		if (this.R2007Plus)
		{
			//Use default lights B 292
			this._writer.WriteBit(vport.UseDefaultLighting);
			//Default lighting type RC 282
			this._writer.WriteByte((byte)vport.DefaultLighting);
			//Brightness BD 141
			this._writer.WriteBitDouble(vport.Brightness);
			//Constrast BD 142
			this._writer.WriteBitDouble(vport.Contrast);
			//Ambient Color CMC 63
			this._writer.WriteCmColor(vport.AmbientColor);
		}

		//Common:
		//Lower left 2RD 10 In fractions of screen width and height.
		this._writer.Write2RawDouble(vport.BottomLeft);
		//Upper right 2RD 11 In fractions of screen width and height.
		this._writer.Write2RawDouble(vport.TopRight);

		//UCSFOLLOW B 71 UCSFOLLOW. Bit 3 (8) of the 71-group.
		this._writer.WriteBit(vport.ViewMode.HasFlag(ViewModeType.Follow));

		//Circle zoom BS 72 Circle zoom percent.
		this._writer.WriteBitShort(vport.CircleZoomPercent);

		//Fast zoom B 73
		this._writer.WriteBit(true);

		//UCSICON X 74 2 bits: 01
		//0 : 74's bit 0 (1)
		this._writer.WriteBit(vport.UcsIconDisplay.HasFlag(UscIconType.OnLower));

		//1 : 74's bit 1 (2)
		this._writer.WriteBit(vport.UcsIconDisplay.HasFlag(UscIconType.OnOrigin));

		//Grid on/off B 76
		this._writer.WriteBit(vport.ShowGrid);
		//Grd spacing 2RD 15
		this._writer.Write2RawDouble(vport.GridSpacing);
		//Snap on/off B 75
		this._writer.WriteBit(vport.SnapOn);

		//Snap style B 77
		this._writer.WriteBit(vport.IsometricSnap);

		//Snap isopair BS 78
		this._writer.WriteBitShort(vport.SnapIsoPair);
		//Snap rot BD 50
		this._writer.WriteBitDouble(vport.SnapRotation);
		//Snap base 2RD 13
		this._writer.Write2RawDouble(vport.SnapBasePoint);
		//Snp spacing 2RD 14
		this._writer.Write2RawDouble(vport.SnapSpacing);

		//R2000+:
		if (this.R2000Plus)
		{
			//Unknown B
			this._writer.WriteBit(false);

			//UCS per Viewport B 71
			this._writer.WriteBit(true);
			//UCS Origin 3BD 110
			this._writer.Write3BitDouble(vport.Origin);
			//UCS X Axis 3BD 111
			this._writer.Write3BitDouble(vport.XAxis);
			//UCS Y Axis 3BD 112
			this._writer.Write3BitDouble(vport.YAxis);
			//UCS Elevation BD 146
			this._writer.WriteBitDouble(vport.Elevation);
			//UCS Orthographic type BS 79
			this._writer.WriteBitShort((short)vport.OrthographicType);
		}

		//R2007+:
		if (this.R2007Plus)
		{
			//Grid flags BS 60
			this._writer.WriteBitShort((short)vport.GridFlags);
			//Grid major BS 61
			this._writer.WriteBitShort(vport.MinorGridLinesPerMajorGridLine);
		}

		//Common:
		//External reference block handle(hard pointer)
		this._writer.HandleReference(DwgReferenceType.HardPointer, 0);

		//R2007+:
		if (this.R2007Plus)
		{
			//Background handle H 332 soft pointer
			this._writer.HandleReference(DwgReferenceType.SoftPointer, 0);
			//Visual Style handle H 348 hard pointer
			this._writer.HandleReference(DwgReferenceType.HardPointer, 0);
			//Sun handle H 361 hard owner
			this._writer.HandleReference(DwgReferenceType.HardOwnership, 0);
		}

		//R2000+:
		if (this.R2000Plus)
		{
			if (vport.OrthographicType == OrthographicType.None)
			{
				//Named UCS Handle H 345 hard pointer
				this._writer.HandleReference(DwgReferenceType.HardPointer, vport.NamedUcs);
				//Base UCS Handle H 346 hard pointer
				this._writer.HandleReference(DwgReferenceType.HardPointer, 0);
			}
			else
			{
				//Named UCS Handle H 345 hard pointer
				this._writer.HandleReference(DwgReferenceType.HardPointer, 0);
				//Base UCS Handle H 346 hard pointer
				this._writer.HandleReference(DwgReferenceType.HardPointer, vport.BaseUcs);
			}
		}

		this.registerObject(vport);
	}

	private void writeXrefDependantBit(TableEntry entry)
	{
		if (this.R2007Plus)
		{
			//xrefindex+1 BS 70 subtract one from this value when read.
			//After that, -1 indicates that this reference did not come from an xref,
			//otherwise this value indicates the index of the blockheader for the xref from which this came.
			this._writer.WriteBitShort((short)(entry.Flags.HasFlag(StandardFlags.XrefDependent) ? 0b100000000 : 0));
		}
		else
		{
			//64-flag B 70 The 64-bit of the 70 group.
			this._writer.WriteBit(entry.Flags.HasFlag(StandardFlags.Referenced));

			//xrefindex + 1 BS 70 subtract one from this value when read.
			//After that, -1 indicates that this reference did not come from an xref,
			//otherwise this value indicates the index of the blockheader for the xref from which this came.
			this._writer.WriteBitShort(0);

			//Xdep B 70 dependent on an xref. (16 bit)
			this._writer.WriteBit(entry.Flags.HasFlag(StandardFlags.XrefDependent));
		}
	}
}