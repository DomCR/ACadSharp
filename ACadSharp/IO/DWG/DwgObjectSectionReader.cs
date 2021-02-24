using ACadSharp.Blocks;
using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.Geometry;
using ACadSharp.Geometry.Units;
using ACadSharp.IO.Templates;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSUtilities.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ACadSharp.IO.DWG
{
	/* Documentation:
	 * This region holds the actual objects in the drawing. 
	 * These can be entities, table entries, dictionary entries, and objects. 
	 * This second use of objects is somewhat confusing; all items stored in the file are “objects”,
	 * but only some of them are object objects. 
	 * Others are entities, table entries, etc. 
	 * The objects in this section can appear in any order.
	 * 
	 * Not all objects present in the file are actually used.
	 * The used objects can be traced back to handle references in the Header section.
	 * 
	 * So the proper way to read a file is to start reading the header and then tracing all 
	 * references from there until all references have been followed. 
	 * Very occasionally a file contains e.g. two APPID objects with the same name,
	 * of which one is used, and the other is not. Reading both would be incorrect due to a
	 * name clash. To complicate matters more, files also exist with table records with duplicate 
	 * names. This is incorrect, and the software should rename the record to be unique upon reading.
	 */
	internal class DwgObjectSectionReader : DwgSectionReader
	{
		private long m_objectInitialPos = 0;

		/// <summary>
		/// Document where the objects will be placed.
		/// </summary>
		private CadDocument m_document;
		/// <summary>
		/// During the object reading the handles will be added at the queue.
		/// </summary>
		private Queue<ulong> m_handles;
		private readonly Dictionary<ulong, long> m_map;
		private readonly Dictionary<short, DxfClass> m_classes;
		/// <summary>
		/// Store the readed objects to create the document once finished
		/// </summary>
		private Dictionary<ulong, CadObject> m_objectMap = new Dictionary<ulong, CadObject>();
		private List<DwgTemplate> m_templates = new List<DwgTemplate>();

		private readonly IDwgStreamReader m_reader;

		/// <summary>
		/// Needed to handle some items like colors or some text data that may not be present.
		/// </summary>
		private IDwgStreamReader m_mergedReaders;
		/// <summary>
		/// Reader to handle the object data.
		/// </summary>
		private IDwgStreamReader m_objectReader;
		/// <summary>
		/// Reader focused on the handles section of the stream.
		/// </summary>
		private IDwgStreamReader m_referenceReader;
		/// <summary>
		/// Reader focused on the string data section of the stream.
		/// </summary>
		private IDwgStreamReader m_textReader;

		/// <summary>
		/// Stream decoded using the crc.
		/// </summary>
		private IDwgStreamReader m_crcReader;
		private CRC8StreamHandler m_crcStream;

		public DwgObjectSectionReader(ACadVersion version, CadDocument document,
			IDwgStreamReader reader, Queue<ulong> handles,
			Dictionary<ulong, long> handleMap,
			List<DxfClass> classes) : base(version)
		{
			m_document = document;
			m_reader = reader;

			m_handles = new Queue<ulong>(handles);
			m_map = new Dictionary<ulong, long>(handleMap);
			m_classes = classes.ToDictionary(x => x.ClassNumber, x => x);

			//Initialize the crc stream
			//RS : CRC for the data section, starting after the sentinel. Use 0xC0C1 for the initial value.
			m_crcStream = new CRC8StreamHandler(m_reader.Stream, 0xC0C1);
			//Setup the entity handler
			m_crcReader = DwgStreamReader.GetStreamHandler(m_version, m_crcStream);
		}
		//**************************************************************************
		/// <summary>
		/// Read all the entities, tables and objects in the file.
		/// </summary>
		public void Read()
		{
			//TODO: Slow execution, implement a progress notification system.

			//Read each handle in the header
			while (m_handles.Any())
			{
				ulong handle = m_handles.Dequeue();
				if (handle == 0)
					//Incorrect handle
					continue;

				if (!m_map.TryGetValue(handle, out long offset))
					continue;

				//Get the object type
				ObjectType type = getEntityType(offset);

				//Read the object
				DwgTemplate template = readObject(type);
				if (template == null)
					continue;

				//Add the object to the map
				m_objectMap[template.CadObject.Handle] = template.CadObject;
				//Add the template to the list to be processed
				m_templates.Add(template);
			}
		}
		//**************************************************************************
		private ObjectType getEntityType(long offset)
		{
			ObjectType type = ObjectType.INVALID;

			//Set the position to the entity to find
			m_crcReader.Position = offset;

			//MS : Size of object, not including the CRC
			ushort size = (ushort)m_crcReader.ReadModularShort();

			if (size <= 0U)
				return type;

			//remove the padding bits make sure the object stream ends on a byte boundary
			uint sizeInBits = (uint)(size << 3);

			//R2010+:
			if (R2010Plus)
			{
				//MC : Size in bits of the handle stream (unsigned, 0x40 is not interpreted as sign).
				//This includes the padding bits at the end of the handle stream 
				//(the padding bits make sure the object stream ends on a byte boundary).
				ulong handleSize = m_crcReader.ReadModularChar();

				//Find the handles offset
				ulong handleSectionOffset = (ulong)m_crcReader.PositionInBits() + sizeInBits - handleSize;

				//Create a handler section reader
				m_objectReader = DwgStreamReader.GetStreamHandler(m_version, new StreamIO(m_crcStream, true).Stream);
				m_objectReader.SetPositionInBits(m_crcReader.PositionInBits());

				//set the initial posiltion and get the object type
				m_objectInitialPos = m_objectReader.PositionInBits();
				type = m_objectReader.ReadObjectType();

				//if (type == ObjectType.BLOCK_HEADER || handleSize == 1)
				//	return type;

				//Create a handler section reader
				m_referenceReader = DwgStreamReader.GetStreamHandler(m_version, new StreamIO(m_crcStream, true).Stream);
				m_referenceReader.SetPositionInBits((long)handleSectionOffset);

				//Create a text section reader
				m_textReader = DwgStreamReader.GetStreamHandler(m_version, new StreamIO(m_crcStream, true).Stream);
				m_textReader.SetPositionByFlag((long)handleSectionOffset - 1);

				m_mergedReaders = new DwgMergedReader(m_objectReader, m_textReader, m_referenceReader);
			}
			else
			{
				//Create a handler section reader
				m_objectReader = DwgStreamReader.GetStreamHandler(m_version, new StreamIO(m_crcStream, true).Stream);
				m_objectReader.SetPositionInBits(m_crcReader.PositionInBits());

				m_referenceReader = DwgStreamReader.GetStreamHandler(m_version, new StreamIO(m_crcStream, true).Stream);
				m_textReader = DwgStreamReader.GetStreamHandler(m_version, new StreamIO(m_crcStream, true).Stream);

				//set the initial posiltion and get the object type
				m_objectInitialPos = m_objectReader.PositionInBits();
				type = m_objectReader.ReadObjectType();
			}


			return type;
		}
		//**************************************************************************
		#region Common entity data
		/// <summary>
		/// Get the handle of the entity and saves the value to the <see cref="m_handles"/>
		/// </summary>
		/// <returns>the handle to reference into the entity</returns>
		private ulong handleReference()
		{
			return handleReference(0);
		}
		/// <summary>
		/// Get the handle of the entity and saves the value to the <see cref="m_handles"/>
		/// </summary>
		/// <returns>the handle to reference into the entity</returns>
		private ulong handleReference(ulong handle)
		{
			//Read the handle
			var value = m_referenceReader.HandleReference(handle);  //690

			//Add the value to the handles queue to be processed
			m_handles.Enqueue(value);

			return value;
		}

		/// <summary>
		/// Read the common entity format.
		/// </summary>
		/// <param name="template"></param>
		private void readCommonEntityData(DwgEntityTemplate template)
		{
			//Get the cad object as an entity
			Entity entity = (Entity)template.CadObject;

			if (m_version >= ACadVersion.AC1015 && m_version < ACadVersion.AC1024)
				//Obj size RL size of object in bits, not including end handles
				updateHandleReader();

			//Common:
			//Handle H 5 code 0, length followed by the handle bytes.
			ulong handle = m_objectReader.HandleReference();
			entity.Handle = handle;

			//Extended object data, if any
			readExtendedData(template);

			//Graphic present Flag B 1 if a graphic is present
			if (m_objectReader.ReadBit())
			{
				//Graphics X if graphicpresentflag is 1, the graphic goes here.
				//See the section on Proxy Entity Graphics for the format of this section.

				//R13 - R007:
				//RL: Size of graphic image in bytes
				//R2010 +:
				//BLL: Size of graphic image in bytes
				long graphicImageSize = m_version >= ACadVersion.AC1024 ?
					m_objectReader.ReadBitLongLong() : m_objectReader.ReadRawLong();

				//Common:
				//X: The graphic image
				//entityHandler.CadObject.JumpGraphicImage(this, entityHandler, graphicImageSize);
				m_mergedReaders.Advance((int)graphicImageSize);
			}

			//R13 - R14 Only:
			if (m_version >= ACadVersion.AC1012 && m_version <= ACadVersion.AC1014)
				updateHandleReader();

			//Common:
			//6B : Flags
			//Entmode BB entity mode
			template.EntityMode = m_objectReader.Read2Bits();

			//FE: Entity mode(entmode). Generally, this indicates whether or not the owner
			//relative handle reference is present.The values go as follows:

			//00 : The owner relative handle reference is present.
			//Applies to the following:
			//VERTEX, ATTRIB, and SEQEND.
			//BLOCK, ENDBLK, and the defining entities in all
			//block defs except *MODEL_SPACE and * PAPER_SPACE.

			//01 : PSPACE entity without a owner relative handle ref.
			//10 : MSPACE entity without a owner relative handle ref.
			//11 : Not used.

			if (template.EntityMode == 0)
				entity.OwnerHandle = m_referenceReader.HandleReference(entity.Handle);

			//Numreactors BL number of persistent reactors attached to this object
			readReactors(template);

			//R13-R14 Only:
			//if (m_version >= ACadVersion.AC1012 && m_version <= ACadVersion.AC1014)
			if (R13_14Only)
			{
				//8 LAYER (hard pointer)
				template.LayerHandle = handleReference();

				//Isbylayerlt B 1 if bylayer linetype, else 0
				if (!m_objectReader.ReadBit())
					//6 [LTYPE (hard pointer)] (present if Isbylayerlt is 0)
					template.LineTypeHandle = handleReference();
			}

			//R13-R2000 Only:
			//previous/next handles present if Nolinks is 0.
			//Nolinks B 1 if major links are assumed +1, -1, else 0 For R2004+this always has value 1 (links are not used)
			if (!(m_version >= ACadVersion.AC1018) && !m_objectReader.ReadBit())
			{
				//[PREVIOUS ENTITY (relative soft pointer)]
				template.PrevEntity = handleReference(entity.Handle);
				//[NEXT ENTITY (relative soft pointer)]
				template.NextEntity = handleReference(entity.Handle);
			}
			else if (!(m_version >= ACadVersion.AC1018))
			{
				m_handles.Enqueue(entity.Handle - 1UL);
				m_handles.Enqueue(entity.Handle + 1UL);
			}

			//Color	CMC(B)	62
			entity.Color = m_objectReader.ReadEnColor(out Transparency transparency, out bool colorFlag);
			entity.Transparency = transparency;

			//R2004+:
			if ((m_version >= ACadVersion.AC1018) && colorFlag)
				//[Color book color handle (hard pointer)]
				template.ColorHandle = handleReference();

			//Ltype scale	BD	48
			entity.LinetypeScale = m_objectReader.ReadBitDouble();

			if (!(m_version >= ACadVersion.AC1015))
			{
				//Common:
				//Invisibility BS 60
				entity.IsInvisible = (m_objectReader.ReadBitShort() & 1) == 0;

				return;
			}

			//R2000+:
			//8 LAYER (hard pointer)
			template.LayerHandle = handleReference();

			//Ltype flags BB 00 = bylayer, 01 = byblock, 10 = continous, 11 = linetype handle present at end of object
			byte ltypeFlags = m_objectReader.Read2Bits();

			if (ltypeFlags == 3)
				//6 [LTYPE (hard pointer)] present if linetype flags were 11
				template.LineTypeHandle = handleReference();

			//R2007+:
			if (m_version >= ACadVersion.AC1021)
			{
				//Material flags BB 00 = bylayer, 01 = byblock, 11 = material handle present at end of object
				if (m_objectReader.Read2Bits() == 3)
				{
					//MATERIAL present if material flags were 11
					long num2 = (long)handleReference();
				}

				//Shadow flags RC
				int num3 = m_objectReader.ReadByte();
			}

			//R2000 +:
			//Plotstyle flags	BB	00 = bylayer, 01 = byblock, 11 = plotstyle handle present at end of object
			if (m_objectReader.Read2Bits() == 3)
			{
				//PLOTSTYLE (hard pointer) present if plotstyle flags were 11
				long plotstyleFlags = (long)handleReference();
			}

			//R2007 +:
			if (m_version > ACadVersion.AC1021)
			{
				//Material flags BB 00 = bylayer, 01 = byblock, 11 = material handle present at end of object
				if (m_objectReader.ReadBit())
				{
					//If has full visual style, the full visual style handle (hard pointer).
					long n = (long)handleReference();
				}
				if (m_objectReader.ReadBit())
				{
					//If has full visual style, the full visual style handle (hard pointer).
					long n = (long)handleReference();
				}
				//Shadow flags RC
				if (m_objectReader.ReadBit())
				{
					//If has full visual style, the full visual style handle (hard pointer).
					long n = (long)handleReference();
				}
			}

			//Common:
			//Invisibility BS 60
			entity.IsInvisible = (m_objectReader.ReadBitShort() & 1) == 1;

			//R2000+:
			//Lineweight RC 370
			entity.Lineweight = (Lineweight)m_objectReader.ReadByte();
		}
		private void readCommonNonEntityData(DwgTemplate template)
		{
			if (m_version >= ACadVersion.AC1015 && m_version < ACadVersion.AC1024)
				//Obj size RL size of object in bits, not including end handles
				updateHandleReader();

			//Common:
			//Handle H 5 code 0, length followed by the handle bytes.
			ulong handle = m_objectReader.HandleReference();
			template.CadObject.Handle = handle;

			//Extended object data, if any
			readExtendedData(template);

			//R13-R14 Only:
			//Obj size RL size of object in bits, not including end handles
			if (R13_14Only)
				updateHandleReader();


			//[Owner ref handle (soft pointer)]
			template.CadObject.OwnerHandle = handleReference(template.CadObject.Handle);
			//Read the cad object reactors
			readReactors(template);
		}
		private void readXrefDependantBit(DwgTableEntryTemplate template)
		{
			TableEntry entry = template.CadObject as TableEntry;

			if (R2007Plus)
			{
				//xrefindex+1 BS 70 subtract one from this value when read.
				//After that, -1 indicates that this reference did not come from an xref,
				//otherwise this value indicates the index of the blockheader for the xref from which this came.
				short xrefindex = m_objectReader.ReadBitShort();
				//Xdep B 70 dependent on an xref. (16 bit)
				entry.XrefDependant = ((uint)xrefindex & 0b100000000) > 0;
			}
			else
			{
				//64-flag B 70 The 64-bit of the 70 group.
				m_objectReader.ReadBit();

				//xrefindex+1 BS 70 subtract one from this value when read.
				//After that, -1 indicates that this reference did not come from an xref,
				//otherwise this value indicates the index of the blockheader for the xref from which this came.
				int xrefindex = m_objectReader.ReadBitShort() - 1;

				//Xdep B 70 dependent on an xref. (16 bit)
				entry.XrefDependant = m_objectReader.ReadBit();
			}
		}
		private void readExtendedData(DwgTemplate template)
		{
			//EED directly follows the entity handle.
			//Each application's data is structured as follows:
			//|Length|Application handle|Data items|

			//EED size BS size of extended entity data, if any
			short size = m_objectReader.ReadBitShort();

			while (size != 0)
			{
				//App handle
				ulong appHandle = m_objectReader.HandleReference();
				long endPos = m_objectReader.Position + size;

				//template.ExtendedData
				readExtendedData(endPos);

				size = m_objectReader.ReadBitShort();
			}
		}
		private ExtendedData readExtendedData(long endPos)
		{
			ExtendedData edata = new ExtendedData();

			m_objectReader.ReadBytes((int)(endPos - m_objectReader.Position));

			//TODO: Implement extended data reader

			return edata;
		}
		/// <summary>
		/// Add the reactors to the template.
		/// </summary>
		/// <param name="template"></param>
		private void readReactors(DwgTemplate template)
		{
			//Numreactors S number of reactors in this object
			int numberOfReactors = m_objectReader.ReadBitLong();

			//Add the reactors to the template
			for (int i = 0; i < numberOfReactors; ++i)
				//[Reactors (soft pointer)]
				template.CadObject.Reactors.Add(handleReference(), null);

			bool flag = false;
			//R2004+:
			if (m_version >= ACadVersion.AC1018)
				/*XDic Missing Flag
				 * B 
				 * If 1, no XDictionary handle is stored for this object,
				 * otherwise XDictionary handle is stored as in R2000 and earlier.
				*/
				flag = m_objectReader.ReadBit();

			if (!flag)
				//xdicobjhandle(hard owner)
				template.XDictHandle = handleReference();   //690

			if (m_version <= ACadVersion.AC1024)
				return;

			//R2013+:	
			//Has DS binary data B If 1 then this object has associated binary data stored in the data store
			m_objectReader.ReadBit();
		}
		/// <summary>
		/// Update the text reader and the handler reader at the end of the object position.
		/// </summary>
		private void updateHandleReader()
		{
			//RL: Size of object data in bits (number of bits before the handles), 
			//or the "endbit" of the pre-handles section.
			long size = m_objectReader.ReadRawLong();

			//Set the position to the handle section
			m_referenceReader.SetPositionInBits(size + m_objectInitialPos);

			if (m_version == ACadVersion.AC1021)
			{
				//"endbit" of the pre-handles section.
				m_textReader.SetPositionByFlag(size + m_objectInitialPos - 1);
			}

			m_mergedReaders = new DwgMergedReader(m_objectReader, m_textReader, m_referenceReader);
		}
		#endregion

		#region Object readers
		private DwgTemplate readObject(ObjectType type)
		{
			DwgTemplate template = null;

			switch (type)
			{
				case ObjectType.UNUSED:
					break;
				case ObjectType.TEXT:
					template = readText();
					break;
				case ObjectType.ATTRIB:
					template = readAttribute();
					break;
				case ObjectType.ATTDEF:
					template = readAttributeDefinition();
					break;
				case ObjectType.BLOCK:
					template = readBlock();
					break;
				case ObjectType.ENDBLK:
					break;
				case ObjectType.SEQEND:
					break;
				case ObjectType.INSERT:
					break;
				case ObjectType.MINSERT:
					break;
				case ObjectType.UNKNOW_9:
					break;
				case ObjectType.VERTEX_2D:
					template = readVertex2D();
					break;
				case ObjectType.VERTEX_3D:
				case ObjectType.VERTEX_MESH:
				case ObjectType.VERTEX_PFACE:
					template = readVertex3D();
					break;
				case ObjectType.VERTEX_PFACE_FACE:
					break;
				case ObjectType.POLYLINE_2D:
					template = readPolyline2D();
					break;
				case ObjectType.POLYLINE_3D:
					template = readPolyline3D();
					break;
				case ObjectType.ARC:
					template = readArc();
					break;
				case ObjectType.CIRCLE:
					template = readCircle();
					break;
				case ObjectType.LINE:
					template = readLine();
					break;
				case ObjectType.DIMENSION_ORDINATE:
					break;
				case ObjectType.DIMENSION_LINEAR:
					break;
				case ObjectType.DIMENSION_ALIGNED:
					break;
				case ObjectType.DIMENSION_ANG_3_Pt:
					break;
				case ObjectType.DIMENSION_ANG_2_Ln:
					break;
				case ObjectType.DIMENSION_RADIUS:
					break;
				case ObjectType.DIMENSION_DIAMETER:
					break;
				case ObjectType.POINT:
					template = readPoint();
					break;
				case ObjectType.FACE3D:
					break;
				case ObjectType.POLYLINE_PFACE:
					break;
				case ObjectType.POLYLINE_MESH:
					break;
				case ObjectType.SOLID:
					break;
				case ObjectType.TRACE:
					break;
				case ObjectType.SHAPE:
					break;
				case ObjectType.VIEWPORT:
					break;
				case ObjectType.ELLIPSE:
					break;
				case ObjectType.SPLINE:
					break;
				case ObjectType.REGION:
					break;
				case ObjectType.SOLID3D:
					break;
				case ObjectType.BODY:
					break;
				case ObjectType.RAY:
					break;
				case ObjectType.XLINE:
					break;
				case ObjectType.DICTIONARY:
					template = readDictionary();
					break;
				case ObjectType.OLEFRAME:
					break;
				case ObjectType.MTEXT:
					break;
				case ObjectType.LEADER:
					break;
				case ObjectType.TOLERANCE:
					break;
				case ObjectType.MLINE:
					break;
				case ObjectType.BLOCK_CONTROL_OBJ:
					template = readBlockControlObject();
					break;
				case ObjectType.BLOCK_HEADER:
					template = readBlockHeader();
					break;
				case ObjectType.LAYER_CONTROL_OBJ:
					break;
				case ObjectType.LAYER:
					template = readLayer();
					break;
				case ObjectType.STYLE_CONTROL_OBJ:
					break;
				case ObjectType.STYLE:
					template = readStyle();
					break;
				case ObjectType.UNKNOW_36:
					break;
				case ObjectType.UNKNOW_37:
					break;
				case ObjectType.LTYPE_CONTROL_OBJ:
					break;
				case ObjectType.LTYPE:
					template = readLType();
					break;
				case ObjectType.UNKNOW_3A:
					break;
				case ObjectType.UNKNOW_3B:
					break;
				case ObjectType.VIEW_CONTROL_OBJ:
					break;
				case ObjectType.VIEW:
					break;
				case ObjectType.UCS_CONTROL_OBJ:
					break;
				case ObjectType.UCS:
					break;
				case ObjectType.VPORT_CONTROL_OBJ:
					break;
				case ObjectType.VPORT:
					break;
				case ObjectType.APPID_CONTROL_OBJ:
					break;
				case ObjectType.APPID:
					break;
				case ObjectType.DIMSTYLE_CONTROL_OBJ:
					break;
				case ObjectType.DIMSTYLE:
					break;
				case ObjectType.VP_ENT_HDR_CTRL_OBJ:
					break;
				case ObjectType.VP_ENT_HDR:
					break;
				case ObjectType.GROUP:
					break;
				case ObjectType.MLINESTYLE:
					break;
				case ObjectType.OLE2FRAME:
					break;
				case ObjectType.DUMMY:
					break;
				case ObjectType.LONG_TRANSACTION:
					break;
				case ObjectType.LWPOLYLINE:
					break;
				case ObjectType.HATCH:
					break;
				case ObjectType.XRECORD:
					break;
				case ObjectType.ACDBPLACEHOLDER:
					break;
				case ObjectType.VBA_PROJECT:
					break;
				case ObjectType.LAYOUT:
					break;
				case ObjectType.ACAD_PROXY_ENTITY:
					break;
				case ObjectType.ACAD_PROXY_OBJECT:
					break;
				default:
					//TODO: implement the non fixed value objects (use the classes)
					template = readUnlistedType((short)type);
					break;
			}

			return template;
		}
		private DwgTemplate readUnlistedType(short classNumber)
		{
			if (!m_classes.TryGetValue(classNumber, out DxfClass c))
				return null;

			switch (c.DxfName)
			{
				case "MATERIAL":
				case "WIPEOUTVARIABLES":
				case "VISUALSTYLE":
				default:
					break;
			}

			return null;
		}
		#region Text entities
		private DwgTemplate readText()
		{
			DwgTextEntityTemplate text = new DwgTextEntityTemplate(new Text());
			readCommonTextData(text);

			return text;
		}
		private DwgTemplate readAttribute()
		{
			DwgTextEntityTemplate att = new DwgTextEntityTemplate(new Entities.Attribute());
			readCommonTextData(att);

			//TODO: implement attribute read
			return null;
		}
		private DwgTemplate readAttributeDefinition()
		{
			DwgTextEntityTemplate attdef = new DwgTextEntityTemplate(new AttributeDefinition());
			readCommonTextData(attdef);

			//TODO: implement attribute definition read
			return null;
		}
		private void readCommonTextData(DwgTextEntityTemplate template)
		{
			readCommonEntityData(template);

			TextEntity text = (TextEntity)template.CadObject;

			double elevation = 0.0;
			XY pt = new XY();

			//R13-14 Only:
			if (m_version >= ACadVersion.AC1012 && m_version <= ACadVersion.AC1014)
			{
				//Elevation BD ---
				elevation = m_objectReader.ReadBitDouble();
				//Insertion pt 2RD 10
				pt = m_objectReader.Read2RawDouble();
				text.InsertPoint = new XYZ(pt.X, pt.Y, elevation);

				//Alignment pt 2RD 11
				pt = m_objectReader.Read2RawDouble();
				text.AlignmentPoint = new XYZ(pt.X, pt.Y, elevation);

				//Extrusion 3BD 210
				text.Normal = m_objectReader.Read3BitDouble();
				//Thickness BD 39
				text.Thickness = m_objectReader.ReadBitDouble();
				//Oblique ang BD 51
				text.ObliqueAngle = m_objectReader.ReadBitDouble();
				//Rotation ang BD 50
				text.Rotation = m_objectReader.ReadBitDouble();
				//Height BD 40
				text.Height = m_objectReader.ReadBitDouble();
				//Width factor BD 41
				text.WidthFactor = m_objectReader.ReadBitDouble();
				//Text value TV 1
				text.Value = m_textReader.ReadVariableText();
				//Generation BS 71
				text.Mirror = (TextMirrorFlag)m_objectReader.ReadBitShort();
				//Horiz align. BS 72
				text.HorizontalAlignment = (TextHorizontalAlignment)m_objectReader.ReadBitShort();
				//Vert align. BS 73
				text.VerticalAlignment = (TextVerticalAlignment)m_objectReader.ReadBitShort();

				//Common:
				//Common Entity Handle Data H 7 STYLE(hard pointer)
				template.StyleHandle = handleReference();
				return;
			}

			//DataFlags RC Used to determine presence of subsquent data
			byte dataFlags = m_objectReader.ReadByte();

			//Elevation RD --- present if !(DataFlags & 0x01)
			if ((dataFlags & 0x1) == 0)
				elevation = m_objectReader.ReadDouble();

			//Insertion pt 2RD 10
			pt = m_objectReader.Read2RawDouble();
			text.InsertPoint = new XYZ(pt.X, pt.Y, elevation);

			//Alignment pt 2DD 11 present if !(DataFlags & 0x02), use 10 & 20 values for 2 default values.
			if ((dataFlags & 0x2) == 0)
			{
				double x = m_objectReader.ReadBitDoubleWithDefault((double)text.InsertPoint.X);
				double y = m_objectReader.ReadBitDoubleWithDefault((double)text.InsertPoint.Y);
				text.AlignmentPoint = new XYZ(x, y, elevation);
			}

			//Extrusion BE 210
			text.Normal = m_objectReader.ReadBitExtrusion();
			//Thickness BT 39
			text.Thickness = m_objectReader.ReadBitThickness();

			//Oblique ang RD 51 present if !(DataFlags & 0x04)
			if ((dataFlags & 0x4) == 0)
				text.ObliqueAngle = m_objectReader.ReadDouble();
			//Rotation ang RD 50 present if !(DataFlags & 0x08)
			if ((dataFlags & 0x8) == 0)
				text.Rotation = m_objectReader.ReadDouble();
			//Height RD 40
			text.Height = m_objectReader.ReadDouble();
			//Width factor RD 41 present if !(DataFlags & 0x10)
			if ((dataFlags & 0x10) == 0)
				text.WidthFactor = m_objectReader.ReadDouble();

			//Text value TV 1
			text.Value = m_textReader.ReadVariableText();

			//Generation BS 71 present if !(DataFlags & 0x20)
			if ((dataFlags & 0x20) == 0)
				text.Mirror = (TextMirrorFlag)m_objectReader.ReadBitShort();
			//Horiz align. BS 72 present if !(DataFlags & 0x40)
			if ((dataFlags & 0x40) == 0)
				text.HorizontalAlignment = (TextHorizontalAlignment)m_objectReader.ReadBitShort();
			//Vert align. BS 73 present if !(DataFlags & 0x80)
			if ((dataFlags & 0x80) == 0)
				text.VerticalAlignment = (TextVerticalAlignment)m_objectReader.ReadBitShort();

			//Common:
			//Common Entity Handle Data H 7 STYLE(hard pointer)
			template.StyleHandle = handleReference();
		}
		#endregion

		private DwgTemplate readBlock()
		{
			//TODO: check the info that reads, it may not be the right template

			Block block = new Block();
			//DwgEntityTemplate template = new DwgEntityTemplate(block);

			//readCommonEntityData(template);

			//Block name TV 2
			block.Name = m_textReader.ReadVariableText();

			return null;
		}

		private DwgTemplate readVertex2D()
		{
			Vertex vertex = new Vertex();
			DwgEntityTemplate template = new DwgEntityTemplate(vertex);

			readCommonEntityData(template);

			//Flags EC 70 NOT bit-pair-coded.
			vertex.Flags = (VertexFlags)m_objectReader.ReadByte();
			//Point 3BD 10 NOTE THAT THE Z SEEMS TO ALWAYS BE 0.0! The Z must be taken from the 2D POLYLINE elevation.
			vertex.Location = m_objectReader.Read3BitDouble();

			//Start width BD 40 If it's negative, use the abs val for start AND end widths (and note that no end width will be present). This is a compression trick for cases where the start and end widths are identical and non-0.
			double width = m_objectReader.ReadBitDouble();
			if (width < 0.0)
			{
				vertex.StartWidth = -width;
				vertex.EndWidth = -width;
			}
			else
			{
				vertex.StartWidth = width;
				//End width BD 41 Not present if the start width is < 0.0; see above.
				vertex.EndWidth = m_objectReader.ReadBitDouble();
			}

			//Bulge BD 42
			vertex.Bulge = m_objectReader.ReadBitDouble();

			//R2010+:
			if (R2010Plus)
				//Vertex ID BL 91
				vertex.Id = m_objectReader.ReadBitLong();

			//Common:
			//Tangent dir BD 50
			vertex.CurveTangent = m_objectReader.ReadBitDouble();

			return template;
		}
		private DwgTemplate readVertex3D()
		{
			Vertex vertex = new Vertex();
			DwgEntityTemplate template = new DwgEntityTemplate(vertex);

			readCommonEntityData(template);

			//Flags EC 70 NOT bit-pair-coded.
			vertex.Flags = (VertexFlags)m_objectReader.ReadByte();
			//Point 3BD 10
			vertex.Location = m_objectReader.Read3BitDouble();

			return template;
		}

		private DwgTemplate readPolyline2D()
		{
			PolyLine pline = new PolyLine();
			DwgPolyLineTemplate template = new DwgPolyLineTemplate(pline);

			readCommonEntityData(template);

			//Flags BS 70
			pline.Flags = (PolylineFlags)m_objectReader.ReadBitShort();
			//Curve type BS 75 Curve and smooth surface type.
			pline.SmoothSurface = (SmoothSurfaceType)m_objectReader.ReadBitShort();
			//Start width BD 40 Default start width
			pline.StartWidth = m_objectReader.ReadBitDouble();
			//End width BD 41 Default end width
			pline.EndWidth = m_objectReader.ReadBitDouble();
			//Thickness BT 39
			pline.Thickness = m_objectReader.ReadBitThickness();
			//Elevation BD 10 The 10-pt is (0,0,elev)
			pline.Elevation = m_objectReader.ReadBitDouble();
			//Extrusion BE 210
			pline.Normal = m_objectReader.ReadBitExtrusion();

			//R2004+:
			if (R2004Plus)
			{
				//Owned Object Count BL Number of objects owned by this object.
				int nownedObjects = m_objectReader.ReadBitLong();

				for (int i = 0; i < nownedObjects; ++i)
					template.VertexHandles.Add(handleReference());
			}

			//R13-R2000:
			if (m_version >= ACadVersion.AC1012 && m_version <= ACadVersion.AC1015)
			{
				//H first VERTEX (soft pointer)
				template.FirstVertexHandle = handleReference();
				//H last VERTEX (soft pointer)
				template.LastVertexHandle = handleReference();
			}

			//Common:
			//H SEQEND(hard owner)
			template.SeqendHandle = handleReference();

			return template;
		}
		private DwgTemplate readPolyline3D()
		{
			PolyLine pline = new PolyLine();
			DwgPolyLineTemplate template = new DwgPolyLineTemplate(pline);

			readCommonEntityData(template);

			//Flags RC 70 NOT DIRECTLY THE 75. Bit-coded (76543210):
			byte num1 = m_objectReader.ReadByte();
			//75 0 : Splined(75 value is 5)
			//1 : Splined(75 value is 6)
			bool splined = ((uint)num1 & 0b1) > 0;
			//(If either is set, set 70 bit 2(4) to indicate splined.)
			bool splined1 = ((uint)num1 & 0b10) > 0;

			if (splined | splined1)
			{
				pline.Flags |= PolylineFlags.SplineFit;
			}

			//Flags RC 70 NOT DIRECTLY THE 70. Bit-coded (76543210):
			//0 : Closed(70 bit 0(1))
			//(Set 70 bit 3(8) because this is a 3D POLYLINE.)
			bool closed = (m_objectReader.ReadByte() & 1U) > 0U;

			//R2004+:
			if (R2004Plus)
			{
				//Owned Object Count BL Number of objects owned by this object.
				int nownedObjects = m_objectReader.ReadBitLong();

				for (int i = 0; i < nownedObjects; ++i)
					template.VertexHandles.Add(handleReference());
			}

			//R13-R2000:
			if (m_version >= ACadVersion.AC1012 && m_version <= ACadVersion.AC1015)
			{
				//H first VERTEX (soft pointer)
				template.FirstVertexHandle = handleReference();
				//H last VERTEX (soft pointer)
				template.LastVertexHandle = handleReference();
			}

			//Common:
			//H SEQEND(hard owner)
			template.SeqendHandle = handleReference();

			return template;
		}
		private DwgTemplate readArc()
		{
			Arc arc = new Arc();
			DwgEntityTemplate template = new DwgEntityTemplate(arc);

			readCommonEntityData(template);

			//Center 3BD 10
			arc.Center = m_objectReader.Read3BitDouble();
			//Radius BD 40
			arc.Radius = m_objectReader.ReadBitDouble();
			//Thickness BT 39
			arc.Thickness = m_objectReader.ReadBitThickness();
			//Extrusion BE 210
			arc.Normal = m_objectReader.ReadBitExtrusion();
			//Start angle BD 50
			arc.StartAngle = m_objectReader.ReadBitDouble();
			//End angle BD 51
			arc.EndAngle = m_objectReader.ReadBitDouble();

			return template;
		}
		private DwgTemplate readCircle()
		{
			Circle circle = new Circle();
			DwgEntityTemplate template = new DwgEntityTemplate(circle);

			readCommonEntityData(template);

			//Center 3BD 10
			circle.Center = m_objectReader.Read3BitDouble();
			//Radius BD 40
			circle.Radius = m_objectReader.ReadBitDouble();
			//Thickness BT 39
			circle.Thickness = m_objectReader.ReadBitThickness();
			//Extrusion BE 210
			circle.Normal = m_objectReader.ReadBitExtrusion();

			return template;
		}
		private DwgTemplate readLine()
		{
			Line line = new Line();
			DwgEntityTemplate template = new DwgEntityTemplate(line);

			readCommonEntityData(template);

			//R13-R14 Only:
			if (R13_14Only)
			{
				//Start pt 3BD 10
				line.StartPoint = m_objectReader.Read3BitDouble();
				//End pt 3BD 11
				line.EndPoint = m_objectReader.Read3BitDouble();
			}

			//R2000+:
			if (R2000Plus)
			{
				//Z’s are zero bit B
				bool flag = m_objectReader.ReadBit();
				//Start Point x RD 10
				double startX = m_objectReader.ReadDouble();
				//End Point x DD 11 Use 10 value for default
				double endX = m_objectReader.ReadBitDoubleWithDefault(startX);
				//Start Point y RD 20
				double startY = m_objectReader.ReadDouble();
				//End Point y DD 21 Use 20 value for default
				double endY = m_objectReader.ReadBitDoubleWithDefault(startY);

				double startZ = 0.0;
				double endZ = 0.0;

				if (!flag)
				{
					//Start Point z RD 30 Present only if “Z’s are zero bit” is 0
					startZ = m_objectReader.ReadDouble();
					//End Point z DD 31 Present only if “Z’s are zero bit” is 0, use 30 value for default.
					endZ = m_objectReader.ReadBitDoubleWithDefault(startZ);
				}

				line.StartPoint = new XYZ(startX, startY, startZ);
				line.EndPoint = new XYZ(endX, endY, endZ);
			}

			//Common:
			//Thickness BT 39
			line.Thickness = m_objectReader.ReadBitThickness();
			//Extrusion BE 210
			line.Normal = m_objectReader.ReadBitExtrusion();

			return template;
		}

		private DwgTemplate readPoint()
		{
			Point pt = new Point();
			DwgEntityTemplate template = new DwgEntityTemplate(pt);

			readCommonEntityData(template);

			//Point 3BD 10
			pt.Location = m_objectReader.Read3BitDouble();
			//Thickness BT 39
			pt.Thickness = m_objectReader.ReadBitThickness();
			//Extrusion BE 210
			pt.Normal = m_objectReader.ReadBitExtrusion();
			//X - axis ang BD 50 See DXF documentation
			pt.Rotation = m_objectReader.ReadBitDouble();

			return template;
		}

		private DwgTemplate readDictionary()
		{
			CadDictionary cadDictionary = new CadDictionary();
			DwgDictionaryTemplate template = new DwgDictionaryTemplate(cadDictionary);

			readCommonNonEntityData(template);

			//Common:
			//Numitems L number of dictonary items
			int nentries = m_objectReader.ReadBitLong();

			//R14 Only:
			if (m_version == ACadVersion.AC1014)
			{
				//Unknown R14 RC Unknown R14 byte, has always been 0
				byte zero = m_objectReader.ReadByte();
			}
			//R2000 +:
			if (R2000Plus)
			{
				//Cloning flag BS 281
				cadDictionary.ClonningFlags = (DictionaryCloningFlags)m_objectReader.ReadBitShort();
				//Hard Owner flag RC 280
				cadDictionary.HardOwnerFlag = m_objectReader.ReadByte() > 0;
			}

			//Common:
			for (int i = 0; i < nentries; ++i)
			{
				//Text TV string name of dictionary entry, numitems entries
				string key = m_textReader.ReadVariableText();
				//Handle refs H parenthandle (soft relative pointer)
				//[Reactors(soft pointer)]
				//xdicobjhandle(hard owner)
				//itemhandles (soft owner)
				ulong handle = handleReference();

				template.HandleEntries.Add(key, handle);
			}

			return template;
		}

		private DwgTemplate readBlockControlObject()
		{
			//TODO: Fix the block control object reader

			CadDictionary cadDictionary = new CadDictionary();
			DwgDictionaryTemplate template = new DwgDictionaryTemplate(cadDictionary);

			readCommonNonEntityData(template);

			//Common:
			//Numentries BL 70 Doesn't count *MODEL_SPACE and *PAPER_SPACE.
			var nentries = m_objectReader.ReadBitLong();
			for (int i = 0; i < nentries; i++)
			{
				handleReference();
			}

			//*MODEL_SPACE and *PAPER_SPACE(hard owner).
			handleReference();
			handleReference();

			return null;
		}
		private DwgTemplate readBlockHeader()
		{
			Block block = new Block();
			DwgBlockTemplate template = new DwgBlockTemplate(block);

			readCommonNonEntityData(template);

			//Common:
			//Entry name TV 2
			block.Name = m_textReader.ReadVariableText();

			readXrefDependantBit(template);

			//Anonymous B 1 if this is an anonymous block (1 bit)
			block.IsAnonymous = m_objectReader.ReadBit();
			//Hasatts B 1 if block contains attdefs (2 bit)
			bool hasatts = m_objectReader.ReadBit();
			//Blkisxref B 1 if block is xref (4 bit)
			block.IsXref = m_objectReader.ReadBit();
			//Xrefoverlaid B 1 if an overlaid xref (8 bit)
			block.IsXRefOverlay = m_objectReader.ReadBit();

			//R2000+:
			if (R2000Plus)
				//Loaded Bit B 0 indicates loaded for an xref
				block.IsLoadedXref = m_objectReader.ReadBit();

			//R2004+:
			int nownedObjects = 0;
			if (R2004Plus && (!block.IsXref && !block.IsXRefOverlay))
				//Owned Object Count BL Number of objects owned by this object.
				nownedObjects = m_objectReader.ReadBitLong();

			//Common:
			//Base pt 3BD 10 Base point of block.
			block.BasePoint = m_objectReader.Read3BitDouble();
			//Xref pname TV 1 Xref pathname. That's right: DXF 1 AND 3!
			//3 1 appears in a tblnext/ search elist; 3 appears in an entget.
			block.XrefPath = m_textReader.ReadVariableText();

			//R2000+:
			int insertCount = 0;
			if (R2000Plus)
			{
				//Insert Count RC A sequence of zero or more non-zero RC’s, followed by a terminating 0 RC.The total number of these indicates how many insert handles will be present.
				for (byte i = m_objectReader.ReadByte(); i > 0; i = m_objectReader.ReadByte())
					++insertCount;

				//Block Description TV 4 Block description.
				block.Comments = m_textReader.ReadVariableText();

				//Size of preview data BL Indicates number of bytes of data following.
				int n = m_objectReader.ReadBitLong();
				for (int index = 0; index < n; ++index)
				{
					//Binary Preview Data N*RC 310
					int data = m_objectReader.ReadByte();
				}
			}

			//R2007+:
			if (R2007Plus)
			{
				//TODO: this goes to the BLOCK_RECORD

				//Insert units BS 70
				var bunits = (UnitsType)m_objectReader.ReadBitShort();
				//Explodable B 280
				var isExplodable = m_objectReader.ReadBit();
				//Block scaling RC 281
				var scaleUniformly = m_objectReader.ReadByte() > 0;
			}

			//R13-R2000:
			if (m_version >= ACadVersion.AC1012 && m_version <= ACadVersion.AC1015
				&& (!block.IsXref && !block.IsXRefOverlay))
			{
				//first entity in the def. (soft pointer)
				template.FirstEntityHandle = handleReference();
				//last entity in the def. (soft pointer)
				template.SecondEntityHandle = handleReference();
			}

			//R2004+:
			if (R2004Plus)
			{
				for (int index = 0; index < nownedObjects; ++index)
					//H[ENTITY(hard owner)] Repeats “Owned Object Count” times.
					template.OwnedObjectsHandlers.Add(handleReference());
			}

			//Common:
			//ENDBLK entity. (hard owner)
			template.EndBlockHandle = handleReference();

			//R2000+:
			if (R2000Plus)
			{
				//Insert Handles H N insert handles, where N corresponds to the number of insert count entries above(soft pointer).
				for (int i = 0; i < insertCount; ++i)
				{
					//Entries
					template.Entries.Add(handleReference());
				}
				//Layout Handle H(hard pointer)
				template.LayoutHandle = handleReference();
			}

			return template;
		}

		private DwgTemplate readLayer()
		{
			//Initialize the template with the default layer
			Layer layer = Layer.Default;
			DwgLayerTemplate template = new DwgLayerTemplate(layer);

			readCommonNonEntityData(template);

			//Common:
			//Entry name TV 2
			layer.Name = m_textReader.ReadVariableText();

			readXrefDependantBit(template);

			//R13-R14 Only:
			if (R13_14Only)
			{
				//Frozen B 70 if frozen (1 bit)
				if (m_objectReader.ReadBit())
					layer.Flags |= LayerFlags.Frozen;

				//On B if on.
				layer.IsOn = m_objectReader.ReadBit();

				//Frz in new B 70 if frozen by default in new viewports (2 bit)
				if (m_objectReader.ReadBit())
					layer.Flags |= LayerFlags.FrozenNewViewports;

				//Locked B 70 if locked (4 bit)
				if (m_objectReader.ReadBit())
					layer.Flags |= LayerFlags.Locked;
			}
			//R2000+:
			if (R2000Plus)
			{
				//Values BS 70,290,370 
				short values = m_objectReader.ReadBitShort();

				//contains frozen (1 bit), 
				if (((uint)values & 0b1) > 0)
					layer.Flags |= LayerFlags.Frozen;

				//on (2 bit)
				layer.IsOn = (values & 0b10) == 0;

				//frozen by default in new viewports (4 bit)
				if (((uint)values & 0b100) > 0)
					layer.Flags |= LayerFlags.FrozenNewViewports;
				//locked (8 bit)
				if (((uint)values & 0b1000) > 0)
					layer.Flags |= LayerFlags.Locked;
				//plotting flag (16 bit),
				layer.PlotFlag = ((uint)values & 0b10000) > 0;

				//and lineweight (mask with 0x03E0)
				layer.LineWeight = (Lineweight)((values & 0x3E0) >> 5);
			}

			//Common:
			//Color CMC 62
			layer.Color = m_mergedReaders.ReadCmColor();

			//Handle refs H Layer control (soft pointer)
			//[Reactors(soft pointer)]
			//xdicobjhandle(hard owner)
			//External reference block handle(hard pointer)
			template.LayerControlHandle = handleReference();

			//R2000+:
			if (R2000Plus)
				//H 390 Plotstyle (hard pointer), by default points to PLACEHOLDER with handle 0x0f.
				template.PlotStyleHandle = handleReference();

			//R2007+:
			if (R2007Plus)
			{
				//H 347 Material
				template.MaterialHandle = handleReference();
			}

			//Common:
			//H 6 linetype (hard pointer)
			template.LineTypeHandle = handleReference();

			//H Unknown handle (hard pointer). Always seems to be NULL.
			//Some times is not...
			//handleReference();

			return template;
		}

		private DwgTemplate readStyle()
		{
			//Initialize an empty style
			Style style = new Style();
			DwgTableEntryTemplate template = new DwgTableEntryTemplate(style);

			readCommonNonEntityData(template);

			//Common:
			//Entry name TV 2
			style.Name = m_textReader.ReadVariableText();
			readXrefDependantBit(template);

			//shape file B 1 if a shape file rather than a font (1 bit)
			if (m_objectReader.ReadBit())
				style.Flags |= StyleFlags.IsShape;
			//Vertical B 1 if vertical (4 bit of flag)
			if (m_objectReader.ReadBit())
				style.Flags |= StyleFlags.VerticalText;
			//Fixed height BD 40
			style.Height = m_objectReader.ReadBitDouble();
			//Width factor BD 41
			style.Width = m_objectReader.ReadBitDouble();
			//Oblique ang BD 50
			style.ObliqueAngle = m_objectReader.ReadBitDouble();
			//Generation RC 71 Generation flags (not bit-pair coded).
			style.MirrorFlag = (TextMirrorFlag)m_objectReader.ReadByte();
			//Last height BD 42
			style.LastHeight = m_objectReader.ReadBitDouble();
			//Font name TV 3
			style.Filename = m_textReader.ReadVariableText();
			//Bigfont name TV 4
			style.BigFontFilename = m_textReader.ReadVariableText();

			ulong styleControl = handleReference();

			return template;
		}

		private DwgTemplate readLType()
		{
			LineType ltype = new LineType();
			DwgTableEntryTemplate template = new DwgTableEntryTemplate(ltype);

			readCommonNonEntityData(template);

			//Common:
			//Entry name TV 2
			ltype.Name = m_textReader.ReadVariableText();

			readXrefDependantBit(template);

			//Description TV 3
			ltype.Description = m_textReader.ReadVariableText();
			//Pattern Len BD 40
			ltype.PatternLen = m_objectReader.ReadBitDouble();
			//Alignment RC 72 Always 'A'.
			ltype.Alignment = m_objectReader.ReadRawChar();

			//Numdashes RC 73 The number of repetitions of the 49...74 data.
			int ndashes = m_objectReader.ReadByte();
			//Hold the text flag
			bool isText = false;
			for (int i = 0; i < ndashes; i++)
			{
				LineTypeSegment segment = new LineTypeSegment();

				//Dash length BD 49 Dash or dot specifier.
				segment.Length = m_objectReader.ReadBitDouble();
				//Complex shapecode BS 75 Shape number if shapeflag is 2, or index into the string area if shapeflag is 4.
				short shapecode = m_objectReader.ReadBitShort();

				//X-offset RD 44 (0.0 for a simple dash.)
				//Y - offset RD 45(0.0 for a simple dash.)
				XY offset = new XY(m_objectReader.ReadDouble(), m_objectReader.ReadDouble());
				segment.Offset = offset;

				//Scale BD 46 (1.0 for a simple dash.)
				segment.Scale = m_objectReader.ReadBitDouble();
				//Rotation BD 50 (0.0 for a simple dash.)
				segment.Rotation = m_objectReader.ReadBitDouble();
				//Shapeflag BS 74 bit coded:
				segment.Shapeflag = (LinetypeShapeFlags)m_objectReader.ReadBitShort();

				if (segment.Shapeflag.HasFlag(LinetypeShapeFlags.Text))
					isText = true;

				//Add the segment to the type
				ltype.Segments.Add(segment);
			}

			//R2004 and earlier:
			if (m_version <= ACadVersion.AC1018)
			{
				//Strings area X 9 256 bytes of text area. The complex dashes that have text use this area via the 75-group indices. It's basically a pile of 0-terminated strings. First byte is always 0 for R13 and data starts at byte 1. In R14 it is not a valid data start from byte 0.
				//(The 9 - group is undocumented.)
				byte[] textarea = m_objectReader.ReadBytes(256);
				//TODO: Read the line type text area
			}
			//R2007+:
			if (R2007Plus && isText)
			{
				byte[] textarea = m_objectReader.ReadBytes(256);
				//TODO: Read the line type text area
			}

			//Common:
			//Handle refs H Ltype control(soft pointer)
			//[Reactors (soft pointer)]
			//xdicobjhandle(hard owner)
			//External reference block handle(hard pointer)
			template.LtypeControlHandle = handleReference();

			//340 shapefile for dash/shape (1 each) (hard pointer)
			for (int i = 0; i < ndashes; i++)
			{
				//TODO: Implement the dashes handles
				//handleReference();
			}

			return template;
		}
		#endregion
	}
}
