using ACadSharp.Blocks;
using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.Geometry;
using ACadSharp.Geometry.Units;
using ACadSharp.IO.Templates;
using CSUtilities.IO;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System.Collections.Generic;
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
		/// During the object reading the handles will be added at the queue.
		/// </summary>
		private Queue<ulong> m_handles;
		private readonly Dictionary<ulong, long> m_map;
		private readonly Dictionary<short, DxfClass> m_classes;

		/// <summary>
		/// Store the readed objects to create the document once finished
		/// </summary>
		private Dictionary<ulong, CadObject> m_objectMap { get { return _modelBuilder.ObjectsMap; } }// = new Dictionary<ulong, CadObject>();
		private Dictionary<ulong, DwgTemplate> m_templates { get { return _modelBuilder.Templates; } } //= new List<DwgTemplate>();
		private DwgModelBuilder _modelBuilder;

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
		private IDwgStreamReader m_handlesReader;
		/// <summary>
		/// Reader focused on the string data section of the stream.
		/// </summary>
		private IDwgStreamReader m_textReader;

		/// <summary>
		/// Stream decoded using the crc.
		/// </summary>
		private IDwgStreamReader m_crcReader;
		private CRC8StreamHandler m_crcStream;

		public DwgObjectSectionReader(ACadVersion version, DwgModelBuilder builder,
			IDwgStreamReader reader, Queue<ulong> handles,
			Dictionary<ulong, long> handleMap,
			List<DxfClass> classes) : base(version)
		{
			_modelBuilder = builder;

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

		/// <summary>
		/// Read all the entities, tables and objects in the file.
		/// </summary>
		public void Read(ProgressEventHandler progress = null)
		{
			progress?.Invoke(this, new ProgressEventArgs(0, "Start reading the object section"));

			int n = 0;
			//Read each handle in the header
			while (m_handles.Any())
			{
				ulong handle = m_handles.Dequeue();

				if (!m_map.TryGetValue(handle, out long offset))
				{
					//Notify the readed object
					progress?.Invoke(this, new ProgressEventArgs(n / (float)m_map.Count, $"NULL readed: {n} of {m_map.Count}"));
					n++;
					continue;
				}

				//Get the object type
				ObjectType type = getEntityType(offset);

				//Notify the readed object
				progress?.Invoke(this, new ProgressEventArgs(n / (float)m_map.Count, $"{type} readed: {n} of {m_map.Count}"));
				n++;

				//Read the object
				DwgTemplate template = readObject(type);
				if (template == null)
				{
					//Add the object as null
					m_objectMap[handle] = null;
					continue;
				}

				//Add the object to the map
				m_objectMap[template.CadObject.Handle] = template.CadObject;
				//Add the template to the list to be processed
				m_templates[template.CadObject.Handle] = template;
			}

			//Build the templates in the document
			//buildTemplates();
			//_modelBuilder.BuildObjects();
			_modelBuilder.BuildModelBase();
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
				m_handlesReader = DwgStreamReader.GetStreamHandler(m_version, new StreamIO(m_crcStream, true).Stream);
				m_handlesReader.SetPositionInBits((long)handleSectionOffset);

				//Create a text section reader
				m_textReader = DwgStreamReader.GetStreamHandler(m_version, new StreamIO(m_crcStream, true).Stream);
				m_textReader.SetPositionByFlag((long)handleSectionOffset - 1);

				m_mergedReaders = new DwgMergedReader(m_objectReader, m_textReader, m_handlesReader);
			}
			else
			{
				//Create a handler section reader
				m_objectReader = DwgStreamReader.GetStreamHandler(m_version, new StreamIO(m_crcStream, true).Stream);
				m_objectReader.SetPositionInBits(m_crcReader.PositionInBits());

				m_handlesReader = DwgStreamReader.GetStreamHandler(m_version, new StreamIO(m_crcStream, true).Stream);
				m_textReader = m_objectReader;

				//set the initial posiltion and get the object type
				m_objectInitialPos = m_objectReader.PositionInBits();
				type = m_objectReader.ReadObjectType();
			}


			return type;
		}

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
			ulong value = m_handlesReader.HandleReference(handle);

			if (!m_objectMap.ContainsKey(value) && !m_handles.Contains(value))
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
				entity.OwnerHandle = m_handlesReader.HandleReference(entity.Handle);

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
			template.LtypeFlags = m_objectReader.Read2Bits();

			if (template.LtypeFlags == 3)
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
		private void readXrefDependantBit(TableEntry entry)
		{
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
				template.XDictHandle = handleReference();

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
			m_handlesReader.SetPositionInBits(size + m_objectInitialPos);

			if (m_version == ACadVersion.AC1021)
			{
				m_textReader = DwgStreamReader.GetStreamHandler(m_version, new StreamIO(m_crcStream, true).Stream);
				//"endbit" of the pre-handles section.
				m_textReader.SetPositionByFlag(size + m_objectInitialPos - 1);
			}

			m_mergedReaders = new DwgMergedReader(m_objectReader, m_textReader, m_handlesReader);
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
					//template = readEndBlock();
					break;
				case ObjectType.SEQEND:
					template = readSeqend();
					break;
				case ObjectType.INSERT:
					template = readInsert();
					break;
				case ObjectType.MINSERT:
					template = readMInsert();
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
					template = readPfaceVertex();
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
					template = readDimOrdinate();
					break;
				case ObjectType.DIMENSION_LINEAR:
					template = readDimLinear();
					break;
				case ObjectType.DIMENSION_ALIGNED:
					template = readDimAligned();
					break;
				case ObjectType.DIMENSION_ANG_3_Pt:
					template = readDimAngular3pt();
					break;
				case ObjectType.DIMENSION_ANG_2_Ln:
					template = readDimLine2pt();
					break;
				case ObjectType.DIMENSION_RADIUS:
					template = readDimRadius();
					break;
				case ObjectType.DIMENSION_DIAMETER:
					template = readDimDiameter();
					break;
				case ObjectType.POINT:
					template = readPoint();
					break;
				case ObjectType.FACE3D:
					template = read3dFace();
					break;
				case ObjectType.POLYLINE_PFACE:
					template = readPolylinePface();
					break;
				case ObjectType.POLYLINE_MESH:
					template = readPolylineMesh();
					break;
				case ObjectType.SOLID:
				case ObjectType.TRACE:
					template = readSolid();
					break;
				case ObjectType.SHAPE:
					template = readShape();
					break;
				case ObjectType.VIEWPORT:
					template = readViewport();
					break;
				case ObjectType.ELLIPSE:
					template = readEllipse();
					break;
				case ObjectType.SPLINE:
					template = readSpline();
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
					template = readMText();
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
					template = readAppIdControlObject();
					break;
				case ObjectType.APPID:
					template = readAppId();
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
					template = readHatch();
					break;
				case ObjectType.XRECORD:
					break;
				case ObjectType.ACDBPLACEHOLDER:
					break;
				case ObjectType.VBA_PROJECT:
					break;
				case ObjectType.LAYOUT:
					template = readLayout();
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

			DwgTemplate template = null;

			switch (c.DxfName)
			{
				case "ACDBDICTIONARYWDFLT":
				case "ACDBDETAILVIEWSTYLE":
				case "ACDBSECTIONVIEWSTYLE":
				case "ACAD_TABLE":
				case "CELLSTYLEMAP":
				case "DBCOLOR":
					template = readDwgColor();
					break;
				case "DICTIONARYVAR":
				case "DICTIONARYWDFLT":
				case "FIELD":
				case "GROUP":
					//System.Diagnostics.Debug.Fail($"Not implemented dxf class: {c.DxfName}");
					break;
				case "HATCH":
					template = readHatch();
					break;
				case "IDBUFFER":
				case "IMAGE":
				case "IMAGEDEF":
				case "IMAGEDEFREACTOR":
				case "LAYER_INDEX":
				case "LAYOUT":
				case "LWPLINE":
				case "MATERIAL":
					//template = readMaterial();
					return null;    //Missing documentation
				case "MLEADER":
				case "MLEADERSTYLE":
				case "OLE2FRAME":
				case "PLACEHOLDER":
				case "PLOTSETTINGS":
				case "RASTERVARIABLES":
				case "SCALE":
				case "SORTENTSTABLE":
				case "SPATIAL_FILTER":
				case "SPATIAL_INDEX":
				case "TABLEGEOMETRY":
				case "TABLESTYLE":
				case "TABLESTYLES":
				case "VBA_PROJECT":
				case "VISUALSTYLE":
				case "WIPEOUT":
				case "WIPEOUTVARIABLE":
				case "WIPEOUTVARIABLES":
				case "XRECORD":
					//System.Diagnostics.Debug.Fail($"Not implemented dxf class: {c.DxfName}");
					break;
				default:
					//System.Diagnostics.Debug.Fail($"Not implemented dxf class: {c.DxfName}");
					break;
			}

			return template;
		}

		#region Text entities
		private DwgTemplate readText()
		{
			Text text = new Text();
			DwgTextEntityTemplate template = new DwgTextEntityTemplate(text);
			readCommonTextData(template);

			return template;
		}
		private DwgTemplate readAttribute()
		{
			Attribute att = new Attribute();
			DwgTextEntityTemplate template = new DwgTextEntityTemplate(att);
			readCommonTextData(template);

			readCommonAttData(att);

			return template;
		}
		private DwgTemplate readAttributeDefinition()
		{
			AttributeDefinition attdef = new AttributeDefinition();
			DwgTextEntityTemplate template = new DwgTextEntityTemplate(attdef);
			readCommonTextData(template);

			readCommonAttData(attdef);

			//R2010+:
			if (R2010Plus)
				//Version RC ?		Repeated??
				attdef.Version = m_objectReader.ReadByte();
			//Common:
			//Prompt TV 3
			attdef.Prompt = m_textReader.ReadVariableText();

			return template;
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
		private void readCommonAttData(AttributeBase att)
		{
			//R2010+:
			if (R2010Plus)
				//Version RC ?
				att.Version = m_objectReader.ReadByte();

			//R2018+:
			if (R2018Plus)
			{
				AttributeType type = (AttributeType)m_objectReader.ReadByte();

				switch (type)
				{
					case AttributeType.SingleLine:
						//Common:
						//Tag TV 2
						att.Tag = m_textReader.ReadVariableText();
						//Field length BS 73 unused
						short length = m_objectReader.ReadBitShort();
						//Flags RC 70 NOT bit-pair - coded.
						att.Flags = (AttributeFlags)m_objectReader.ReadByte();
						//R2007 +:
						if (R2007Plus)
							//Lock position flag B 280
							att.IsReallyLocked = m_objectReader.ReadBit();

						break;
					case AttributeType.MultiLine:
					case AttributeType.ConstantMultiLine:
						//Attribute type is multi line
						//MTEXT fields … Here all fields of an embedded MTEXT object
						//are written, starting from the Entmode
						//(entity mode). The owner handle can be 0.

						//TODO: Read MText data
						System.Diagnostics.Debug.Fail("Reader not implemented for MText attribute.");
						return;

						short dataSize = m_objectReader.ReadBitShort();
						if (dataSize > 0)
						{
							//Annotative data bytes RC Byte array with length Annotative data size.
							m_objectReader.Advance(dataSize);
							//Registered application H Hard pointer.
							handleReference();
							//Unknown BS 72? Value 0.
							m_objectReader.ReadBitShort();
						}
						break;
					default:
						break;
				}
			}
		}
		#endregion

		private DwgTemplate readBlock()
		{
			BlockBegin block = new BlockBegin();
			DwgEntityTemplate template = new DwgEntityTemplate(block);

			readCommonEntityData(template);

			//Block name TV 2
			block.Name = m_textReader.ReadVariableText();

			return template;
		}

		private DwgTemplate readSeqend()
		{
			//TODO: is seqend necessary??

			return null;
		}

		#region Insert methods
		private DwgTemplate readInsert()
		{
			DwgInsertTemplate template = new DwgInsertTemplate(new Insert());

			readInsertCommonData(template);
			readInsertCommonHandles(template);

			return template;
		}
		private DwgTemplate readMInsert()
		{
			Insert insert = new Insert();
			DwgInsertTemplate template = new DwgInsertTemplate(insert);

			readInsertCommonData(template);

			//Common:
			//Numcols BS 70
			insert.ColumnCount = (ushort)m_objectReader.ReadBitShort();
			//Numrows BS 71
			insert.RowCount = (ushort)m_objectReader.ReadBitShort();
			//Col spacing BD 44
			insert.ColumnSpacing = m_objectReader.ReadBitDouble();
			//Row spacing BD 45
			insert.RowSpacing = m_objectReader.ReadBitDouble();

			readInsertCommonHandles(template);

			return template;
		}

		private void readInsertCommonData(DwgInsertTemplate template)
		{
			Insert e = template.CadObject as Insert;

			readCommonEntityData(template);

			//Ins pt 3BD 10
			e.InsertPoint = m_objectReader.Read3BitDouble();

			//R13-R14 Only:
			if (R13_14Only)
			{
				//X Scale BD 41
				//Y Scale BD 42
				//Z Scale BD 43
				e.Scale = m_objectReader.Read3BitDouble();
			}

			//R2000 + Only:
			if (R2000Plus)
			{
				double x = 1.0;
				double y = 1.0;
				double z = 1.0;

				//Data flags BB
				//Scale Data Varies with Data flags:
				switch (m_objectReader.Read2Bits())
				{
					//00 – 41 value stored as a RD, followed by a 42 value stored as DD (use 41 for default value), and a 43 value stored as a DD(use 41 value for default value).
					case 0:
						x = m_objectReader.ReadDouble();
						y = m_objectReader.ReadBitDoubleWithDefault(x);
						z = m_objectReader.ReadBitDoubleWithDefault(x);
						e.Scale = new XYZ(x, y, z);
						break;
					//01 – 41 value is 1.0, 2 DD’s are present, each using 1.0 as the default value, representing the 42 and 43 values.
					case 1:
						y = m_objectReader.ReadBitDoubleWithDefault(x);
						z = m_objectReader.ReadBitDoubleWithDefault(x);
						e.Scale = new XYZ(x, y, z);
						break;
					//10 – 41 value stored as a RD, and 42 & 43 values are not stored, assumed equal to 41 value.
					case 2:
						double xyz = m_objectReader.ReadDouble();
						e.Scale = new XYZ(xyz, xyz, xyz);
						break;
					//11 - scale is (1.0, 1.0, 1.0), no data stored.
					case 3:
						e.Scale = new XYZ(x, y, z);
						break;
				}
			}

			//Common:
			//Rotation BD 50
			e.Rotation = m_objectReader.ReadBitDouble();
			//Extrusion 3BD 210
			e.Normal = m_objectReader.Read3BitDouble();
			//Has ATTRIBs B 66 Single bit; 1 if ATTRIBs follow.
			template.HasAtts = m_objectReader.ReadBit();
			template.OwnedObjectsCount = 0;

			//R2004+:
			if (R2004Plus & template.HasAtts)
				//Owned Object Count BL Number of objects owned by this object.
				template.OwnedObjectsCount = m_objectReader.ReadBitLong();
		}
		private void readInsertCommonHandles(DwgInsertTemplate template)
		{
			//Common:
			//Common Entity Handle Data
			//H 2 BLOCK HEADER(hard pointer)
			template.BlockHeaderHandle = handleReference();

			if (!template.HasAtts)
				return;

			//R13 - R200:
			if (m_version >= ACadVersion.AC1012 && m_version <= ACadVersion.AC1015)
			{
				//H[1st ATTRIB(soft pointer)] if 66 bit set; can be NULL
				template.FirstAttributeHandle = handleReference();
				//H[last ATTRIB](soft pointer)] if 66 bit set; can be NULL
				template.EndAttributeHandle = handleReference();
			}
			//R2004:
			else if (R2004Plus)
			{
				for (int i = 0; i < template.OwnedObjectsCount; ++i)
					//H[ATTRIB(hard owner)] Repeats “Owned Object Count” times.
					template.OwnedHandles.Add(handleReference());
			}

			//Common:
			//H[SEQEND(hard owner)] if 66 bit set
			template.SeqendHandle = handleReference();
		}
		#endregion

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
		private DwgTemplate readPfaceVertex()
		{
			//TODO: Implement poly face vertex class
			Vertex vertex = new Vertex();
			DwgEntityTemplate template = new DwgEntityTemplate(vertex);

			readCommonEntityData(template);

			//Vert index BS 71 1 - based vertex index(see DXF doc)
			m_objectReader.ReadBitShort();
			//Vert index BS 72 1 - based vertex index(see DXF doc)
			m_objectReader.ReadBitShort();
			//Vert index BS 73 1 - based vertex index(see DXF doc)
			m_objectReader.ReadBitShort();
			//Vert index BS 74 1 - based vertex index(see DXF doc)
			m_objectReader.ReadBitShort();

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
		private DwgTemplate readDimOrdinate()
		{
			return null;
		}
		private DwgTemplate readDimLinear()
		{
			return null;
		}
		private DwgTemplate readDimAligned()
		{
			return null;
		}
		private DwgTemplate readDimAngular3pt()
		{
			return null;
		}
		private DwgTemplate readDimLine2pt()
		{
			return null;
		}
		private DwgTemplate readDimRadius()
		{
			return null;
		}
		private DwgTemplate readDimDiameter()
		{
			return null;
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
		private DwgTemplate read3dFace()
		{
			Face3D face = new Face3D();
			DwgEntityTemplate template = new DwgEntityTemplate(face);

			readCommonEntityData(template);

			//R13 - R14 Only:
			if (R13_14Only)
			{
				//1st corner 3BD 10
				face.FirstCorner = m_objectReader.Read3BitDouble();
				//2nd corner 3BD 11
				face.SecondCorner = m_objectReader.Read3BitDouble();
				//3rd corner 3BD 12
				face.ThirdCorner = m_objectReader.Read3BitDouble();
				//4th corner 3BD 13
				face.FourthCorner = m_objectReader.Read3BitDouble();
				//Invis flags BS 70 Invisible edge flags
				face.Flags = (InvisibleEdgeFlags)m_objectReader.ReadBitShort();
			}

			//R2000 +:
			if (R2000Plus)
			{
				//Has no flag ind. B
				bool noFlags = m_objectReader.ReadBit();
				//Z is zero bit B
				bool zIsZero = m_objectReader.ReadBit();

				//1st corner x RD 10
				double x = m_objectReader.ReadDouble();
				//1st corner y RD 20
				double y = m_objectReader.ReadDouble();
				//1st corner z RD 30 Present only if “Z is zero bit” is 0.
				double z = 0.0;

				if (!zIsZero)
					z = m_objectReader.ReadDouble();

				face.FirstCorner = new XYZ(x, y, z);

				//2nd corner 3DD 11 Use 10 value as default point
				//3rd corner 3DD 12 Use 11 value as default point
				//4th corner 3DD 13 Use 12 value as default point
				//Invis flags BS 70 Present it “Has no flag ind.” is 0.
			}

			return template;
		}
		private DwgTemplate readPolylinePface()
		{
			return null;
		}
		private DwgTemplate readPolylineMesh()
		{
			return null;
		}
		private DwgTemplate readSolid()
		{
			return null;
		}
		private DwgTemplate readShape()
		{
			return null;
		}
		private DwgTemplate readViewport()
		{
			Viewport viewport = new Viewport();
			DwgViewportTemplate template = new DwgViewportTemplate(viewport);

			//Common Entity Data
			readCommonEntityData(template);

			//Center 3BD 10
			viewport.Center = m_objectReader.Read3BitDouble();
			//Width BD 40
			viewport.Width = m_objectReader.ReadBitDouble();
			//Height BD 41
			viewport.Height = m_objectReader.ReadBitDouble();

			//R2000 +:
			if (R2000Plus)
			{
				//View Target 3BD 17
				viewport.ViewTarget = m_objectReader.Read3BitDouble();
				//View Direction 3BD 16
				viewport.ViewDirection = m_objectReader.Read3BitDouble();
				//View Twist Angle BD 51
				viewport.TwistAngle = m_objectReader.ReadBitDouble();
				//View Height BD 45
				viewport.ViewHeight = m_objectReader.ReadBitDouble();
				//Lens Length BD 42
				viewport.LensLength = m_objectReader.ReadBitDouble();
				//Front Clip Z BD 43
				viewport.FrontClipPlane = m_objectReader.ReadBitDouble();
				//Back Clip Z BD 44
				viewport.BackClipPlane = m_objectReader.ReadBitDouble();
				//Snap Angle BD 50
				viewport.SnapAngle = m_objectReader.ReadBitDouble();
				//View Center 2RD 12
				viewport.ViewCenter = m_objectReader.Read2RawDouble();
				//Snap Base 2RD 13
				viewport.SnapBase = m_objectReader.Read2RawDouble();
				//Snap Spacing 2RD 14
				viewport.SnapSpacing = m_objectReader.Read2RawDouble();
				//Grid Spacing 2RD 15
				viewport.GridSpacing = m_objectReader.Read2RawDouble();
				//Circle Zoom BS 72
				viewport.CircleZoomPercent = m_objectReader.ReadBitShort();
			}

			//R2007 +:
			if (R2007Plus)
				//Grid Major BS 61
				viewport.MajorGridLineFrequency = m_objectReader.ReadBitShort();

			int frozenLayerCount = 0;
			//R2000 +:
			if (R2000Plus)
			{
				//Frozen Layer Count BL
				frozenLayerCount = m_objectReader.ReadBitLong();
				//Status Flags BL 90
				viewport.Status = (ViewportStatusFlags)m_objectReader.ReadBitLong();
				//Style Sheet TV 1
				viewport.StyleSheetName = m_textReader.ReadVariableText();
				//Render Mode RC 281
				viewport.RenderMode = (RenderMode)m_objectReader.ReadByte();
				//UCS at origin B 74
				viewport.DisplayUcsIcon = m_objectReader.ReadBit();
				//UCS per Viewport B 71
				viewport.UcsPerViewport = m_objectReader.ReadBit();
				//UCS Origin 3BD 110
				viewport.UcsOrigin = m_objectReader.Read3BitDouble();
				//UCS X Axis 3BD 111
				viewport.UcsXAxis = m_objectReader.Read3BitDouble();
				//UCS Y Axis 3BD 112
				viewport.UcsYAxis = m_objectReader.Read3BitDouble();
				//UCS Elevation BD 146
				viewport.Elevation = m_objectReader.ReadBitDouble();
				//UCS Ortho View Type BS 79
				viewport.UcsOrthographicType = (OrthographicType)m_objectReader.ReadBitShort();
			}

			//R2004 +:
			if (R2004Plus)
				//ShadePlot Mode BS 170
				viewport.ShadePlotMode = (ShadePlotMode)m_objectReader.ReadBitShort();

			//R2007 +:
			if (R2007Plus)
			{
				//Use def. lights B 292
				viewport.UseDefaultLighting = m_objectReader.ReadBit();
				//Def.lighting type RC 282
				viewport.DefaultLightingType = (LightingType)m_objectReader.ReadByte();
				//Brightness BD 141
				viewport.Brightness = m_objectReader.ReadBitDouble();
				//Contrast BD 142
				viewport.Constrast = m_objectReader.ReadBitDouble();
				//Ambient light color CMC 63
				viewport.AmbientLightColor = m_objectReader.ReadCmColor();
			}

			//R13 - R14 Only:
			if (R13_14Only)
				//H VIEWPORT ENT HEADER(hard pointer)
				template.ViewportHeaderHandle = handleReference();

			//R2000 +:
			if (R2000Plus)
			{
				for (int i = 0; i < frozenLayerCount; ++i)
					//H 341 Frozen Layer Handles(use count from above)(hard pointer until R2000, soft pointer from R2004 onwards)
					template.FrozenLayerHandles.Add(handleReference());

				//H 340 Clip boundary handle(soft pointer)
				template.BoundaryHandle = handleReference();
			}

			//R2000:
			if (m_version == ACadVersion.AC1015)
				//H VIEWPORT ENT HEADER((hard pointer))
				template.ViewportHeaderHandle = handleReference();

			//R2000 +:
			if (R2000Plus)
			{
				//H 345 Named UCS Handle(hard pointer)
				template.NamedUcsHandle = handleReference();
				//H 346 Base UCS Handle(hard pointer)
				template.BaseUcsHandle = handleReference();
			}

			//R2007 +:
			if (!R2007Plus)
			{
				//H 332 Background(soft pointer)
				long backgroundHandle = (long)handleReference();
				//H 348 Visual Style(hard pointer)
				long visualStyleHandle = (long)handleReference();
				//H 333 Shadeplot ID(soft pointer)
				long shadePlotIdHandle = (long)handleReference();
				//H 361 Sun(hard owner)
				long sunHandle = (long)handleReference();
			}

			return template;
		}
		private DwgTemplate readEllipse()
		{
			Ellipse ellipse = new Ellipse();
			DwgEntityTemplate template = new DwgEntityTemplate(ellipse);

			readCommonEntityData(template);

			//Center 3BD 10 (WCS)
			ellipse.Center = m_objectReader.Read3BitDouble();
			//SM axis vec 3BD 11 Semi-major axis vector (WCS)
			var smaxis = m_objectReader.Read3BitDouble();
			//Extrusion 3BD 210
			ellipse.Normal = m_objectReader.Read3BitDouble();
			//Axis ratio BD 40 Minor/major axis ratio
			ellipse.RadiusRatio = m_objectReader.ReadBitDouble();
			//Beg angle BD 41 Starting angle (eccentric anomaly, radians)
			ellipse.StartParameter = m_objectReader.ReadBitDouble();
			//End angle BD 42 Ending angle (eccentric anomaly, radians)
			ellipse.EndParameter = m_objectReader.ReadBitDouble();

			return template;
		}
		private DwgTemplate readSpline()
		{
			return null;
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
				string name = m_textReader.ReadVariableText();
				//Handle refs H parenthandle (soft relative pointer)
				//[Reactors(soft pointer)]
				//xdicobjhandle(hard owner)
				//itemhandles (soft owner)
				ulong handle = handleReference();

				template.HandleEntries.Add(handle, name);
			}

			return template;
		}

		private DwgTemplate readMText()
		{
			MText mtext = new MText();
			DwgTextEntityTemplate template = new DwgTextEntityTemplate(mtext);

			readCommonEntityData(template);

			//Insertion pt3 BD 10 First picked point. (Location relative to text depends on attachment point (71).)
			mtext.InsertPoint = m_objectReader.Read3BitDouble();
			//Extrusion 3BD 210 Undocumented; appears in DXF and entget, but ACAD doesn't even bother to adjust it to unit length.
			mtext.Normal = m_objectReader.Read3BitDouble();
			//X-axis dir 3BD 11 Apparently the text x-axis vector. (Why not just a rotation?) ACAD maintains it as a unit vector.
			mtext.AlignmentPoint = m_objectReader.Read3BitDouble();
			//Rect width BD 41 Reference rectangle width (width picked by the user).
			mtext.RectangleWitdth = m_objectReader.ReadBitDouble();

			//R2007+:
			if (R2007Plus)
			{
				//Rect height BD 46 Reference rectangle height.
				mtext.RectangleHeight = m_objectReader.ReadBitDouble();
			}

			//Common:
			//Text height BD 40 Undocumented
			mtext.Height = m_objectReader.ReadBitDouble();
			//Attachment BS 71 Similar to justification; see DXF doc
			mtext.AttachmentPoint = (AttachmentPointType)m_objectReader.ReadBitShort();
			//Drawing dir BS 72 Left to right, etc.; see DXF doc
			mtext.DrawingDirection = (DrawingDirectionType)m_objectReader.ReadBitShort();
			//Extents ht BD ---Undocumented and not present in DXF or entget
			m_objectReader.ReadBitDouble();
			//Extents wid BD ---Undocumented and not present in DXF or entget
			m_objectReader.ReadBitDouble();
			//Text TV 1 All text in one long string (Autocad format)
			mtext.Value = m_textReader.ReadVariableText();

			//H 7 STYLE (hard pointer)
			template.StyleHandle = handleReference();

			//R2000+:
			if (R2000Plus)
			{
				//Linespacing Style BS 73
				mtext.LineSpacingStyle = (LineSpacingStyleType)m_objectReader.ReadBitShort();
				//Linespacing Factor BD 44
				mtext.LineSpacing = m_objectReader.ReadBitDouble();
				//Unknown bit B
				m_objectReader.ReadBit();
			}

			//R2004+:
			if (R2004Plus)
			{
				//Background flags BL 90 0 = no background, 1 = background fill, 2 = background fill with drawing fill color, 0x10 = text frame (R2018+)
				mtext.BackgroundFillFlags = (BackgroundFillFlags)m_objectReader.ReadBitLong();

				//background flags has bit 0x01 set, or in case of R2018 bit 0x10:
				if ((mtext.BackgroundFillFlags & BackgroundFillFlags.UseBackgroundFillColor) != BackgroundFillFlags.None
					|| m_version > ACadVersion.AC1027
					&& (mtext.BackgroundFillFlags & BackgroundFillFlags.TextFrame) > 0)
				{
					//Background scale factor	BL 45 default = 1.5
					mtext.BackgroundScale = m_objectReader.ReadBitDouble();
					//Background color CMC 63
					mtext.BackgroundColor = m_mergedReaders.ReadCmColor();
					//Background transparency BL 441
					mtext.BackgroundTransparency = new Transparency((short)m_objectReader.ReadBitLong());
				}
			}

			//R2018+
			if (!R2018Plus)
				return template;

			//Is NOT annotative B
			mtext.IsAnnotative = !m_objectReader.ReadBit();

			//IF MTEXT is not annotative
			if (!mtext.IsAnnotative)
			{
				//Version BS Default 0
				var version = m_objectReader.ReadBitShort();
				//Default flag B Default true
				var defaultFlag = m_objectReader.ReadBit();

				//BEGIN REDUNDANT FIELDS(see above for descriptions)
				//Registered application H Hard pointer
				ulong appHandle = handleReference();

				//TODO: finish Mtext reader, save redundant fields??

				//Attachment point BL
				AttachmentPointType attachmentPoint = (AttachmentPointType)m_objectReader.ReadBitLong();
				//X - axis dir 3BD 10
				m_objectReader.Read3BitDouble();
				//Insertion point 3BD 11
				m_objectReader.Read3BitDouble();
				//Rect width BD 40
				m_objectReader.ReadBitDouble();
				//Rect height BD 41
				m_objectReader.ReadBitDouble();
				//Extents width BD 42
				m_objectReader.ReadBitDouble();
				//Extents height BD 43
				m_objectReader.ReadBitDouble();
				//END REDUNDANT FIELDS

				//Column type BS 71 0 = No columns, 1 = static columns, 2 = dynamic columns
				mtext.ColumnType = (ColumnType)m_objectReader.ReadBitShort();
				//IF Has Columns data(column type is not 0)
				if (mtext.ColumnType != ColumnType.NoColumns)
				{
					//Column height count BL 72
					int count = m_objectReader.ReadBitLong();
					//Columnn width BD 44
					mtext.ColumnWidth = m_objectReader.ReadBitDouble();
					//Gutter BD 45
					mtext.ColumnGutter = m_objectReader.ReadBitDouble();
					//Auto height? B 73
					mtext.ColumnAutoHeight = m_objectReader.ReadBit();
					//Flow reversed? B 74
					mtext.ColumnFlowReversed = m_objectReader.ReadBit();

					//IF not auto height and column type is dynamic columns
					if (!mtext.ColumnAutoHeight && mtext.ColumnType == ColumnType.DynamicColumns && count > 0)
					{
						for (int i = 0; i < count; ++i)
						{
							//Column height BD 46
							mtext.ColumnHeights.Add(m_objectReader.ReadBitDouble());
						}
					}
				}
			}

			return template;
		}

		private DwgTemplate readBlockControlObject()
		{
			//TODO: Fix the block control object reader
			DwgBlockCtrlObjectTemplate template = new DwgBlockCtrlObjectTemplate();

			_modelBuilder.BlockControlTemplate = template;

			readCommonNonEntityData(template);

			//Common:
			//Numentries BL 70 Doesn't count *MODEL_SPACE and *PAPER_SPACE.
			var nentries = m_objectReader.ReadBitLong();
			for (int i = 0; i < nentries; i++)
			{
				//xdicobjhandle (hard owner)
				template.Handles.Add(handleReference());
			}

			//*MODEL_SPACE and *PAPER_SPACE(hard owner).
			template.ModelSpaceHandle = handleReference();
			template.PaperSpaceHandle = handleReference();

			return template;
		}
		private DwgTemplate readBlockHeader()
		{
			Block block = new Block();
			DwgBlockTemplate template = new DwgBlockTemplate(block);

			_modelBuilder.BlockHeaders.Add(template);

			readCommonNonEntityData(template);

			//Common:
			//Entry name TV 2
			block.Name = m_textReader.ReadVariableText();

			readXrefDependantBit((TableEntry)template.CadObject);

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
			if (R2004Plus && !block.IsXref && !block.IsXRefOverlay)
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

			//NULL(hard pointer)
			handleReference();
			//BLOCK entity. (hard owner)
			template.HardOwnerHandle = handleReference();

			//R13-R2000:
			if (m_version >= ACadVersion.AC1012 && m_version <= ACadVersion.AC1015
				&& !block.IsXref && !block.IsXRefOverlay)
			{
				//first entity in the def. (soft pointer)
				template.FirstEntityHandle = handleReference();
				//last entity in the def. (soft pointer)
				template.SecondEntityHandle = handleReference();
			}

			//R2004+:
			if (R2004Plus)
			{
				for (int i = 0; i < nownedObjects; ++i)
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
			//layer.Name = m_objectReader.ReadVariableText();

			readXrefDependantBit((TableEntry)template.CadObject);

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
			Style style = Style.Default;
			DwgTableEntryTemplate<Style> template = new DwgTableEntryTemplate<Style>(style);

			readCommonNonEntityData(template);

			//Common:
			//Entry name TV 2
			style.Name = m_textReader.ReadVariableText();
			readXrefDependantBit((TableEntry)template.CadObject);

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
			DwgTableEntryTemplate<LineType> template = new DwgTableEntryTemplate<LineType>(ltype);

			readCommonNonEntityData(template);

			//Common:
			//Entry name TV 2
			ltype.Name = m_textReader.ReadVariableText();

			readXrefDependantBit((TableEntry)template.CadObject);

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

		private DwgTemplate readAppIdControlObject()
		{
			_modelBuilder.DocumentToBuild.AppIds = new Tables.Collections.AppIdsTable();
			DwgTableTemplate<AppId> template = new DwgTableTemplate<AppId>(
				_modelBuilder.DocumentToBuild.AppIds);
			_modelBuilder.AppIds = template;

			readCommonNonEntityData(template);

			//Common:
			//Numentries BL 70
			int numentries = m_objectReader.ReadBitLong();
			for (int i = 0; i < numentries; ++i)
				//Handle refs H NULL(soft pointer)	xdicobjhandle(hard owner)	the apps(soft owner)
				template.EntryHandles.Add(handleReference());

			return template;
		}

		private DwgTemplate readAppId()
		{
			AppId dxfAppId = new AppId();
			DwgTemplate template = new DwgTemplate<AppId>(dxfAppId);

			readCommonNonEntityData(template);

			dxfAppId.Name = m_textReader.ReadVariableText();

			readXrefDependantBit(dxfAppId);

			//Unknown RC 71 Undoc'd 71-group; doesn't even appear in DXF or an entget if it's 0.
			m_objectReader.ReadByte();
			//Handle refs H The app control(soft pointer)
			//[Reactors(soft pointer)]
			//xdicobjhandle(hard owner)
			//External reference block handle(hard pointer)
			handleReference();

			return template;
		}

		private DwgTemplate readHatch()
		{
			Hatch hatch = new Hatch();
			DwgHatchTemplate template = new DwgHatchTemplate(hatch);

			readCommonEntityData(template);

			//R2004+:
			if (R2004Plus)
			{
				HatchGradientPattern gradient = new HatchGradientPattern(null);

				//Is Gradient Fill BL 450 Non-zero indicates a gradient fill is used.
				bool enabled = m_objectReader.ReadBitLong() != 0;

				//Reserved BL 451
				gradient.Reserved = m_objectReader.ReadBitLong();
				//Gradient Angle BD 460
				gradient.Angle = m_objectReader.ReadBitDouble();
				//Gradient Shift BD 461
				gradient.Shift = m_objectReader.ReadBitDouble();
				//Single Color Grad.BL 452
				gradient.IsSingleColorGradient = (uint)m_objectReader.ReadBitLong() > 0U;
				//Gradient Tint BD 462
				gradient.ColorTint = m_objectReader.ReadBitDouble();

				//# of Gradient Colors BL 453
				int ncolors = m_objectReader.ReadBitLong();
				for (int i = 0; i < ncolors; ++i)
				{
					//Unknown double BD 463
					var value = m_objectReader.ReadBitDouble();
					//RGB Color
					var color = m_mergedReaders.ReadCmColor();

					gradient.Colors.Add(color);
				}

				//Gradient Name TV 470
				gradient.Name = m_textReader.ReadVariableText();

				//Set the pattern if is enabled
				if (enabled)
					hatch.Pattern = gradient;
			}

			//Common:
			//Z coord BD 30 X, Y always 0.0
			hatch.Elevation = m_objectReader.ReadBitDouble();
			//Extrusion 3BD 210
			hatch.Normal = m_objectReader.Read3BitDouble();
			//Name TV 2 name of hatch
			hatch.Pattern = new HatchPattern(m_textReader.ReadVariableText());
			//Solidfill B 70 1 if solidfill, else 0
			hatch.IsSolid = m_objectReader.ReadBit();
			//Associative B 71 1 if associative, else 0
			hatch.IsAssociative = m_objectReader.ReadBit();

			//Numpaths BL 91 Number of paths enclosing the hatch
			int npaths = m_objectReader.ReadBitLong();
			bool hasDerivedBoundary = false;

			#region Read the boundary path data
			//int numboundaryobjhandles = 0;
			for (int i = 0; i < npaths; i++)
			{
				DwgHatchTemplate.DwgBoundaryPathTemplate pathTemplate = new DwgHatchTemplate.DwgBoundaryPathTemplate();

				//Pathflag BL 92 Path flag
				pathTemplate.Path.Flags = (BoundaryPathFlags)m_mergedReaders.ReadBitLong();

				if (pathTemplate.Path.Flags.HasFlag(BoundaryPathFlags.Derived))
					hasDerivedBoundary = true;

				if (!pathTemplate.Path.Flags.HasFlag(BoundaryPathFlags.Polyline))
				{
					//Numpathsegs BL 93 number of segments in this path
					int nsegments = m_mergedReaders.ReadBitLong();
					for (int j = 0; j < nsegments; ++j)
					{
						//pathtypestatus RC 72 type of path
						byte pathTypeStatus = m_mergedReaders.ReadByte();
						switch (pathTypeStatus)
						{
							case 1:
								pathTemplate.Path.Edges.Add(new HatchBoundaryPath.Line
								{
									//pt0 2RD 10 first endpoint
									Start = m_mergedReaders.Read2RawDouble(),
									//pt1 2RD 11 second endpoint
									End = m_mergedReaders.Read2RawDouble()
								});
								break;
							case 2:
								pathTemplate.Path.Edges.Add(new HatchBoundaryPath.Arc
								{
									//pt0 2RD 10 center
									Center = m_mergedReaders.Read2RawDouble(),
									//radius BD 40 radius
									Radius = m_mergedReaders.ReadBitDouble(),
									//startangle BD 50 start angle
									StartAngle = m_mergedReaders.ReadBitDouble(),
									//endangle BD 51 endangle
									EndAngle = m_mergedReaders.ReadBitDouble(),
									//isccw B 73 1 if counter clockwise, otherwise 0
									CounterClockWise = m_mergedReaders.ReadBit()
								});
								break;
							case 3:
								pathTemplate.Path.Edges.Add(new HatchBoundaryPath.Ellipse
								{
									//pt0 2RD 10 center
									Center = m_mergedReaders.Read2RawDouble(),
									//endpoint 2RD 11 endpoint of major axis
									MajorAxisEndPoint = m_mergedReaders.Read2RawDouble(),
									//minormajoratio BD 40 ratio of minor to major axis
									MinorToMajorRatio = m_mergedReaders.ReadBitDouble(),
									//startangle BD 50 start angle
									StartAngle = m_mergedReaders.ReadBitDouble(),
									//endangle BD 51 endangle
									EndAngle = m_mergedReaders.ReadBitDouble(),
									//isccw B 73 1 if counter clockwise, otherwise 0
									CounterClockWise = m_mergedReaders.ReadBit()
								});
								break;
							case 4:
								HatchBoundaryPath.Spline splineEdge = new HatchBoundaryPath.Spline();
								//degree BL 94 degree of the spline
								splineEdge.Degree = m_mergedReaders.ReadBitLong();
								//isrational B 73 1 if rational(has weights), else 0
								splineEdge.Rational = m_mergedReaders.ReadBit();
								//isperiodic B 74 1 if periodic, else 0
								splineEdge.Periodic = m_mergedReaders.ReadBit();

								//numknots BL 95 number of knots
								int numknots = m_mergedReaders.ReadBitLong();
								//numctlpts BL 96 number of control points
								int numctlpts = m_mergedReaders.ReadBitLong();

								for (int k = 0; k < numknots; ++k)
									//knot BD 40 knot value
									splineEdge.Knots.Add(m_mergedReaders.ReadBitDouble());

								for (int p = 0; p < numctlpts; ++p)
								{
									//pt0 2RD 10 control point
									var cp = m_mergedReaders.Read2RawDouble();

									double wheight = 0;
									if (splineEdge.Rational)
										//weight BD 40 weight
										wheight = m_mergedReaders.ReadBitDouble();

									//Add the control point and its wheight 
									splineEdge.ControlPoints.Add(new XYZ(cp.X, cp.Y, wheight));
								}

								//R24:
								if (R2010Plus)
								{
									//Numfitpoints BL 97 number of fit points
									int nfitPoints = m_mergedReaders.ReadBitLong();
									if (nfitPoints > 0)
									{
										for (int fp = 0; fp < nfitPoints; ++fp)
										{
											//Fitpoint 2RD 11
											XY fpoint = m_mergedReaders.Read2RawDouble();
										}

										//Start tangent 2RD 12
										XY startTangent = m_mergedReaders.Read2RawDouble();
										//End tangent 2RD 13
										XY endTangent = m_mergedReaders.Read2RawDouble();
									}
								}

								//Add the spline 
								pathTemplate.Path.Edges.Add(splineEdge);
								break;
						}
					}
				}
				else    //POLYLINE PATH
				{
					HatchBoundaryPath.Polyline pline = new HatchBoundaryPath.Polyline();
					//bulgespresent B 72 bulges are present if 1
					bool bulgespresent = m_mergedReaders.ReadBit();
					//closed B 73 1 if closed
					pline.IsClosed = m_mergedReaders.ReadBit();

					//numpathsegs BL 91 number of path segments
					int numpathsegs = m_mergedReaders.ReadBitLong();
					for (int index = 0; index < numpathsegs; ++index)
					{
						//pt0 2RD 10 point on polyline
						XY vertex = m_mergedReaders.Read2RawDouble();

						double bulge = 0;
						if (bulgespresent)
							//bulge BD 42 bulge
							bulge = m_mergedReaders.ReadBitDouble();

						//Add the vertex 
						pline.Vertices.Add(new XYZ(vertex.X, vertex.Y, bulge));
					}
				}

				//numboundaryobjhandles BL 97 Number of boundary object handles for this path
				int numboundaryobjhandles = m_objectReader.ReadBitLong();

				//TODO: Add the path to the hatch (this has handles)
				for (int h = 0; h < numboundaryobjhandles; h++)
				{
					pathTemplate.Handles.Add(handleReference());
				}

				template.AddPath(pathTemplate);
			}
			#endregion

			//style BS 75 style of hatch 0==odd parity, 1==outermost, 2==whole area
			hatch.HatchStyle = (HatchStyleType)m_objectReader.ReadBitShort();
			//patterntype BS 76 pattern type 0==user-defined, 1==predefined, 2==custom
			hatch.HatchPatternType = (HatchPatternType)m_objectReader.ReadBitShort();

			if (hatch.IsSolid)
			{
				//angle BD 52 hatch angle
				hatch.PatternAngle = m_objectReader.ReadBitDouble();
				//scaleorspacing BD 41 scale or spacing(pattern fill only)
				hatch.PatternScale = m_objectReader.ReadBitDouble();
				//doublehatch B 77 1 for double hatch
				hatch.IsDouble = m_objectReader.ReadBit();

				//numdeflines BS 78 number of definition lines
				int numdeflines = (int)m_objectReader.ReadBitShort();
				for (int li = 0; li < numdeflines; ++li)
				{
					HatchPattern.Line line = new HatchPattern.Line();
					//angle BD 53 line angle
					line.Angle = m_objectReader.ReadBitDouble();
					//pt0 2BD 43 / 44 pattern through this point(X, Y)
					line.BasePoint = m_objectReader.Read2BitDouble();
					//offset 2BD 45 / 56 pattern line offset
					line.Offset = m_objectReader.Read2BitDouble();

					//  numdashes BS 79 number of dash length items
					int ndashes = (int)m_objectReader.ReadBitShort();
					for (int ds = 0; ds < ndashes; ++ds)
					{
						//dashlength BD 49 dash length
						line.DashLengths.Add(m_objectReader.ReadBitDouble());
					}

					hatch.Pattern.Lines.Add(line);
				}
			}

			if (hasDerivedBoundary)
				//pixelsize BD 47 pixel size
				hatch.PixelSize = m_objectReader.ReadBitDouble();

			//numseedpoints BL 98 number of seed points
			int numseedpoints = m_objectReader.ReadBitLong();
			for (int sp = 0; sp < numseedpoints; ++sp)
			{
				//pt0 2RD 10 seed point
				XY spt = m_objectReader.Read2RawDouble();
				hatch.SeedPoints.Add(spt);
			}

			return template;
		}

		private DwgTemplate readLayout()
		{
			//TODO: layout pag 210
			Layout layout = new Layout();
			DwgLayoutTemplate template = new DwgLayoutTemplate(layout);

			readCommonNonEntityData(template);

			layout.PlotSettings = readPlotSettings();

			//Common:
			//Layout name TV 1 layout name
			layout.Name = m_textReader.ReadVariableText();
			//Tab order BL 71 layout tab order
			layout.TabOrder = m_objectReader.ReadBitLong();
			//Flag BS 70 layout flags
			layout.Flags = (LayoutFlags)m_objectReader.ReadBitShort();
			//Ucs origin 3BD 13 layout ucs origin
			layout.Origin = m_objectReader.Read3BitDouble();
			//Limmin 2RD 10 layout minimum limits
			layout.MinLimits = m_objectReader.Read2RawDouble();
			//Limmax 2RD 11 layout maximum limits
			layout.MaxLimits = m_objectReader.Read2RawDouble();
			//Inspoint 3BD 12 layout insertion base point
			layout.InsertionBasePoint = m_objectReader.Read3BitDouble();
			//Ucs x axis 3BD 16 layout ucs x axis direction
			layout.XAxis = m_objectReader.Read3BitDouble();
			//Ucs y axis 3BD 17 layout ucs y axis direction
			layout.YAxis = m_objectReader.Read3BitDouble();
			//Elevation BD 146 layout elevation
			layout.Elevation = m_objectReader.ReadBitDouble();
			//Orthoview type BS 76 layout orthographic view type of UCS
			layout.UcsOrthographicType = (OrthographicType)m_objectReader.ReadBitShort();
			//Extmin 3BD 14 layout extent min
			layout.MinExtents = m_objectReader.Read3BitDouble();
			//Extmax 3BD 15 layout extent max
			layout.MaxExtents = m_objectReader.Read3BitDouble();

			int nLayouts = 0;
			//R2004 +:
			if (R2004Plus)
				//Viewport count RL # of viewports in this layout
				nLayouts = m_objectReader.ReadBitLong();

			//Common:
			//330 associated paperspace block record handle(soft pointer)
			template.PaperSpaceBlockHandle = handleReference();
			//331 last active viewport handle(soft pointer)
			template.ActiveViewportHandle = handleReference();
			//346 base ucs handle(hard pointer)
			template.BaseUcsHandle = handleReference();
			//345 named ucs handle(hard pointer)
			template.NamesUcsHandle = handleReference();

			//R2004+:
			if (R2004Plus)
			{
				//Viewport handle(repeats Viewport count times) (soft pointer)
				for (int index = 0; index < nLayouts; ++index)
					template.ViewportHandles.Add(handleReference());
			}

			return template;
		}

		private PlotSettings readPlotSettings()
		{
			PlotSettings plot = new PlotSettings();

			//Common:
			//Page setup name TV 1 plotsettings page setup name
			plot.Name = m_textReader.ReadVariableText();
			//Printer / Config TV 2 plotsettings printer or configuration file
			plot.SystemPrinterName = m_textReader.ReadVariableText();
			//Plot layout flags BS 70 plotsettings plot layout flag
			plot.Flags = (PlotFlags)m_objectReader.ReadBitShort();

			PaperMargin margin = new PaperMargin()
			{
				//Left Margin BD 40 plotsettings left margin in millimeters
				Left = m_objectReader.ReadBitDouble(),
				//Bottom Margin BD 41 plotsettings bottom margin in millimeters
				Bottom = m_objectReader.ReadBitDouble(),
				//Right Margin BD 42 plotsettings right margin in millimeters
				Right = m_objectReader.ReadBitDouble(),
				//Top Margin BD 43 plotsettings top margin in millimeters
				Top = m_objectReader.ReadBitDouble()
			};
			plot.UnprintableMargin = margin;

			//Paper Width BD 44 plotsettings paper width in millimeters
			plot.PaperWidth = m_objectReader.ReadBitDouble();
			//Paper Height BD 45 plotsettings paper height in millimeters
			plot.PaperHeight = m_objectReader.ReadBitDouble();

			//Paper Size TV 4 plotsettings paper size
			plot.PaperSize = m_textReader.ReadVariableText();

			//Plot origin 2BD 46,47 plotsettings origin offset in millimeters
			plot.PlotOrigin = m_objectReader.Read2BitDouble();
			//Paper units BS 72 plotsettings plot paper units
			plot.PaperUnits = (PlotPaperUnits)m_objectReader.ReadBitShort();
			//Plot rotation BS 73 plotsettings plot rotation
			plot.PaperRotation = (PlotRotation)m_objectReader.ReadBitShort();
			//Plot type BS 74 plotsettings plot type
			plot.PlotType = (PlotType)m_objectReader.ReadBitShort();

			//Window min 2BD 48,49 plotsettings plot window area lower left
			plot.WindowLowerLeft = m_objectReader.Read2BitDouble();
			//Window max 2BD 140,141 plotsettings plot window area upper right
			plot.WindowUpperLeft = m_objectReader.Read2BitDouble();

			//R13 - R2000 Only:
			if (m_version >= ACadVersion.AC1012 && m_version <= ACadVersion.AC1015)
				//Plot view name T 6 plotsettings plot view name
				plot.PlotViewName = m_textReader.ReadVariableText();

			//Common:
			//Real world units BD 142 plotsettings numerator of custom print scale
			plot.NumeratorScale = m_objectReader.ReadBitDouble();
			//Drawing units BD 143 plotsettings denominator of custom print scale
			plot.DenominatorScale = m_objectReader.ReadBitDouble();
			//Current style sheet TV 7 plotsettings current style sheet
			plot.StyleSheet = m_textReader.ReadVariableText();
			//Scale type BS 75 plotsettings standard scale type
			plot.ScaledFit = (ScaledType)m_objectReader.ReadBitShort();
			//Scale factor BD 147 plotsettings scale factor
			plot.StandardScale = m_objectReader.ReadBitDouble();
			//Paper image origin 2BD 148,149 plotsettings paper image origin
			plot.PaperImageOrigin = m_objectReader.Read2BitDouble();

			//R2004+:
			if (R2004Plus)
			{
				//Shade plot mode BS 76
				plot.ShadePlotMode = (ShadePlotMode)m_objectReader.ReadBitShort();
				//Shade plot res.Level BS 77
				plot.ShadePlotResolutionMode = (ShadePlotResolutionMode)m_objectReader.ReadBitShort();
				//Shade plot custom DPI BS 78
				plot.ShadePlotDPI = m_objectReader.ReadBitShort();

				//6 plot view handle(hard pointer)
				ulong plotViewHandle = handleReference();
			}

			//R2007 +:
			if (R2007Plus)
				//Visual Style handle(soft pointer)
				handleReference();

			return plot;
		}

		#endregion

		private DwgTemplate readDwgColor()
		{
			DwgColorTemplate.DwgColor dwgColor = new DwgColorTemplate.DwgColor();
			DwgColorTemplate template = new DwgColorTemplate(dwgColor);

			readCommonNonEntityData(template);

			short colorIndex = m_objectReader.ReadBitShort();

			if (R2004Plus && m_version < ACadVersion.AC1032)
			{
				short index = (short)m_objectReader.ReadBitLong();
				byte flags = m_objectReader.ReadByte();

				if ((flags & 1U) > 0U)
					template.Name = m_textReader.ReadVariableText();

				if ((flags & 2U) > 0U)
					template.BookName = m_textReader.ReadVariableText();

				dwgColor.Color = new Color(index);
			}

			dwgColor.Color = new Color(colorIndex);

			return template;
		}
	}
}
