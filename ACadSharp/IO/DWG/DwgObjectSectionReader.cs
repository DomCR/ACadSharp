using ACadSharp.Entities;
using ACadSharp.Geometry;
using ACadSharp.IO.Templates;
using CSUtilities.IO;
using System;
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
		/// Document where the objects will be placed.
		/// </summary>
		private CadDocument m_document;
		/// <summary>
		/// During the object reading the handles will be added at the queue.
		/// </summary>
		private Queue<ulong> m_handles;
		private Dictionary<ulong, long> m_map;
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
			IDwgStreamReader reader, Queue<ulong> handles, Dictionary<ulong, long> handleMap) : base(version)
		{
			m_document = document;
			m_reader = reader;

			m_handles = new Queue<ulong>(handles);
			m_map = new Dictionary<ulong, long>(handleMap);

			//Initialize the crc stream
			//RS : CRC for the data section, starting after the sentinel. Use 0xC0C1 for the initial value.
			m_crcStream = new CRC8StreamHandler(m_reader.Stream, 0xC0C1);
			//Setup the entity handler
			m_crcReader = DwgStreamReader.GetStreamHandler(m_version, m_crcStream);
		}
		//**************************************************************************
		public void Read()
		{
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

				//Add the object to the map
				m_objectMap[template.CadObject.Handle] = template.CadObject;
				//Add the template to the list to be processed
				m_templates.Add(template);
			}
		}
		[Obsolete("Temporal method to test the reading of a single type object.")]
		public List<CadObject> Read(ObjectType type)
		{
			List<CadObject> objects = new List<CadObject>();

			foreach (long offset in m_map.Values)
			{
				ObjectType et = getEntityType(offset);

				if (et != type)
					continue;

				var template = readObject(et);

				objects.Add(template.CadObject);
			}

			return objects;
		}
		[Obsolete("Temporal method to test the reading of a single object by offset.")]
		public CadObject Read(long offset)
		{
			ObjectType et = getEntityType(offset);

			//TEXT ENTITIES OFFSETS:
			//	250511
			//	250939

			return readObject(et).CadObject;
		}
		//**************************************************************************
		private ObjectType getEntityType(long offset)
		{
			//Set the position to the entity to find
			m_crcReader.Position = offset;

			//MS : Size of object, not including the CRC
			ushort size = (ushort)m_crcReader.ReadModularShort();

			if (size <= 0U)
				return ObjectType.UNUSED;

			//remove the padding bits make sure the object stream ends on a byte boundary
			uint sizeWithoutPadding = (uint)(size << 3);

			//R2010+:
			if (m_version >= ACadVersion.AC1024)
			{
				//MC : Size in bits of the handle stream (unsigned, 0x40 is not interpreted as sign).
				ulong handleSize = m_crcReader.ReadModularChar();
				//Find the handles offset
				ulong handleSectionOffset = size - handleSize;

				//Create a handler section reader
				IDwgStreamReader hhandler = DwgStreamReader.GetStreamHandler(m_version, new StreamIO(m_crcStream).Stream);
				hhandler.SetPositionInBits((long)handleSectionOffset);

				throw new NotImplementedException();
			}
			else
			{
				//Create a handler section reader
				m_objectReader = DwgStreamReader.GetStreamHandler(m_version, new StreamIO(m_crcStream, true).Stream);
				m_objectReader.SetPositionInBits(m_crcReader.PositionInBits());

				m_referenceReader = DwgStreamReader.GetStreamHandler(m_version, new StreamIO(m_crcStream, true).Stream);
				m_textReader = DwgStreamReader.GetStreamHandler(m_version, new StreamIO(m_crcStream, true).Stream);
			}

			m_objectInitialPos = m_objectReader.PositionInBits();

			return m_objectReader.ReadObjectType();
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
			var value = m_referenceReader.HandleReference(handle);

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
			if (m_version >= ACadVersion.AC1012 && m_version <= ACadVersion.AC1024)
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
			//R2013+:
			else if (!(m_version >= ACadVersion.AC1018))
			{
				//Has DS binary data B If 1 then this object has associated binary data stored in the data store. 
				//See for more details chapter 24.
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

			if (!(m_version < ACadVersion.AC1015))
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

		private void readExtendedData(DwgEntityTemplate template)
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
		/// <param name="handler"></param>
		private void readReactors(DwgEntityTemplate handler)
		{
			//Numreactors S number of reactors in this object
			int numberOfReactors = m_objectReader.ReadBitLong();

			//Add the reactors to the template
			for (int i = 0; i < numberOfReactors; ++i)
				//[Reactors (soft pointer)]
				handler.CadObject.Reactors.Add(handleReference(), null);

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
				handler.XDictHandle = handleReference();

			if (m_version <= ACadVersion.AC1024)
				return;

			//R2013+:	
			//Has DS binary data B If 1 then this object has associated binary data stored in the data store. See for more details chapter 24.
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
					break;
				case ObjectType.VERTEX_3D:
					break;
				case ObjectType.VERTEX_MESH:
					break;
				case ObjectType.VERTEX_PFACE:
					break;
				case ObjectType.VERTEX_PFACE_FACE:
					break;
				case ObjectType.POLYLINE_2D:
					break;
				case ObjectType.POLYLINE_3D:
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
					break;
				case ObjectType.BLOCK_HEADER:
					break;
				case ObjectType.LAYER_CONTROL_OBJ:
					break;
				case ObjectType.LAYER:
					break;
				case ObjectType.STYLE_CONTROL_OBJ:
					break;
				case ObjectType.STYLE:
					break;
				case ObjectType.UNKNOW_36:
					break;
				case ObjectType.UNKNOW_37:
					break;
				case ObjectType.LTYPE_CONTROL_OBJ:
					break;
				case ObjectType.LTYPE:
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
					break;
			}

			return template;
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

		private DwgTemplate readArc()
		{
			Arc arc = new Arc();
			DwgEntityTemplate template = new DwgEntityTemplate(arc);

			readCommonEntityData(template);

			//Center 3BD 10
			arc.Center = this.m_objectReader.Read3BitDouble();
			//Radius BD 40
			arc.Radius = this.m_objectReader.ReadBitDouble();
			//Thickness BT 39
			arc.Thickness = this.m_objectReader.ReadBitThickness();
			//Extrusion BE 210
			arc.Normal = this.m_objectReader.ReadBitExtrusion();
			//Start angle BD 50
			arc.StartAngle = this.m_objectReader.ReadBitDouble();
			//End angle BD 51
			arc.EndAngle = this.m_objectReader.ReadBitDouble();

			return template;
		}
		private DwgTemplate readCircle()
		{
			Circle circle = new Circle();
			DwgEntityTemplate template = new DwgEntityTemplate(circle);

			readCommonEntityData(template);

			//Center 3BD 10
			circle.Center = this.m_objectReader.Read3BitDouble();
			//Radius BD 40
			circle.Radius = this.m_objectReader.ReadBitDouble();
			//Thickness BT 39
			circle.Thickness = this.m_objectReader.ReadBitThickness();
			//Extrusion BE 210
			circle.Normal = this.m_objectReader.ReadBitExtrusion();

			return template;
		}
		private DwgTemplate readLine()
		{
			Line line = new Line();
			DwgEntityTemplate template = new DwgEntityTemplate(line);

			readCommonEntityData(template);

			//R13-R14 Only:
			if (R13_15Only)
			{
				//Start pt 3BD 10
				line.StartPoint = this.m_objectReader.Read3BitDouble();
				//End pt 3BD 11
				line.EndPoint = this.m_objectReader.Read3BitDouble();
			}

			//R2000+:
			if (R2000Plus)
			{
				//Z’s are zero bit B
				bool flag = this.m_objectReader.ReadBit();
				//Start Point x RD 10
				double startX = this.m_objectReader.ReadDouble();
				//End Point x DD 11 Use 10 value for default
				double endX = this.m_objectReader.ReadBitDoubleWithDefault(startX);
				//Start Point y RD 20
				double startY = this.m_objectReader.ReadDouble();
				//End Point y DD 21 Use 20 value for default
				double endY = this.m_objectReader.ReadBitDoubleWithDefault(startY);

				double startZ = 0.0;
				double endZ = 0.0;

				if (!flag)
				{
					//Start Point z RD 30 Present only if “Z’s are zero bit” is 0
					startZ = this.m_objectReader.ReadDouble();
					//End Point z DD 31 Present only if “Z’s are zero bit” is 0, use 30 value for default.
					endZ = this.m_objectReader.ReadBitDoubleWithDefault(startZ);
				}

				line.StartPoint = new XYZ(startX, startY, startZ);
				line.EndPoint = new XYZ(endX, endY, endZ);
			}

			//Common:
			//Thickness BT 39
			line.Thickness = this.m_objectReader.ReadBitThickness();
			//Extrusion BE 210
			line.Normal = this.m_objectReader.ReadBitExtrusion();

			return template;
		}

		private DwgTemplate readLayer()
		{
			DwgTextEntityTemplate attdef = new DwgTextEntityTemplate(new AttributeDefinition());

			return null;
		}
		#endregion
	}
}
