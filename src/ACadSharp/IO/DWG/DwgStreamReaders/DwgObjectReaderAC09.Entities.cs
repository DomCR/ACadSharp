using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using ACadSharp.XData;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG;

internal partial class DwgObjectReaderAC09
{
	private enum EntityType : byte
	{
		LINE = 1,
		POINT = 2,
		CIRCLE = 3,
		SHAPE = 4,
		REPEAT = 5,
		ENDREP = 6,
		TEXT = 7,
		ARC = 8,
		TRACE = 9,
		LOAD = 10,
		SOLID = 11,
		BLOCK = 12,
		ENDBLK = 13,
		INSERT = 14,
		ATTDEF = 15,
		ATTRIB = 16,
		SEQEND = 17,
		JUMP = 18,
		POLYLINE = 19,
		VERTEX = 20,
		LINE3D = 21,
		FACE3D = 22,
		DIMENSION = 23,
		VIEWPORT = 24,
	}

	//Flags of the common entity data
	private const byte _hasColor = 0x01;
	private const byte _hasLineType = 0x02;
	private const byte _hasElevation = 0x04;
	private const byte _hasThickness = 0x08;
	private const byte _hasHandle = 0x20;
	private const byte _hasExtraFlags = 0x40;
	private const byte _hasAttributes = 0x80;

	//Extra flags of the common entity data, R11+
	private const byte _hasExtendedData = 0x02;
	private const byte _hasViewport = 0x04;

	/// <summary>
	/// Insert or polyline that owns the next ATTRIB or VERTEX entities until the SEQEND.
	/// </summary>
	private ICadOwnerTemplate _complexOwner;

	private Color? _entityColor;

	private double _entityElevation;

	private ExtendedDataDictionaryTemplate _entityExtendedData;

	private byte _entityFlags;

	private short _entityLayer;

	private int? _entityLineType;

	/// <summary>
	/// Flags of the optional fields of each entity type.
	/// </summary>
	private ushort _entityOptions;

	private long _entityStart;

	private short _entitySize;

	private double _entityThickness;

	private bool _isPaperSpace;

	/// <summary>
	/// Extended data of the entity by the handle of its application.
	/// </summary>
	private class ExtendedDataDictionaryTemplate : Dictionary<ulong, List<ExtendedDataRecord>> { }

	private void readBlocks()
	{
		foreach (KeyValuePair<int, uint> item in this._blockAddresses)
		{
			if (!this._blockRecords.TryGetValue(item.Key, out CadBlockRecordTemplate template))
				continue;

			long start = this.resolveBlockAddress(item.Value);
			if (start < this._fileHeader.BlocksSection.Seeker
				|| start >= this._fileHeader.BlocksSection.Seeker + this._fileHeader.BlocksSection.Size)
			{
				this._builder.Notify($"Address of the block {template.CadObject.Name} is outside the blocks section", NotificationType.Warning);
				continue;
			}

			this.readEntities(start, -1, template);
		}
	}

	/// <summary>
	/// Read the entities from a position following the JUMP entities.
	/// </summary>
	/// <param name="start">Position of the first entity.</param>
	/// <param name="end">Position where the entities end, -1 for a block definition that ends with ENDBLK.</param>
	/// <param name="owner">Block record that owns the entities.</param>
	private void readEntities(long start, long end, CadBlockRecordTemplate owner)
	{
		HashSet<long> readed = new HashSet<long>();
		long position = start;

		//Position to continue at if the extra entities section ends without a JUMP back
		long returnPosition = -1;

		this._complexOwner = null;

		while (position != end)
		{
			if (!this.isInSection(position) || !readed.Add(position))
			{
				if (returnPosition > 0 && readed.Add(returnPosition))
				{
					position = returnPosition;
					returnPosition = -1;
					continue;
				}

				if (end > 0)
				{
					this._builder.Notify($"The entities end at {position} instead of {end}", NotificationType.Warning);
				}

				break;
			}

			this._reader.Position = position;

			//RC : Entity type, 0x80 for the deleted entities
			byte type = this._reader.ReadByte();
			//RC : Flags
			this._entityFlags = this._reader.ReadByte();
			//RS : Size of the entity
			this._entitySize = this._reader.ReadShort();
			this._entityStart = position;

			if (this._entitySize < 4 || position + this._entitySize > this._reader.Stream.Length)
			{
				this._builder.Notify($"Invalid size {this._entitySize} for the entity at {position}", NotificationType.Warning);
				break;
			}

			long next = position + this._entitySize;

			if ((type & 0x80) != 0)
			{
				//Deleted entity
				position = next;
				continue;
			}

			if ((EntityType)type == EntityType.JUMP)
			{
				//RL : Address of the next entity
				returnPosition = next;
				position = this.resolveJumpAddress((uint)this._reader.ReadInt());
				continue;
			}

			CadTemplate template = null;
			try
			{
				this.checkCrc(position, this._entitySize - 2, $"the entity at {position}");
				this._reader.Position = position + 4;

				template = this.readEntity((EntityType)type, owner);
			}
			catch (Exception ex)
			{
				if (!this._builder.Configuration.Failsafe)
					throw;

				this._builder.Notify($"Could not read the entity {(EntityType)type} at {position}", NotificationType.Error, ex);
			}

			if (template is CadEntityTemplate entity && entity.CadObject is BlockEnd && owner != this._modelSpaceTemplate)
			{
				break;
			}

			position = next;

			//The extra entities section ends with a JUMP back, if it is missing the entities continue after the first JUMP
			if (position == this._fileHeader.ExtrasSection.Seeker + this._fileHeader.ExtrasSection.Size && returnPosition > 0)
			{
				position = returnPosition;
				returnPosition = -1;
			}
		}

		this._complexOwner = null;
	}

	private bool isInSection(long position)
	{
		return this.isInSection(position, this._fileHeader.EntitiesSection)
			|| this.isInSection(position, this._fileHeader.BlocksSection)
			|| this.isInSection(position, this._fileHeader.ExtrasSection);
	}

	private bool isInSection(long position, FileHeaders.DwgSectionLocatorRecord section)
	{
		return section.Seeker > 0 && position >= section.Seeker && position < section.Seeker + section.Size;
	}

	private CadTemplate readEntity(EntityType type, CadBlockRecordTemplate owner)
	{
		this.readCommonEntityHeader(type);

		CadTemplate template;
		switch (type)
		{
			case EntityType.LINE:
			case EntityType.LINE3D:
				template = this.readLine(type);
				break;
			case EntityType.POINT:
				template = this.readPoint();
				break;
			case EntityType.CIRCLE:
				template = this.readCircle();
				break;
			case EntityType.SHAPE:
				template = this.readShape();
				break;
			case EntityType.TEXT:
				template = this.readText();
				break;
			case EntityType.ARC:
				template = this.readArc();
				break;
			case EntityType.TRACE:
			case EntityType.SOLID:
				template = this.readSolid();
				break;
			case EntityType.BLOCK:
				template = this.readBlock(owner);
				break;
			case EntityType.ENDBLK:
				template = this.readEndBlock(owner);
				break;
			case EntityType.INSERT:
				template = this.readInsert();
				break;
			case EntityType.ATTDEF:
				template = this.readAttributeDefinition();
				break;
			case EntityType.ATTRIB:
				template = this.readAttribute();
				break;
			case EntityType.SEQEND:
				template = this.readSeqend();
				break;
			case EntityType.POLYLINE:
				template = this.readPolyline();
				break;
			case EntityType.VERTEX:
				template = this.readVertex();
				break;
			case EntityType.FACE3D:
				template = this.read3dFace();
				break;
			case EntityType.DIMENSION:
				template = this.readDimension();
				break;
			case EntityType.VIEWPORT:
				template = this.readViewport();
				break;
			default:
				this._builder.Notify($"Entity not implemented: {type}", NotificationType.NotImplemented);
				return null;
		}

		this.checkEntityEnd(template);

		switch (template?.CadObject)
		{
			case null:
				return null;
			case Block:
			case BlockEnd:
				break;
			case Seqend:
				this._complexOwner = null;
				break;
			case Vertex:
			case AttributeEntity:
				//Owned by the polyline or the insert
				break;
			default:
				this._complexOwner = null;

				CadBlockRecordTemplate record = owner;
				if (owner == this._modelSpaceTemplate && this._isPaperSpace)
				{
					record = this._paperSpaceTemplate;
				}

				template.OwnerHandle = record.CadObject.Handle;
				record.OwnedObjectsHandlers.Add(template.CadObject.Handle);

				if (template is CadPolyLineTemplate || (template is CadInsertTemplate && (this._entityFlags & _hasAttributes) != 0))
				{
					this._complexOwner = (ICadOwnerTemplate)template;
				}
				break;
		}

		this._builder.AddTemplate(template);

		return template;
	}

	/// <summary>
	/// Read the data common to all the entities, the entity is not created yet.
	/// </summary>
	private void readCommonEntityHeader(EntityType type)
	{
		this._entityColor = null;
		this._entityLineType = null;
		this._entityElevation = 0;
		this._entityThickness = 0;
		this._entityExtendedData = null;
		this._isPaperSpace = false;

		//RS : Layer index 8
		this._entityLayer = this._reader.ReadShort();
		//RS : Flags of the optional fields
		this._entityOptions = (ushort)this._reader.ReadShort();

		byte extraFlags = 0;

		//R11+:
		if (this.R11Plus && (this._entityFlags & _hasExtraFlags) != 0)
		{
			//RC : Extra flags
			extraFlags = this._reader.ReadByte();

			if ((extraFlags & _hasExtendedData) != 0)
			{
				this.readExtendedData();
			}
		}

		if ((this._entityFlags & _hasColor) != 0)
		{
			//RC : Color 62
			this._entityColor = this.readColor(this._reader.ReadByte());
		}

		if ((this._entityFlags & _hasLineType) != 0)
		{
			//R11+:
			if (this.R11Plus)
			{
				//RS : Linetype index 6
				this._entityLineType = this._reader.ReadShort();
			}
			else
			{
				//RC : Linetype index 6
				this._entityLineType = this._reader.ReadByte();
			}
		}

		//LINE, POINT, 3DLINE and 3DFACE use the flag for the points without z, there is no elevation
		bool hasElevation = (this._entityFlags & _hasElevation) != 0
			&& type != EntityType.LINE
			&& type != EntityType.POINT
			&& type != EntityType.LINE3D
			&& type != EntityType.FACE3D;
		bool hasThickness = (this._entityFlags & _hasThickness) != 0;

		//R11+:
		if (this.R11Plus)
		{
			//RD : Thickness 39
			if (hasThickness)
				this._entityThickness = this._reader.ReadDouble();
			//RD : Elevation 38
			if (hasElevation)
				this._entityElevation = this._reader.ReadDouble();
		}
		else
		{
			//RD : Elevation 38
			if (hasElevation)
				this._entityElevation = this._reader.ReadDouble();
			//RD : Thickness 39
			if (hasThickness)
				this._entityThickness = this._reader.ReadDouble();
		}

		if ((this._entityFlags & _hasHandle) != 0)
		{
			//RC : Size of the handle
			byte size = this._reader.ReadByte();
			//Handle 5, the reader assigns its own handles
			this._reader.ReadBytes(size);
		}

		//R11+:
		if ((extraFlags & _hasViewport) != 0)
		{
			//RS : Viewport of the entity, the entities in paper space have it
			short viewport = this._reader.ReadShort();
			this._isPaperSpace = true;
		}
	}

	private void readCommonEntityData(CadEntityTemplate template)
	{
		template.CadObject.Handle = this.nextHandle();

		template.LayerHandle = this.getEntryHandle(DxfFileToken.TableLayer, this._entityLayer);

		if (this._entityColor.HasValue)
		{
			template.CadObject.Color = this._entityColor.Value;
		}

		if (this._entityLineType.HasValue)
		{
			template.LineTypeHandle = this.getEntryHandle(DxfFileToken.TableLinetype, this._entityLineType.Value);
			if (!template.LineTypeHandle.HasValue)
			{
				//0 : BYLAYER, 1 : BYBLOCK
				template.LtypeFlags = (byte)(DwgHeaderReaderAC09.IsByBlockLineType(this._version, this._entityLineType.Value) ? 1 : 0);
			}
		}

		if (this._entityExtendedData != null)
		{
			foreach (KeyValuePair<ulong, List<ExtendedDataRecord>> item in this._entityExtendedData)
			{
				template.EDataTemplate.Add(item.Key, item.Value);
			}
		}
	}

	private void readExtendedData()
	{
		//RS : Size of the extended data
		short size = this._reader.ReadShort();
		long end = this._reader.Position + size;

		this._entityExtendedData = new ExtendedDataDictionaryTemplate();
		List<ExtendedDataRecord> records = null;

		while (this._reader.Position < end)
		{
			//RC : Group code - 1000
			DxfCode code = (DxfCode)(1000 + this._reader.ReadByte());

			ExtendedDataRecord record;
			switch (code)
			{
				case DxfCode.ExtendedDataAsciiString:
					//RC : Length
					byte length = this._reader.ReadByte();
					record = new ExtendedDataString(this._reader.ReadFixedText(length));
					break;
				case DxfCode.ExtendedDataRegAppName:
					//RS : Application index, starts the records of the application
					short appIndex = this._reader.ReadShort();
					ulong appHandle = this.getEntryHandle(DxfFileToken.TableAppId, appIndex) ?? 0;
					if (!this._entityExtendedData.TryGetValue(appHandle, out records))
					{
						records = new List<ExtendedDataRecord>();
						this._entityExtendedData.Add(appHandle, records);
					}
					continue;
				case DxfCode.ExtendedDataControlString:
					//RC : 0 open, 1 close
					record = new ExtendedDataControlString(this._reader.ReadByte() != 0);
					break;
				case DxfCode.ExtendedDataLayerName:
					//RS : Layer index
					record = new ExtendedDataLayer(this.getEntryHandle(DxfFileToken.TableLayer, this._reader.ReadShort()) ?? 0);
					break;
				case DxfCode.ExtendedDataBinaryChunk:
					//RC : Length
					byte chunkLength = this._reader.ReadByte();
					record = new ExtendedDataBinaryChunk(this._reader.ReadBytes(chunkLength));
					break;
				case DxfCode.ExtendedDataHandle:
					//8 bytes : Handle, big endian
					ulong handle = 0;
					foreach (byte b in this._reader.ReadBytes(8))
					{
						handle = (handle << 8) | b;
					}
					record = new ExtendedDataHandle(handle);
					break;
				case DxfCode.ExtendedDataXCoordinate:
					record = new ExtendedDataCoordinate(this._reader.Read3RawDouble());
					break;
				case DxfCode.ExtendedDataWorldXCoordinate:
					record = new ExtendedDataWorldCoordinate(this._reader.Read3RawDouble());
					break;
				case DxfCode.ExtendedDataWorldXDisp:
					record = new ExtendedDataDisplacement(this._reader.Read3RawDouble());
					break;
				case DxfCode.ExtendedDataWorldXDir:
					record = new ExtendedDataDirection(this._reader.Read3RawDouble());
					break;
				case DxfCode.ExtendedDataReal:
					record = new ExtendedDataReal(this._reader.ReadDouble());
					break;
				case DxfCode.ExtendedDataDist:
					record = new ExtendedDataDistance(this._reader.ReadDouble());
					break;
				case DxfCode.ExtendedDataScale:
					record = new ExtendedDataScale(this._reader.ReadDouble());
					break;
				case DxfCode.ExtendedDataInteger16:
					record = new ExtendedDataInteger16(this._reader.ReadShort());
					break;
				case DxfCode.ExtendedDataInteger32:
					record = new ExtendedDataInteger32(this._reader.ReadInt());
					break;
				default:
					this._builder.Notify($"Unknown code for extended data: {code}", NotificationType.Warning);
					this._reader.Position = end;
					return;
			}

			records?.Add(record);
		}

		this._reader.Position = end;
	}

	/// <summary>
	/// Check that the fields of the entity end where its size says, only reported in the tests.
	/// </summary>
	private void checkEntityEnd(CadTemplate template)
	{
#if TEST
		long expected = this._entityStart + this._entitySize - (this.R11Plus ? 2 : 0);
		if (template != null && template is not CadPolyLineTemplate && this._reader.Position != expected)
		{
			this._builder.Notify($"The fields of {template.CadObject.GetType().Name} at {this._entityStart} end at {this._reader.Position} instead of {expected}, options {this._entityOptions:X4}", NotificationType.Warning);
		}
#endif
	}

	private bool hasOption(int flag)
	{
		return (this._entityOptions & flag) != 0;
	}

	private Color readColor(byte value)
	{
		//0 : BYBLOCK, without color the entity is BYLAYER
		if (value == 0)
		{
			return Color.ByBlock;
		}

		return new Color(value);
	}

	/// <summary>
	/// Read a point in the plane of the entity, the elevation is the z.
	/// </summary>
	private XYZ readPlanePoint()
	{
		XY xy = this._reader.Read2RawDouble();
		return new XYZ(xy.X, xy.Y, this._entityElevation);
	}

	/// <summary>
	/// Read a point of LINE, POINT, 3DLINE and 3DFACE, the flag for the elevation leaves the z out.
	/// </summary>
	private XYZ readPoint3D()
	{
		XY xy = this._reader.Read2RawDouble();
		double z = (this._entityFlags & _hasElevation) != 0 ? 0 : this._reader.ReadDouble();
		return new XYZ(xy.X, xy.Y, z);
	}

	private CadTemplate read3dFace()
	{
		Face3D face = new Face3D();
		CadEntityTemplate template = new CadEntityTemplate(face);

		this.readCommonEntityData(template);

		//2RD + RD : 1st corner 10
		face.FirstCorner = this.readPoint3D();
		//2RD + RD : 2nd corner 11
		face.SecondCorner = this.readPoint3D();
		//2RD + RD : 3rd corner 12
		face.ThirdCorner = this.readPoint3D();
		//2RD + RD : 4th corner 13
		face.FourthCorner = this.readPoint3D();

		//Option 0x01
		if (this.hasOption(0x01))
		{
			//RS : Invisible edges 70
			face.Flags = (InvisibleEdgeFlags)this._reader.ReadShort();
		}

		return template;
	}

	private CadTemplate readArc()
	{
		Arc arc = new Arc();
		CadEntityTemplate template = new CadEntityTemplate(arc);

		this.readCommonEntityData(template);

		//2RD : Center 10
		arc.Center = this.readPlanePoint();
		//RD : Radius 40
		arc.Radius = this._reader.ReadDouble();
		//RD : Start angle 50
		arc.StartAngle = this._reader.ReadDouble();
		//RD : End angle 51
		arc.EndAngle = this._reader.ReadDouble();
		arc.Thickness = this._entityThickness;

		//Option 0x01
		if (this.hasOption(0x01))
		{
			//3RD : Extrusion 210
			arc.Normal = this._reader.Read3RawDouble();
		}

		return template;
	}

	private CadTemplate readAttribute()
	{
		AttributeEntity att = new AttributeEntity();
		CadAttributeTemplate template = new CadAttributeTemplate(att);

		this.readCommonEntityData(template);
		this.readCommonAttData(template, att);

		if (this._complexOwner is CadInsertTemplate insert)
		{
			template.OwnerHandle = insert.CadObject.Handle;
			insert.OwnedObjectsHandlers.Add(att.Handle);
		}
		else
		{
			this._builder.Notify($"Attribute at {this._entityStart} without insert", NotificationType.Warning);
			return null;
		}

		return template;
	}

	private CadTemplate readAttributeDefinition()
	{
		AttributeDefinition attdef = new AttributeDefinition();
		CadAttributeTemplate template = new CadAttributeTemplate(attdef);

		this.readCommonEntityData(template);
		this.readCommonAttData(template, attdef);

		return template;
	}

	/// <summary>
	/// ATTDEF and ATTRIB, their optional fields are one flag after the ones of TEXT.
	/// </summary>
	private void readCommonAttData(CadAttributeTemplate template, AttributeBase att)
	{
		//2RD : Insertion point 10
		att.InsertPoint = this.readPlanePoint();
		//RD : Height 40
		att.Height = this._reader.ReadDouble();
		//TV : Default value 1
		att.Value = this._reader.ReadVariableText();

		if (att is AttributeDefinition attdef)
		{
			//TV : Prompt 3
			attdef.Prompt = this._reader.ReadVariableText();
		}

		//TV : Tag 2
		att.Tag = this._reader.ReadVariableText();
		//RC : Flags 70
		att.Flags = (AttributeFlags)this._reader.ReadByte();
		att.Thickness = this._entityThickness;

		//Option 0x02
		if (this.hasOption(0x02))
		{
			//RD : Rotation 50
			att.Rotation = this._reader.ReadDouble();
		}

		//Option 0x04
		if (this.hasOption(0x04))
		{
			//RD : Width factor 41
			att.WidthFactor = this._reader.ReadDouble();
		}

		//Option 0x08
		if (this.hasOption(0x08))
		{
			//RD : Oblique angle 51
			att.ObliqueAngle = this._reader.ReadDouble();
		}

		//Option 0x10
		//RC : Style index 7
		byte style = this.hasOption(0x10) ? this._reader.ReadByte() : (byte)0;
		template.StyleHandle = this.getEntryHandle(DxfFileToken.TableStyle, style);

		//Option 0x20
		if (this.hasOption(0x20))
		{
			//RC : Generation 71
			att.Mirror = (TextMirrorFlag)this._reader.ReadByte();
		}

		//Option 0x40
		if (this.hasOption(0x40))
		{
			//RC : Horizontal alignment 72
			att.HorizontalAlignment = (TextHorizontalAlignment)this._reader.ReadByte();
		}

		//Option 0x80
		if (this.hasOption(0x80))
		{
			//2RD : Alignment point 11
			att.AlignmentPoint = this.readPlanePoint();
		}

		//Option 0x100
		if (this.hasOption(0x100))
		{
			//3RD : Extrusion 210
			att.Normal = this._reader.Read3RawDouble();
		}

		//Option 0x200
		if (this.hasOption(0x200))
		{
			//RC : Vertical alignment 74
			att.VerticalAlignment = (TextVerticalAlignmentType)this._reader.ReadByte();
		}
	}

	private CadTemplate readBlock(CadBlockRecordTemplate owner)
	{
		Block block = new Block(new BlockRecord());
		CadEntityTemplate template = new CadEntityTemplate(block);

		this.readCommonEntityData(template);

		//2RD : Base point 10
		owner.CadObject.BlockEntity.BasePoint = this.readPlanePoint();

		//Option 0x02
		if (this.hasOption(0x02))
		{
			//TV : Xref path 1
			owner.CadObject.BlockEntity.XRefPath = this._reader.ReadVariableText();
		}

		//Option 0x04
		if (this.hasOption(0x04))
		{
			//TV : Block name 2, the name of the table is used
			string name = this._reader.ReadVariableText();
		}

		template.OwnerHandle = owner.CadObject.Handle;
		owner.BeginBlockHandle = block.Handle;

		return template;
	}

	private CadTemplate readCircle()
	{
		Circle circle = new Circle();
		CadEntityTemplate template = new CadEntityTemplate(circle);

		this.readCommonEntityData(template);

		//2RD : Center 10
		circle.Center = this.readPlanePoint();
		//RD : Radius 40
		circle.Radius = this._reader.ReadDouble();
		circle.Thickness = this._entityThickness;

		//Option 0x01
		if (this.hasOption(0x01))
		{
			//3RD : Extrusion 210
			circle.Normal = this._reader.Read3RawDouble();
		}

		return template;
	}

	private CadTemplate readDimension()
	{
		//RS : Block index 2
		short blockIndex = this._reader.ReadShort();
		//3RD : Definition point 10
		XYZ definitionPoint = this._reader.Read3RawDouble();
		//2RD : Text middle point 11
		XYZ textMiddlePoint = this.readPlanePoint();
		//Option 0x01, 2RD : Insertion point 12
		XYZ insertionPoint = this.hasOption(0x01) ? this.readPlanePoint() : XYZ.Zero;
		//Option 0x02, RC : Dimension type 70
		DimensionType flags = this.hasOption(0x02) ? (DimensionType)this._reader.ReadByte() : DimensionType.Linear;
		//Option 0x04, TV : Text 1
		string text = this.hasOption(0x04) ? this._reader.ReadVariableText() : string.Empty;
		//Option 0x08, 3RD : First definition point 13
		XYZ point13 = this.hasOption(0x08) ? this._reader.Read3RawDouble() : XYZ.Zero;
		//Option 0x10, 3RD : Second definition point 14
		XYZ point14 = this.hasOption(0x10) ? this._reader.Read3RawDouble() : XYZ.Zero;
		//Option 0x20, 3RD : Definition point 15
		XYZ point15 = this.hasOption(0x20) ? this._reader.Read3RawDouble() : XYZ.Zero;
		//Option 0x40, 2RD (R10: 3RD) : Arc definition point 16
		XYZ point16 = XYZ.Zero;
		if (this.hasOption(0x40))
		{
			point16 = this.R11Plus ? this.readPlanePoint() : this._reader.Read3RawDouble();
		}
		//Option 0x80, RD : Leader length 40
		double leaderLength = this.hasOption(0x80) ? this._reader.ReadDouble() : 0;
		//Option 0x100, RD : Rotation 50
		double rotation = this.hasOption(0x100) ? this._reader.ReadDouble() : 0;
		//Option 0x200, RD : Oblique angle 52
		double obliqueAngle = this.hasOption(0x200) ? this._reader.ReadDouble() : 0;
		//Option 0x400, RD : Text rotation 53
		double textRotation = this.hasOption(0x400) ? this._reader.ReadDouble() : 0;
		//Option 0x4000, 3RD : Extrusion 210
		XYZ normal = this.hasOption(0x4000) ? this._reader.Read3RawDouble() : XYZ.AxisZ;
		//Option 0x8000, RS : Dimension style index 3
		short styleIndex = this.hasOption(0x8000) ? this._reader.ReadShort() : (short)-1;

		Dimension dimension;
		switch ((DimensionType)((int)flags & 0x0F))
		{
			case DimensionType.Aligned:
				dimension = new DimensionAligned
				{
					FirstPoint = point13,
					SecondPoint = point14,
					ExtLineRotation = obliqueAngle,
				};
				break;
			case DimensionType.Angular:
				dimension = new DimensionAngular2Line
				{
					FirstPoint = point13,
					SecondPoint = point14,
					AngleVertex = point15,
					DimensionArc = point16,
				};
				break;
			case DimensionType.Diameter:
				dimension = new DimensionDiameter
				{
					AngleVertex = point15,
					LeaderLength = leaderLength,
				};
				break;
			case DimensionType.Radius:
				dimension = new DimensionRadius
				{
					AngleVertex = point15,
					LeaderLength = leaderLength,
				};
				break;
			case DimensionType.Angular3Point:
				dimension = new DimensionAngular3Pt
				{
					FirstPoint = point13,
					SecondPoint = point14,
					AngleVertex = point15,
				};
				break;
			case DimensionType.Ordinate:
				dimension = new DimensionOrdinate
				{
					FeatureLocation = point13,
					LeaderEndpoint = point14,
				};
				break;
			default:
				dimension = new DimensionLinear
				{
					FirstPoint = point13,
					SecondPoint = point14,
					Rotation = rotation,
					ExtLineRotation = obliqueAngle,
				};
				break;
		}

		CadDimensionTemplate template = new CadDimensionTemplate(dimension);

		this.readCommonEntityData(template);

		dimension.Flags = flags;
		dimension.DefinitionPoint = definitionPoint;
		dimension.TextMiddlePoint = textMiddlePoint;
		dimension.InsertionPoint = insertionPoint;
		dimension.Text = text;
		dimension.TextRotation = textRotation;
		dimension.Normal = normal;

		template.BlockHandle = this.getBlockHandle(blockIndex);
		template.StyleHandle = this.getEntryHandle(DxfFileToken.TableDimstyle, styleIndex);

		return template;
	}

	private CadTemplate readEndBlock(CadBlockRecordTemplate owner)
	{
		BlockEnd blockEnd = new BlockEnd(new BlockRecord());
		CadEntityTemplate template = new CadEntityTemplate(blockEnd);

		this.readCommonEntityData(template);

		template.OwnerHandle = owner.CadObject.Handle;
		owner.EndBlockHandle = blockEnd.Handle;

		return template;
	}

	private CadTemplate readInsert()
	{
		Insert insert = new Insert();
		CadInsertTemplate template = new CadInsertTemplate(insert);

		this.readCommonEntityData(template);

		//RS : Block index 2
		template.BlockHeaderHandle = this.getBlockHandle(this._reader.ReadShort());
		//2RD : Insertion point 10
		insert.InsertPoint = this.readPlanePoint();

		//Option 0x01
		if (this.hasOption(0x01))
		{
			//RD : X scale 41
			insert.XScale = this._reader.ReadDouble();
		}

		//Option 0x02
		if (this.hasOption(0x02))
		{
			//RD : Y scale 42
			insert.YScale = this._reader.ReadDouble();
		}

		//Option 0x04
		if (this.hasOption(0x04))
		{
			//RD : Rotation 50
			insert.Rotation = this._reader.ReadDouble();
		}

		//Option 0x08
		if (this.hasOption(0x08))
		{
			//RD : Z scale 43
			insert.ZScale = this._reader.ReadDouble();
		}

		//Option 0x10
		if (this.hasOption(0x10))
		{
			//RS : Column count 70
			insert.ColumnCount = (ushort)this._reader.ReadShort();
		}

		//Option 0x20
		if (this.hasOption(0x20))
		{
			//RS : Row count 71
			insert.RowCount = (ushort)this._reader.ReadShort();
		}

		//Option 0x40
		if (this.hasOption(0x40))
		{
			//RD : Column spacing 44
			insert.ColumnSpacing = this._reader.ReadDouble();
		}

		//Option 0x80
		if (this.hasOption(0x80))
		{
			//RD : Row spacing 45
			insert.RowSpacing = this._reader.ReadDouble();
		}

		//Option 0x100
		if (this.hasOption(0x100))
		{
			//3RD : Extrusion 210
			insert.Normal = this._reader.Read3RawDouble();
		}

		return template;
	}

	private CadTemplate readLine(EntityType type)
	{
		Line line = new Line();
		CadEntityTemplate template = new CadEntityTemplate(line);

		this.readCommonEntityData(template);

		//2RD + RD : Start point 10
		line.StartPoint = this.readPoint3D();
		//2RD + RD : End point 11
		line.EndPoint = this.readPoint3D();
		line.Thickness = this._entityThickness;

		//Option 0x01
		if (this.hasOption(0x01))
		{
			//3RD : Extrusion 210
			line.Normal = this._reader.Read3RawDouble();
		}

		return template;
	}

	private CadTemplate readPoint()
	{
		Point point = new Point();
		CadEntityTemplate template = new CadEntityTemplate(point);

		this.readCommonEntityData(template);

		//2RD + RD : Location 10
		point.Location = this.readPoint3D();
		point.Thickness = this._entityThickness;

		//Option 0x01
		if (this.hasOption(0x01))
		{
			//3RD : Extrusion 210
			point.Normal = this._reader.Read3RawDouble();
		}

		//Option 0x02
		if (this.hasOption(0x02))
		{
			//RD : X axis angle 50
			point.Rotation = this._reader.ReadDouble();
		}

		return template;
	}

	private CadTemplate readPolyline()
	{
		//Option 0x01, RC : Flags 70
		PolylineFlags flags = this.hasOption(0x01) ? (PolylineFlags)this._reader.ReadByte() : PolylineFlags.Default;

		IPolyline polyline;
		if (flags.HasFlag(PolylineFlags.PolyfaceMesh))
		{
			polyline = new PolyfaceMesh();
		}
		else if (flags.HasFlag(PolylineFlags.PolygonMesh))
		{
			polyline = new PolygonMesh();
		}
		else if (flags.HasFlag(PolylineFlags.Polyline3D))
		{
			polyline = new Polyline3D();
		}
		else
		{
			polyline = new Polyline2D();
		}

		CadPolyLineTemplate template = new CadPolyLineTemplate(polyline);
		this.readCommonEntityData(template);

		switch (polyline)
		{
			case Polyline<Vertex2D> pline:
				this.readPolylineData(pline, flags);
				break;
			case Polyline<Vertex3D> pline:
				this.readPolylineData(pline, flags);
				break;
			case Polyline<PolygonMeshVertex> pline:
				this.readPolylineData(pline, flags);
				break;
			case Polyline<VertexFaceMesh> pline:
				this.readPolylineData(pline, flags);
				break;
		}

		return template;
	}

	private void readPolylineData<T>(Polyline<T> polyline, PolylineFlags flags)
		where T : Entity, IVertex
	{
		polyline.Flags = flags;
		polyline.Elevation = this._entityElevation;
		polyline.Thickness = this._entityThickness;

		//Option 0x02
		if (this.hasOption(0x02))
		{
			//RD : Start width 40
			polyline.StartWidth = this._reader.ReadDouble();
		}

		//Option 0x04
		if (this.hasOption(0x04))
		{
			//RD : End width 41
			polyline.EndWidth = this._reader.ReadDouble();
		}

		//Option 0x08
		if (this.hasOption(0x08))
		{
			//3RD : Extrusion 210
			polyline.Normal = this._reader.Read3RawDouble();
		}

		//Option 0x10, RS : M vertex count 71
		short mCount = this.hasOption(0x10) ? this._reader.ReadShort() : (short)0;
		//Option 0x20, RS : N vertex count 72
		short nCount = this.hasOption(0x20) ? this._reader.ReadShort() : (short)0;
		//Option 0x40, RS : M density 73
		short mDensity = this.hasOption(0x40) ? this._reader.ReadShort() : (short)0;
		//Option 0x80, RS : N density 74
		short nDensity = this.hasOption(0x80) ? this._reader.ReadShort() : (short)0;

		//Option 0x100
		if (this.hasOption(0x100))
		{
			//RS : Curve type 75
			polyline.SmoothSurface = (SmoothSurfaceType)this._reader.ReadShort();
		}

		if (polyline is PolygonMesh mesh)
		{
			mesh.MVertexCount = mCount;
			mesh.NVertexCount = nCount;
			mesh.MSmoothSurfaceDensity = mDensity;
			mesh.NSmoothSurfaceDensity = nDensity;
		}

		//Option 0x8000 : unknown data of the polylines in the extra entities section
	}

	private CadTemplate readSeqend()
	{
		Seqend seqend = new Seqend();
		CadEntityTemplate template = new CadEntityTemplate(seqend);

		this.readCommonEntityData(template);

		//RL : Address of the entity that starts the sequence
		int begin = this._reader.ReadInt();

		switch (this._complexOwner)
		{
			case CadPolyLineTemplate pline:
				template.OwnerHandle = pline.CadObject.Handle;
				pline.SeqendHandle = seqend.Handle;
				break;
			case CadInsertTemplate insert:
				template.OwnerHandle = insert.CadObject.Handle;
				insert.SeqendHandle = seqend.Handle;
				break;
			default:
				return null;
		}

		return template;
	}

	private CadTemplate readShape()
	{
		Shape shape = new Shape();
		CadShapeTemplate template = new CadShapeTemplate(shape);

		this.readCommonEntityData(template);

		//2RD : Insertion point 10
		shape.InsertionPoint = this.readPlanePoint();
		//RD : Size 40
		shape.Size = this._reader.ReadDouble();
		//RC : Shape number 2
		template.ShapeIndex = this._reader.ReadByte();
		shape.Thickness = this._entityThickness;

		//Option 0x01
		if (this.hasOption(0x01))
		{
			//RD : Rotation 50
			shape.Rotation = this._reader.ReadDouble();
		}

		//Option 0x02
		//RC : Style index of the shape file
		byte style = this.hasOption(0x02) ? this._reader.ReadByte() : (byte)0;
		template.ShapeFileHandle = this.getEntryHandle(DxfFileToken.TableStyle, style);

		//Option 0x04
		if (this.hasOption(0x04))
		{
			//RD : Width factor 41
			shape.RelativeXScale = this._reader.ReadDouble();
		}

		//Option 0x08
		if (this.hasOption(0x08))
		{
			//RD : Oblique angle 51
			shape.ObliqueAngle = this._reader.ReadDouble();
		}

		return template;
	}

	private CadTemplate readSolid()
	{
		Solid solid = new Solid();
		CadEntityTemplate template = new CadEntityTemplate(solid);

		this.readCommonEntityData(template);

		//2RD : 1st corner 10
		solid.FirstCorner = this.readPlanePoint();
		//2RD : 2nd corner 11
		solid.SecondCorner = this.readPlanePoint();
		//2RD : 3rd corner 12
		solid.ThirdCorner = this.readPlanePoint();
		//2RD : 4th corner 13
		solid.FourthCorner = this.readPlanePoint();
		solid.Thickness = this._entityThickness;

		//Option 0x01
		if (this.hasOption(0x01))
		{
			//3RD : Extrusion 210
			solid.Normal = this._reader.Read3RawDouble();
		}

		return template;
	}

	private CadTemplate readText()
	{
		TextEntity text = new TextEntity();
		CadTextEntityTemplate template = new CadTextEntityTemplate(text);

		this.readCommonEntityData(template);

		//2RD : Insertion point 10
		text.InsertPoint = this.readPlanePoint();
		//RD : Height 40
		text.Height = this._reader.ReadDouble();
		//TV : Value 1
		text.Value = this._reader.ReadVariableText();
		text.Thickness = this._entityThickness;

		//Option 0x01
		if (this.hasOption(0x01))
		{
			//RD : Rotation 50
			text.Rotation = this._reader.ReadDouble();
		}

		//Option 0x02
		if (this.hasOption(0x02))
		{
			//RD : Width factor 41
			text.WidthFactor = this._reader.ReadDouble();
		}

		//Option 0x04
		if (this.hasOption(0x04))
		{
			//RD : Oblique angle 51
			text.ObliqueAngle = this._reader.ReadDouble();
		}

		//Option 0x08
		//RC : Style index 7
		byte style = this.hasOption(0x08) ? this._reader.ReadByte() : (byte)0;
		template.StyleHandle = this.getEntryHandle(DxfFileToken.TableStyle, style);

		//Option 0x10
		if (this.hasOption(0x10))
		{
			//RC : Generation 71
			text.Mirror = (TextMirrorFlag)this._reader.ReadByte();
		}

		//Option 0x20
		if (this.hasOption(0x20))
		{
			//RC : Horizontal alignment 72
			text.HorizontalAlignment = (TextHorizontalAlignment)this._reader.ReadByte();
		}

		//Option 0x40
		if (this.hasOption(0x40))
		{
			//2RD : Alignment point 11
			text.AlignmentPoint = this.readPlanePoint();
		}

		//Option 0x80
		if (this.hasOption(0x80))
		{
			//3RD : Extrusion 210
			text.Normal = this._reader.Read3RawDouble();
		}

		//Option 0x100
		if (this.hasOption(0x100))
		{
			//RC : Vertical alignment 73
			text.VerticalAlignment = (TextVerticalAlignmentType)this._reader.ReadByte();
		}

		return template;
	}

	private CadTemplate readVertex()
	{
		if (this._complexOwner is not CadPolyLineTemplate pline)
		{
			this._builder.Notify($"Vertex at {this._entityStart} without polyline", NotificationType.Warning);
			return null;
		}

		//Option 0x4000 : the face records of a polyface mesh have no location
		//2RD : Location 10
		XYZ location = this.hasOption(0x4000) ? XYZ.Zero : this.readPlanePoint();
		//Option 0x01, RD : Start width 40
		double startWidth = this.hasOption(0x01) ? this._reader.ReadDouble() : 0;
		//Option 0x02, RD : End width 41
		double endWidth = this.hasOption(0x02) ? this._reader.ReadDouble() : 0;
		//Option 0x04, RD : Bulge 42
		double bulge = this.hasOption(0x04) ? this._reader.ReadDouble() : 0;
		//Option 0x08, RC : Flags 70
		VertexFlags flags = this.hasOption(0x08) ? (VertexFlags)this._reader.ReadByte() : VertexFlags.Default;
		//Option 0x10, RD : Curve fit tangent direction 50
		double tangent = this.hasOption(0x10) ? this._reader.ReadDouble() : 0;

		//Options 0x20 to 0x100, RS : Vertex indexes of the face record 71-74
		short[] indexes = new short[4];
		for (int i = 0; i < indexes.Length; i++)
		{
			if (this.hasOption(0x20 << i))
			{
				indexes[i] = this._reader.ReadShort();
			}
		}

		Entity vertex;
		switch (pline.CadObject)
		{
			case PolyfaceMesh:
				if (this.hasOption(0x4000) || (flags.HasFlag(VertexFlags.PolyFaceMeshVertex) && !flags.HasFlag(VertexFlags.PolygonMesh3D)))
				{
					vertex = new VertexFaceRecord
					{
						Index1 = indexes[0],
						Index2 = indexes[1],
						Index3 = indexes[2],
						Index4 = indexes[3],
					};
				}
				else
				{
					vertex = new VertexFaceMesh();
				}
				break;
			case PolygonMesh:
				vertex = new PolygonMeshVertex();
				break;
			case Polyline3D:
				vertex = new Vertex3D();
				break;
			default:
				vertex = new Vertex2D();
				break;
		}

		if (vertex is Vertex v)
		{
			v.Location = location;
			v.StartWidth = startWidth;
			v.EndWidth = endWidth;
			v.Bulge = bulge;
			v.Flags = flags;
			v.CurveTangent = tangent;
		}

		CadEntityTemplate template = new CadEntityTemplate(vertex);
		this.readCommonEntityData(template);

		template.OwnerHandle = pline.CadObject.Handle;
		pline.OwnedObjectsHandlers.Add(vertex.Handle);

		return template;
	}

	private CadTemplate readViewport()
	{
		Viewport viewport = new Viewport();
		CadViewportTemplate template = new CadViewportTemplate(viewport);

		this.readCommonEntityData(template);

		//3RD : Center 10
		viewport.Center = this._reader.Read3RawDouble();
		//RD : Width 40
		viewport.Width = this._reader.ReadDouble();
		//RD : Height 41
		viewport.Height = this._reader.ReadDouble();
		//RS : Viewport id 69
		template.ViewportId = this._reader.ReadShort();

		return template;
	}

	private ulong? getBlockHandle(int index)
	{
		if (this._blockRecords.TryGetValue(index, out CadBlockRecordTemplate record))
		{
			return record.CadObject.Handle;
		}

		return null;
	}
}
