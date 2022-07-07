using ACadSharp.Blocks;
using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.Types.Units;
using ACadSharp.IO.Templates;
using ACadSharp.Objects;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using CSMath;
using CSUtilities.IO;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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
		private long _objectInitialPos = 0;

		private uint _size;

		/// <summary>
		/// During the object reading the handles will be added at the queue.
		/// </summary>
		private Queue<ulong> _handles;

		private readonly Dictionary<ulong, long> _map;
		private readonly Dictionary<short, DxfClass> _classes;

		private DwgDocumentBuilder _builder;

		private readonly IDwgStreamReader _reader;

		/// <summary>
		/// Needed to handle some items like colors or some text data that may not be present.
		/// </summary>
		private IDwgStreamReader _mergedReaders;

		/// <summary>
		/// Reader to handle the object data.
		/// </summary>
		private IDwgStreamReader _objectReader;

		/// <summary>
		/// Reader focused on the handles section of the stream.
		/// </summary>
		private IDwgStreamReader _handlesReader;

		/// <summary>
		/// Reader focused on the string data section of the stream.
		/// </summary>
		private IDwgStreamReader _textReader;

		/// <summary>
		/// Stream decoded using the crc.
		/// </summary>
		private readonly IDwgStreamReader _crcReader;

		private readonly Stream _crcStream;

		public DwgObjectSectionReader(
			ACadVersion version,
			DwgDocumentBuilder builder,
			IDwgStreamReader reader,
			Queue<ulong> handles,
			Dictionary<ulong, long> handleMap,
			DxfClassCollection classes) : base(version)
		{
			this._builder = builder;

			this._reader = reader;

			this._handles = new Queue<ulong>(handles);
			this._map = new Dictionary<ulong, long>(handleMap);
			this._classes = classes.ToDictionary(x => x.ClassNumber, x => x);

			//Initialize the crc stream
			//RS : CRC for the data section, starting after the sentinel. Use 0xC0C1 for the initial value
			if (this._builder.Flags.HasFlag(DwgReaderFlags.CheckCrc))
				this._crcStream = new CRC8StreamHandler(this._reader.Stream, 0xC0C1);
			else
				this._crcStream = this._reader.Stream;

			//Setup the entity handler
			this._crcReader = DwgStreamReader.GetStreamHandler(this._version, this._crcStream);
		}

		/// <summary>
		/// Read all the entities, tables and objects in the file.
		/// </summary>
		public void Read()
		{
			//Read each handle in the header
			while (this._handles.Any())
			{
				ulong handle = this._handles.Dequeue();

				//Check if the handle has already been read
				if (!this._map.TryGetValue(handle, out long offset) || this._builder.TryGetObjectTemplate(handle, out CadTemplate _))
				{
					continue;
				}

				//Get the object type
				ObjectType type = this.getEntityType(offset);

				//Read the object
				CadTemplate template = this.readObject(type);

				//Add the template to the list to be processed
				if (template != null)
					this._builder.AddTemplate(template);
			}
		}

		private ObjectType getEntityType(long offset)
		{
			ObjectType type = ObjectType.INVALID;

			//Set the position to the entity to find
			this._crcReader.Position = offset;

			//MS : Size of object, not including the CRC
			this._size = (uint)this._crcReader.ReadModularShort();

			if (this._size <= 0U)
				return type;

			//remove the padding bits make sure the object stream ends on a byte boundary
			uint sizeInBits = (uint)(this._size << 3);

			//R2010+:
			if (this.R2010Plus)
			{
				//MC : Size in bits of the handle stream (unsigned, 0x40 is not interpreted as sign).
				//This includes the padding bits at the end of the handle stream
				//(the padding bits make sure the object stream ends on a byte boundary).
				ulong handleSize = this._crcReader.ReadModularChar();

				//Find the handles offset
				ulong handleSectionOffset = (ulong)this._crcReader.PositionInBits() + sizeInBits - handleSize;

				//Create a handler section reader
				this._objectReader = DwgStreamReader.GetStreamHandler(this._version, new StreamIO(this._crcStream, true).Stream);
				this._objectReader.SetPositionInBits(this._crcReader.PositionInBits());

				//set the initial posiltion and get the object type
				this._objectInitialPos = this._objectReader.PositionInBits();
				type = this._objectReader.ReadObjectType();

				//Create a handler section reader
				this._handlesReader = DwgStreamReader.GetStreamHandler(this._version, new StreamIO(this._crcStream, true).Stream);
				this._handlesReader.SetPositionInBits((long)handleSectionOffset);

				//Create a text section reader
				this._textReader = DwgStreamReader.GetStreamHandler(this._version, new StreamIO(this._crcStream, true).Stream);
				this._textReader.SetPositionByFlag((long)handleSectionOffset - 1);

				this._mergedReaders = new DwgMergedReader(this._objectReader, this._textReader, this._handlesReader);
			}
			else
			{
				//Create a handler section reader
				this._objectReader = DwgStreamReader.GetStreamHandler(this._version, new StreamIO(this._crcStream, true).Stream);
				this._objectReader.SetPositionInBits(this._crcReader.PositionInBits());

				this._handlesReader = DwgStreamReader.GetStreamHandler(this._version, new StreamIO(this._crcStream, true).Stream);
				this._textReader = this._objectReader;

				//set the initial posiltion and get the object type
				this._objectInitialPos = this._objectReader.PositionInBits();
				type = this._objectReader.ReadObjectType();
			}

			return type;
		}

		#region Common entity data

		/// <summary>
		/// Get the handle of the entity and saves the value to the <see cref="_handles"/>
		/// </summary>
		/// <returns>the handle to reference into the entity</returns>
		private ulong handleReference()
		{
			return this.handleReference(0);
		}

		/// <summary>
		/// Get the handle of the entity and saves the value to the <see cref="_handles"/>
		/// </summary>
		/// <returns>the handle to reference into the entity</returns>
		private ulong handleReference(ulong handle)
		{
			//Read the handle
			ulong value = this._handlesReader.HandleReference(handle);

			if (!this._builder.TryGetObjectTemplate(value, out CadTemplate _) && !this._handles.Contains(value) && value != 0)
				//Add the value to the handles queue to be processed
				this._handles.Enqueue(value);

			return value;
		}

		/// <summary>
		/// Read the common entity format.
		/// </summary>
		/// <param name="template"></param>
		private void readCommonEntityData(CadEntityTemplate template)
		{
			//Get the cad object as an entity
			Entity entity = template.CadObject;

			if (this._version >= ACadVersion.AC1015 && this._version < ACadVersion.AC1024)
				//Obj size RL size of object in bits, not including end handles
				this.updateHandleReader();

			//Common:
			//Handle H 5 code 0, length followed by the handle bytes.
			ulong handle = this._objectReader.HandleReference();
			entity.Handle = handle;

			//Extended object data, if any
			this.readExtendedData(template);

			//Graphic present Flag B 1 if a graphic is present
			if (this._objectReader.ReadBit())
			{
				//Graphics X if graphicpresentflag is 1, the graphic goes here.
				//See the section on Proxy Entity Graphics for the format of this section.

				//R13 - R007:
				//RL: Size of graphic image in bytes
				//R2010 +:
				//BLL: Size of graphic image in bytes
				long graphicImageSize = this._version >= ACadVersion.AC1024 ?
					this._objectReader.ReadBitLongLong() : this._objectReader.ReadRawLong();

				//Common:
				//X: The graphic image
				//entityHandler.CadObject.JumpGraphicImage(this, entityHandler, graphicImageSize);
				this._mergedReaders.Advance((int)graphicImageSize);
			}

			//R13 - R14 Only:
			if (this._version >= ACadVersion.AC1012 && this._version <= ACadVersion.AC1014)
				this.updateHandleReader();

			//Common:
			//6B : Flags
			//Entmode BB entity mode
			template.EntityMode = this._objectReader.Read2Bits();

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
				template.OwnerHandle = this._handlesReader.HandleReference(entity.Handle);

			//Numreactors BL number of persistent reactors attached to this object
			this.readReactors(template);

			//R13-R14 Only:
			if (this.R13_14Only)
			{
				//8 LAYER (hard pointer)
				template.LayerHandle = this.handleReference();

				//Isbylayerlt B 1 if bylayer linetype, else 0
				if (!this._objectReader.ReadBit())
					//6 [LTYPE (hard pointer)] (present if Isbylayerlt is 0)
					template.LineTypeHandle = this.handleReference();
			}

			//R13-R2000 Only:
			//previous/next handles present if Nolinks is 0.
			//Nolinks B 1 if major links are assumed +1, -1, else 0 For R2004+this always has value 1 (links are not used)
			if (!this.R2004Plus && !this._objectReader.ReadBit())
			{
				//[PREVIOUS ENTITY (relative soft pointer)]
				template.PrevEntity = this.handleReference(entity.Handle);
				//[NEXT ENTITY (relative soft pointer)]
				template.NextEntity = this.handleReference(entity.Handle);
			}
			else if (!this.R2004Plus)
			{
				this._handles.Enqueue(entity.Handle - 1UL);
				this._handles.Enqueue(entity.Handle + 1UL);
			}

			//Color	CMC(B)	62
			entity.Color = this._objectReader.ReadEnColor(out Transparency transparency, out bool colorFlag);
			entity.Transparency = transparency;

			//R2004+:
			if ((this._version >= ACadVersion.AC1018) && colorFlag)
				//[Color book color handle (hard pointer)]
				template.ColorHandle = this.handleReference();

			//Ltype scale	BD	48
			entity.LinetypeScale = this._objectReader.ReadBitDouble();

			if (!(this._version >= ACadVersion.AC1015))
			{
				//Common:
				//Invisibility BS 60
				entity.IsInvisible = (this._objectReader.ReadBitShort() & 1) == 0;

				return;
			}

			//R2000+:
			//8 LAYER (hard pointer)
			template.LayerHandle = this.handleReference();

			//Ltype flags BB 00 = bylayer, 01 = byblock, 10 = continous, 11 = linetype handle present at end of object
			template.LtypeFlags = this._objectReader.Read2Bits();

			if (template.LtypeFlags == 3)
				//6 [LTYPE (hard pointer)] present if linetype flags were 11
				template.LineTypeHandle = this.handleReference();

			//R2007+:
			if (this._version >= ACadVersion.AC1021)
			{
				//Material flags BB 00 = bylayer, 01 = byblock, 11 = material handle present at end of object
				if (this._objectReader.Read2Bits() == 3)
				{
					//MATERIAL present if material flags were 11
					template.MaterialHandle = this.handleReference();
				}

				//Shadow flags RC
				this._objectReader.ReadByte();
			}

			//R2000 +:
			//Plotstyle flags	BB	00 = bylayer, 01 = byblock, 11 = plotstyle handle present at end of object
			if (this._objectReader.Read2Bits() == 3)
			{
				//PLOTSTYLE (hard pointer) present if plotstyle flags were 11
				long plotstyleFlags = (long)this.handleReference();
			}

			//R2007 +:
			if (this._version > ACadVersion.AC1021)
			{
				//Material flags BB 00 = bylayer, 01 = byblock, 11 = material handle present at end of object
				if (this._objectReader.ReadBit())
				{
					//If has full visual style, the full visual style handle (hard pointer).
					long n = (long)this.handleReference();
				}
				if (this._objectReader.ReadBit())
				{
					//If has full visual style, the full visual style handle (hard pointer).
					long n = (long)this.handleReference();
				}
				//Shadow flags RC
				if (this._objectReader.ReadBit())
				{
					//If has full visual style, the full visual style handle (hard pointer).
					long n = (long)this.handleReference();
				}
			}

			//Common:
			//Invisibility BS 60
			entity.IsInvisible = (this._objectReader.ReadBitShort() & 1) == 1;

			//R2000+:
			//Lineweight RC 370
			byte v = this._objectReader.ReadByte();
			//TODO: LineweightType reading, strange values, ex : 29
			entity.Lineweight = LineweightType.Default;
		}

		private void readCommonNonEntityData(CadTemplate template)
		{
			if (this._version >= ACadVersion.AC1015 && this._version < ACadVersion.AC1024)
				//Obj size RL size of object in bits, not including end handles
				this.updateHandleReader();

			//Common:
			//Handle H 5 code 0, length followed by the handle bytes.
			ulong handle = this._objectReader.HandleReference();
			template.CadObject.Handle = handle;

			//Extended object data, if any
			this.readExtendedData(template);

			//R13-R14 Only:
			//Obj size RL size of object in bits, not including end handles
			if (this.R13_14Only)
				this.updateHandleReader();

			//[Owner ref handle (soft pointer)]
			template.OwnerHandle = this.handleReference(template.CadObject.Handle);

			//Read the cad object reactors
			this.readReactors(template);
		}

		private void readXrefDependantBit(TableEntry entry)
		{
			if (this.R2007Plus)
			{
				//xrefindex+1 BS 70 subtract one from this value when read.
				//After that, -1 indicates that this reference did not come from an xref,
				//otherwise this value indicates the index of the blockheader for the xref from which this came.
				short xrefindex = this._objectReader.ReadBitShort();

				//Xdep B 70 dependent on an xref. (16 bit)
				if (((uint)xrefindex & 0b100000000) > 0)
					entry.Flags |= StandardFlags.XrefDependent;
			}
			else
			{
				//64-flag B 70 The 64-bit of the 70 group.
				if (this._objectReader.ReadBit())
					entry.Flags |= StandardFlags.Referenced;

				//xrefindex + 1 BS 70 subtract one from this value when read.
				//After that, -1 indicates that this reference did not come from an xref,
				//otherwise this value indicates the index of the blockheader for the xref from which this came.
				int xrefindex = this._objectReader.ReadBitShort() - 1;

				//Xdep B 70 dependent on an xref. (16 bit)
				if (this._objectReader.ReadBit())
					entry.Flags |= StandardFlags.XrefDependent;
			}
		}

		private void readExtendedData(CadTemplate template)
		{
			//EED directly follows the entity handle.
			//Each application's data is structured as follows:
			//|Length|Application handle|Data items|

			//EED size BS size of extended entity data, if any
			short size = this._objectReader.ReadBitShort();

			while (size != 0)
			{
				//App handle
				ulong appHandle = this._objectReader.HandleReference();
				long endPos = this._objectReader.Position + size;

				//template.ExtendedData
				ExtendedData edata = this.readExtendedDataRecords(endPos);

				template.EDataTemplate.Add(appHandle, edata);

				size = this._objectReader.ReadBitShort();
			}
		}

		private ExtendedData readExtendedDataRecords(long endPos)
		{
			ExtendedData data = new ExtendedData();

			while (this._objectReader.Position < endPos)
			{
				//Each data item has a 1-byte code (DXF group code minus 1000) followed by the value.
				DxfCode dxfCode = (DxfCode)(1000 + this._objectReader.ReadByte());

				ExtendedDataRecord record = null;

				switch (dxfCode)
				{
					//0 (1000) String.
					case DxfCode.ExtendedDataAsciiString:
					//R13-R2004: 1st byte of value is the length N; this is followed by a 2-byte short indicating the codepage, followed by N single-byte characters.
					//R2007 +: 2 - byte length N, followed by N Unicode characters(2 bytes each).
					case DxfCode.ExtendedDataRegAppName:
						//1 (1001) This one seems to be invalid; can't even use as a string inside braces.
						//This would be a registered application that this data relates to, but we've already had that above, 
						//so it would be redundant or irrelevant here.
						record = new ExtendedDataRecord(dxfCode, this._objectReader.ReadTextUnicode());
						break;
					case DxfCode.ExtendedDataControlString:
						//2 (1002) A '{' or '}'; 1 byte; ASCII 0 means '{', ASCII 1 means '}'
						record = new ExtendedDataRecord(dxfCode, this._objectReader.ReadByte());
						break;
					case DxfCode.ExtendedDataLayerName:
						//3 (1003) A layer table reference. The value is the handle of the layer;
						//it's 8 bytes -- even if the leading ones are 0. It's not a string; read 
						//it as hex, as usual for handles. (There's no length specifier this time.) 
						//Even layer 0 is referred to by handle here.
						byte[] arr = this._objectReader.ReadBytes(8);
						ulong handle = System.BitConverter.ToUInt64(arr, 0);
						record = new ExtendedDataRecord(dxfCode, handle);
						break;
					case DxfCode.ExtendedDataBinaryChunk:
						//4 (1004) Binary chunk. The first byte of the value is a char giving the length; the bytes follow.
						record = new ExtendedDataRecord(dxfCode, this._objectReader.ReadBytes(this._objectReader.ReadByte()));
						break;
					case DxfCode.ExtendedDataHandle:
						//5 (1005) An entity handle reference.
						//The value is given as 8 bytes -- even if the leading ones are 0.
						//It's not a string; read it as hex, as usual for handles.
						//(There's no length specifier this time.)
						arr = this._objectReader.ReadBytes(8);
						handle = System.BitConverter.ToUInt64(arr, 0);
						record = new ExtendedDataRecord(dxfCode, handle);
						break;
					//10 - 13 (1010 - 1013)
					case DxfCode.ExtendedDataXCoordinate:
					case DxfCode.ExtendedDataWorldXCoordinate:
					case DxfCode.ExtendedDataWorldYCoordinate:
					case DxfCode.ExtendedDataWorldZCoordinate:
					case DxfCode.ExtendedDataWorldXDisp:
					case DxfCode.ExtendedDataWorldYDisp:
					case DxfCode.ExtendedDataWorldZDisp:
					case DxfCode.ExtendedDataWorldXDir:
					case DxfCode.ExtendedDataWorldYDir:
					case DxfCode.ExtendedDataWorldZDir:
						//Points; 24 bytes(XYZ)-- 3 doubles
						record = new ExtendedDataRecord(
							dxfCode,
							new XYZ(
								this._objectReader.ReadDouble(),
								this._objectReader.ReadDouble(),
								this._objectReader.ReadDouble()
								)
							);
						break;
					//40 - 42 (1040 - 1042)
					case DxfCode.ExtendedDataReal:
					case DxfCode.ExtendedDataDist:
					case DxfCode.ExtendedDataScale:
						//Reals; 8 bytes(double)
						record = new ExtendedDataRecord(dxfCode, this._objectReader.ReadDouble());
						break;
					//70(1070) A short int; 2 bytes
					case DxfCode.ExtendedDataInteger16:
						record = new ExtendedDataRecord(dxfCode, this._objectReader.ReadShort());
						break;
					//71(1071) A long int; 4 bytes
					case DxfCode.ExtendedDataInteger32:
						record = new ExtendedDataRecord(dxfCode, this._objectReader.ReadRawLong());
						break;
					default:
						this._objectReader.ReadBytes((int)(endPos - this._objectReader.Position));
						this._builder.Notify(new NotificationEventArgs($"Unknown code for extended data: {dxfCode}"));
						return data;
				}

				data.Data.Add(record);
			}

			return data;
		}

		// Add the reactors to the template.
		private void readReactors(CadTemplate template)
		{
			//Numreactors S number of reactors in this object
			int numberOfReactors = this._objectReader.ReadBitLong();

			//Add the reactors to the template
			for (int i = 0; i < numberOfReactors; ++i)
				//[Reactors (soft pointer)]
				template.CadObject.Reactors.Add(this.handleReference(), null);

			bool flag = false;
			//R2004+:
			if (this._version >= ACadVersion.AC1018)
				/*XDic Missing Flag
				 * B
				 * If 1, no XDictionary handle is stored for this object,
				 * otherwise XDictionary handle is stored as in R2000 and earlier.
				*/
				flag = this._objectReader.ReadBit();

			if (!flag)
				//xdicobjhandle(hard owner)
				template.XDictHandle = this.handleReference();

			if (this._version <= ACadVersion.AC1024)
				return;

			//R2013+:
			//Has DS binary data B If 1 then this object has associated binary data stored in the data store
			this._objectReader.ReadBit();
		}

		/// <summary>
		/// Update the text reader and the handler reader at the end of the object position.
		/// </summary>
		private void updateHandleReader()
		{
			//RL: Size of object data in bits (number of bits before the handles),
			//or the "endbit" of the pre-handles section.
			long size = this._objectReader.ReadRawLong();

			//Set the position to the handle section
			this._handlesReader.SetPositionInBits(size + this._objectInitialPos);

			if (this._version == ACadVersion.AC1021)
			{
				this._textReader = DwgStreamReader.GetStreamHandler(this._version, new StreamIO(this._crcStream, true).Stream);
				//"endbit" of the pre-handles section.
				this._textReader.SetPositionByFlag(size + this._objectInitialPos - 1);
			}

			this._mergedReaders = new DwgMergedReader(this._objectReader, this._textReader, this._handlesReader);
		}

		#endregion Common entity data

		#region Object readers

		private CadTemplate readObject(ObjectType type)
		{
			CadTemplate template = null;

			switch (type)
			{
				case ObjectType.UNUSED:
					break;
				case ObjectType.TEXT:
					template = this.readText();
					break;
				case ObjectType.ATTRIB:
					template = this.readAttribute();
					break;
				case ObjectType.ATTDEF:
					template = this.readAttributeDefinition();
					break;
				case ObjectType.BLOCK:
					template = this.readBlock();
					break;
				case ObjectType.ENDBLK:
					template = this.readEndBlock();
					break;
				case ObjectType.SEQEND:
					template = this.readSeqend();
					break;
				case ObjectType.INSERT:
					template = this.readInsert();
					break;
				case ObjectType.MINSERT:
					template = this.readMInsert();
					break;
				case ObjectType.UNKNOW_9:
					break;
				case ObjectType.VERTEX_2D:
					template = this.readVertex2D();
					break;
				case ObjectType.VERTEX_3D:
				case ObjectType.VERTEX_MESH:
				case ObjectType.VERTEX_PFACE:
					template = this.readVertex3D();
					break;
				case ObjectType.VERTEX_PFACE_FACE:
					template = this.readPfaceVertex();
					break;
				case ObjectType.POLYLINE_2D:
					template = this.readPolyline2D();
					break;
				case ObjectType.POLYLINE_3D:
					template = this.readPolyline3D();
					break;
				case ObjectType.ARC:
					template = this.readArc();
					break;
				case ObjectType.CIRCLE:
					template = this.readCircle();
					break;
				case ObjectType.LINE:
					template = this.readLine();
					break;
				case ObjectType.DIMENSION_ORDINATE:
					template = this.readDimOrdinate();
					break;
				case ObjectType.DIMENSION_LINEAR:
					template = this.readDimLinear();
					break;
				case ObjectType.DIMENSION_ALIGNED:
					template = this.readDimAligned();
					break;
				case ObjectType.DIMENSION_ANG_3_Pt:
					template = this.readDimAngular3pt();
					break;
				case ObjectType.DIMENSION_ANG_2_Ln:
					template = this.readDimLine2pt();
					break;
				case ObjectType.DIMENSION_RADIUS:
					template = this.readDimRadius();
					break;
				case ObjectType.DIMENSION_DIAMETER:
					template = this.readDimDiameter();
					break;
				case ObjectType.POINT:
					template = this.readPoint();
					break;
				case ObjectType.FACE3D:
					template = this.read3dFace();
					break;
				case ObjectType.POLYLINE_PFACE:
					template = this.readPolylinePface();
					break;
				case ObjectType.POLYLINE_MESH:
					template = this.readPolylineMesh();
					break;
				case ObjectType.SOLID:
				case ObjectType.TRACE:
					template = this.readSolid();
					break;
				case ObjectType.SHAPE:
					template = this.readShape();
					break;
				case ObjectType.VIEWPORT:
					template = this.readViewport();
					break;
				case ObjectType.ELLIPSE:
					template = this.readEllipse();
					break;
				case ObjectType.SPLINE:
					template = this.readSpline();
					break;
				case ObjectType.REGION:
					break;
				case ObjectType.SOLID3D:
					break;
				case ObjectType.BODY:
					break;
				case ObjectType.RAY:
					template = this.readRay();
					break;
				case ObjectType.XLINE:
					template = this.readXLine();
					break;
				case ObjectType.DICTIONARY:
					template = this.readDictionary();
					break;
				case ObjectType.OLEFRAME:
					break;
				case ObjectType.MTEXT:
					template = this.readMText();
					break;
				case ObjectType.LEADER:
					template = this.readLeader();
					break;
				case ObjectType.TOLERANCE:
					break;
				case ObjectType.MLINE:
					template = this.readMLine();
					break;
				case ObjectType.BLOCK_CONTROL_OBJ:
					template = this.readBlockControlObject();
					break;
				case ObjectType.BLOCK_HEADER:
					template = this.readBlockHeader();
					break;
				case ObjectType.LAYER_CONTROL_OBJ:
					template = this.readDocumentTable(new LayersTable());
					break;
				case ObjectType.LAYER:
					template = this.readLayer();
					break;
				case ObjectType.STYLE_CONTROL_OBJ:
					template = this.readDocumentTable(new TextStylesTable());
					break;
				case ObjectType.STYLE:
					template = this.readTextStyle();
					break;
				case ObjectType.UNKNOW_36:
					break;
				case ObjectType.UNKNOW_37:
					break;
				case ObjectType.LTYPE_CONTROL_OBJ:
					template = this.readLTypeControlObject();
					break;
				case ObjectType.LTYPE:
					template = this.readLType();
					break;
				case ObjectType.UNKNOW_3A:
					break;
				case ObjectType.UNKNOW_3B:
					break;
				case ObjectType.VIEW_CONTROL_OBJ:
					template = this.readDocumentTable(new ViewsTable());
					break;
				case ObjectType.VIEW:
					template = this.readView();
					break;
				case ObjectType.UCS_CONTROL_OBJ:
					template = this.readDocumentTable(new UCSTable());
					break;
				case ObjectType.UCS:
					template = this.readUcs();
					break;
				case ObjectType.VPORT_CONTROL_OBJ:
					template = this.readDocumentTable(new VPortsTable());
					break;
				case ObjectType.VPORT:
					template = this.readVPort();
					break;
				case ObjectType.APPID_CONTROL_OBJ:
					template = this.readDocumentTable(new AppIdsTable());
					break;
				case ObjectType.APPID:
					template = this.readAppId();
					break;
				case ObjectType.DIMSTYLE_CONTROL_OBJ:
					template = this.readDocumentTable(new DimensionStylesTable());
					break;
				case ObjectType.DIMSTYLE:
					template = this.readDimStyle();
					break;
				case ObjectType.VP_ENT_HDR_CTRL_OBJ:
					template = this.readViewportEntityControl();
					break;
				case ObjectType.VP_ENT_HDR:
					template = this.readViewportEntityHeader();
					break;
				case ObjectType.GROUP:
					template = this.readGroup();
					break;
				case ObjectType.MLINESTYLE:
					template = this.readMLStyle();
					break;
				case ObjectType.OLE2FRAME:
					break;
				case ObjectType.DUMMY:
					break;
				case ObjectType.LONG_TRANSACTION:
					break;
				case ObjectType.LWPOLYLINE:
					template = this.readLWPolyline();
					break;
				case ObjectType.HATCH:
					template = this.readHatch();
					break;
				case ObjectType.XRECORD:
					try
					{
						template = this.readXRecord();
					}
					catch (System.Exception)
					{
						//Xrecord don't seem stable enough
						this._builder.Notify(new NotificationEventArgs($"Failed to read xrecord"));
					}
					break;
				case ObjectType.ACDBPLACEHOLDER:
					break;
				case ObjectType.VBA_PROJECT:
					break;
				case ObjectType.LAYOUT:
					template = this.readLayout();
					break;
				case ObjectType.ACAD_PROXY_ENTITY:
					break;
				case ObjectType.ACAD_PROXY_OBJECT:
					break;
				default:
					return this.readUnlistedType((short)type);
			}

			if (template == null)
				this._builder.Notify(new NotificationEventArgs($"Object type not implemented: {type}", NotificationType.NotImplemented));

			return template;
		}

		private CadTemplate readUnlistedType(short classNumber)
		{
			if (!this._classes.TryGetValue(classNumber, out DxfClass c))
				return null;

			CadTemplate template = null;

			switch (c.DxfName)
			{
				case "ACDBDICTIONARYWDFLT":
				case "ACDBDETAILVIEWSTYLE":
				case "ACDBSECTIONVIEWSTYLE":
				case "ACAD_TABLE":
				case "CELLSTYLEMAP":
					break;
				case "DBCOLOR":
					template = this.readDwgColor();
					break;
				case "DICTIONARYVAR":
					template = this.readDictionaryVar();
					break;
				case "DICTIONARYWDFLT":
				case "FIELD":
					break;
				case "GROUP":
					template = this.readGroup();
					break;
				case "HATCH":
					template = this.readHatch();
					break;
				case "IDBUFFER":
				case "IMAGE":
				case "IMAGEDEF":
				case "IMAGEDEFREACTOR":
				case "LAYER_INDEX":
					break;
				case "LAYOUT":
					template = this.readLayout();
					break;
				case "LWPLINE":
				case "MATERIAL":
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
					break;
				case "XRECORD":
					template = this.readXRecord();
					break;
				default:
					break;
			}

			if (template == null)
				this._builder.Notify(new NotificationEventArgs($"Unlisted object not implemented, DXF name: {c.DxfName}"));

			return template;
		}

		#region Text entities

		private CadTemplate readText()
		{
			TextEntity text = new TextEntity();
			CadTextEntityTemplate template = new CadTextEntityTemplate(text);

			this.readCommonTextData(template);

			return template;
		}

		private CadTemplate readAttribute()
		{
			AttributeEntity att = new AttributeEntity();
			CadTextEntityTemplate template = new CadTextEntityTemplate(att);

			this.readCommonTextData(template);

			this.readCommonAttData(att);

			return template;
		}

		private CadTemplate readAttributeDefinition()
		{
			AttributeDefinition attdef = new AttributeDefinition();
			CadTextEntityTemplate template = new CadTextEntityTemplate(attdef);

			this.readCommonTextData(template);

			this.readCommonAttData(attdef);

			//R2010+:
			if (this.R2010Plus)
				//Version RC ?		Repeated??
				attdef.Version = this._objectReader.ReadByte();

			//Common:
			//Prompt TV 3
			attdef.Prompt = this._textReader.ReadVariableText();

			return template;
		}

		private void readCommonTextData(CadTextEntityTemplate template)
		{
			this.readCommonEntityData(template);

			TextEntity text = (TextEntity)template.CadObject;

			double elevation = 0.0;
			XY pt = new XY();

			//R13-14 Only:
			if (this._version >= ACadVersion.AC1012 && this._version <= ACadVersion.AC1014)
			{
				//Elevation BD ---
				elevation = this._objectReader.ReadBitDouble();
				//Insertion pt 2RD 10
				pt = this._objectReader.Read2RawDouble();
				text.InsertPoint = new XYZ(pt.X, pt.Y, elevation);

				//Alignment pt 2RD 11
				pt = this._objectReader.Read2RawDouble();
				text.AlignmentPoint = new XYZ(pt.X, pt.Y, elevation);

				//Extrusion 3BD 210
				text.Normal = this._objectReader.Read3BitDouble();
				//Thickness BD 39
				text.Thickness = this._objectReader.ReadBitDouble();
				//Oblique ang BD 51
				text.ObliqueAngle = this._objectReader.ReadBitDouble();
				//Rotation ang BD 50
				text.Rotation = this._objectReader.ReadBitDouble();
				//Height BD 40
				text.Height = this._objectReader.ReadBitDouble();
				//Width factor BD 41
				text.WidthFactor = this._objectReader.ReadBitDouble();
				//Text value TV 1
				text.Value = this._textReader.ReadVariableText();
				//Generation BS 71
				text.Mirror = (TextMirrorFlag)this._objectReader.ReadBitShort();
				//Horiz align. BS 72
				text.HorizontalAlignment = (TextHorizontalAlignment)this._objectReader.ReadBitShort();
				//Vert align. BS 73
				text.VerticalAlignment = (TextVerticalAlignmentType)this._objectReader.ReadBitShort();

				//Common:
				//Common Entity Handle Data H 7 STYLE(hard pointer)
				template.StyleHandle = this.handleReference();
				return;
			}

			//DataFlags RC Used to determine presence of subsquent data
			byte dataFlags = this._objectReader.ReadByte();

			//Elevation RD --- present if !(DataFlags & 0x01)
			if ((dataFlags & 0x1) == 0)
				elevation = this._objectReader.ReadDouble();

			//Insertion pt 2RD 10
			pt = this._objectReader.Read2RawDouble();
			text.InsertPoint = new XYZ(pt.X, pt.Y, elevation);

			//Alignment pt 2DD 11 present if !(DataFlags & 0x02), use 10 & 20 values for 2 default values.
			if ((dataFlags & 0x2) == 0)
			{
				double x = this._objectReader.ReadBitDoubleWithDefault((double)text.InsertPoint.X);
				double y = this._objectReader.ReadBitDoubleWithDefault((double)text.InsertPoint.Y);
				text.AlignmentPoint = new XYZ(x, y, elevation);
			}

			//Extrusion BE 210
			text.Normal = this._objectReader.ReadBitExtrusion();
			//Thickness BT 39
			text.Thickness = this._objectReader.ReadBitThickness();

			//Oblique ang RD 51 present if !(DataFlags & 0x04)
			if ((dataFlags & 0x4) == 0)
				text.ObliqueAngle = this._objectReader.ReadDouble();
			//Rotation ang RD 50 present if !(DataFlags & 0x08)
			if ((dataFlags & 0x8) == 0)
				text.Rotation = this._objectReader.ReadDouble();
			//Height RD 40
			text.Height = this._objectReader.ReadDouble();
			//Width factor RD 41 present if !(DataFlags & 0x10)
			if ((dataFlags & 0x10) == 0)
				text.WidthFactor = this._objectReader.ReadDouble();

			//Text value TV 1
			text.Value = this._textReader.ReadVariableText();

			//Generation BS 71 present if !(DataFlags & 0x20)
			if ((dataFlags & 0x20) == 0)
				text.Mirror = (TextMirrorFlag)this._objectReader.ReadBitShort();
			//Horiz align. BS 72 present if !(DataFlags & 0x40)
			if ((dataFlags & 0x40) == 0)
				text.HorizontalAlignment = (TextHorizontalAlignment)this._objectReader.ReadBitShort();
			//Vert align. BS 73 present if !(DataFlags & 0x80)
			if ((dataFlags & 0x80) == 0)
				text.VerticalAlignment = (TextVerticalAlignmentType)this._objectReader.ReadBitShort();

			//Common:
			//Common Entity Handle Data H 7 STYLE(hard pointer)
			template.StyleHandle = this.handleReference();
		}

		private void readCommonAttData(AttributeBase att)
		{
			//R2010+:
			if (this.R2010Plus)
				//Version RC ?
				att.Version = this._objectReader.ReadByte();

			//R2018+:
			if (this.R2018Plus)
			{
				AttributeType type = (AttributeType)this._objectReader.ReadByte();

				switch (type)
				{
					case AttributeType.SingleLine:
						//Common:
						//Tag TV 2
						att.Tag = this._textReader.ReadVariableText();
						//Field length BS 73 unused
						short length = this._objectReader.ReadBitShort();
						//Flags RC 70 NOT bit-pair - coded.
						att.Flags = (AttributeFlags)this._objectReader.ReadByte();
						//R2007 +:
						if (this.R2007Plus)
							//Lock position flag B 280
							att.IsReallyLocked = this._objectReader.ReadBit();

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

						short dataSize = this._objectReader.ReadBitShort();
						if (dataSize > 0)
						{
							//Annotative data bytes RC Byte array with length Annotative data size.
							this._objectReader.Advance(dataSize);
							//Registered application H Hard pointer.
							this.handleReference();
							//Unknown BS 72? Value 0.
							this._objectReader.ReadBitShort();
						}
						break;
					default:
						break;
				}
			}
		}

		#endregion Text entities

		private CadTemplate readDocumentTable<T>(Table<T> table, CadTableTemplate<T> template = null)
			where T : TableEntry
		{
			if (template == null)
				template = new CadTableTemplate<T>(table);

			this.readCommonNonEntityData(template);

			this._builder.DocumentToBuild.RegisterCollection(template.CadObject);

			//Common:
			//Numentries BL 70
			//Blocks: 	Numentries BL 70 Doesn't count *MODEL_SPACE and *PAPER_SPACE
			//Layers: 	Numentries BL 70 Counts layer "0", too
			int numentries = this._objectReader.ReadBitLong();
			for (int i = 0; i < numentries; ++i)
				//Handle refs H NULL(soft pointer)
				//xdicobjhandle(hard owner)
				//the apps(soft owner)
				template.EntryHandles.Add(this.handleReference());

			return template;
		}

		private CadTemplate readBlock()
		{
			Block block = new Block(new BlockRecord());
			CadEntityTemplate template = new CadEntityTemplate(block);

			this.readCommonEntityData(template);

			//Block name TV 2
			block.Name = this._textReader.ReadVariableText();

			return template;
		}

		private CadTemplate readEndBlock()
		{
			BlockEnd block = new BlockEnd(new BlockRecord());
			CadEntityTemplate template = new CadEntityTemplate(block);

			this.readCommonEntityData(template);

			return template;
		}

		private CadTemplate readSeqend()
		{
			CadEntityTemplate template = new CadEntityTemplate(new Seqend());

			this.readCommonEntityData(template);

			return template;
		}

		#region Insert methods

		private CadTemplate readInsert()
		{
			CadInsertTemplate template = new CadInsertTemplate(new Insert());

			this.readInsertCommonData(template);
			this.readInsertCommonHandles(template);

			return template;
		}

		private CadTemplate readMInsert()
		{
			Insert insert = new Insert();
			CadInsertTemplate template = new CadInsertTemplate(insert);

			this.readInsertCommonData(template);

			//Common:
			//Numcols BS 70
			insert.ColumnCount = (ushort)this._objectReader.ReadBitShort();
			//Numrows BS 71
			insert.RowCount = (ushort)this._objectReader.ReadBitShort();
			//Col spacing BD 44
			insert.ColumnSpacing = this._objectReader.ReadBitDouble();
			//Row spacing BD 45
			insert.RowSpacing = this._objectReader.ReadBitDouble();

			this.readInsertCommonHandles(template);

			return template;
		}

		private void readInsertCommonData(CadInsertTemplate template)
		{
			Insert insert = template.CadObject as Insert;

			this.readCommonEntityData(template);

			//Ins pt 3BD 10
			insert.InsertPoint = this._objectReader.Read3BitDouble();

			//R13-R14 Only:
			if (this.R13_14Only)
			{
				XYZ scale = this._objectReader.Read3BitDouble();
				//X Scale BD 41
				insert.XScale = scale.X;
				//Y Scale BD 42
				insert.YScale = scale.Y;
				//Z Scale BD 43
				insert.ZScale = scale.Z;
			}

			//R2000 + Only:
			if (this.R2000Plus)
			{
				//Data flags BB
				//Scale Data Varies with Data flags:
				switch (this._objectReader.Read2Bits())
				{
					//00 – 41 value stored as a RD, followed by a 42 value stored as DD (use 41 for default value), and a 43 value stored as a DD(use 41 value for default value).
					case 0:
						insert.XScale = this._objectReader.ReadDouble();
						insert.YScale = this._objectReader.ReadBitDoubleWithDefault(insert.XScale);
						insert.ZScale = this._objectReader.ReadBitDoubleWithDefault(insert.XScale);
						break;
					//01 – 41 value is 1.0, 2 DD’s are present, each using 1.0 as the default value, representing the 42 and 43 values.
					case 1:
						insert.YScale = this._objectReader.ReadBitDoubleWithDefault(insert.XScale);
						insert.ZScale = this._objectReader.ReadBitDoubleWithDefault(insert.XScale);
						break;
					//10 – 41 value stored as a RD, and 42 & 43 values are not stored, assumed equal to 41 value.
					case 2:
						double xyz = this._objectReader.ReadDouble();
						insert.XScale = xyz;
						insert.YScale = xyz;
						insert.ZScale = xyz;
						break;
					//11 - scale is (1.0, 1.0, 1.0), no data stored.
					case 3:
						insert.XScale = 1;
						insert.YScale = 1;
						insert.ZScale = 1;
						break;
				}
			}

			//Common:
			//Rotation BD 50
			insert.Rotation = this._objectReader.ReadBitDouble();
			//Extrusion 3BD 210
			insert.Normal = this._objectReader.Read3BitDouble();
			//Has ATTRIBs B 66 Single bit; 1 if ATTRIBs follow.
			template.HasAtts = this._objectReader.ReadBit();
			template.OwnedObjectsCount = 0;

			//R2004+:
			if (this.R2004Plus & template.HasAtts)
				//Owned Object Count BL Number of objects owned by this object.
				template.OwnedObjectsCount = this._objectReader.ReadBitLong();
		}

		private void readInsertCommonHandles(CadInsertTemplate template)
		{
			//Common:
			//Common Entity Handle Data
			//H 2 BLOCK HEADER(hard pointer)
			template.BlockHeaderHandle = this.handleReference();

			if (!template.HasAtts)
				return;

			//R13 - R2000:
			if (this._version >= ACadVersion.AC1012 && this._version <= ACadVersion.AC1015)
			{
				//H[1st ATTRIB(soft pointer)] if 66 bit set; can be NULL
				template.FirstAttributeHandle = this.handleReference();
				//H[last ATTRIB](soft pointer)] if 66 bit set; can be NULL
				template.EndAttributeHandle = this.handleReference();
			}
			//R2004+:
			else if (this.R2004Plus)
			{
				for (int i = 0; i < template.OwnedObjectsCount; ++i)
					//H[ATTRIB(hard owner)] Repeats “Owned Object Count” times.
					template.OwnedHandles.Add(this.handleReference());
			}

			//Common:
			//H[SEQEND(hard owner)] if 66 bit set
			template.SeqendHandle = this.handleReference();
		}

		#endregion Insert methods

		private CadTemplate readVertex2D()
		{
			Vertex2D vertex = new Vertex2D();
			CadEntityTemplate template = new CadEntityTemplate(vertex);

			this.readCommonEntityData(template);

			//Flags EC 70 NOT bit-pair-coded.
			vertex.Flags = (VertexFlags)this._objectReader.ReadByte();
			//Point 3BD 10 NOTE THAT THE Z SEEMS TO ALWAYS BE 0.0! The Z must be taken from the 2D POLYLINE elevation.
			vertex.Location = this._objectReader.Read3BitDouble();

			//Start width BD 40 If it's negative, use the abs val for start AND end widths (and note that no end width will be present).
			//This is a compression trick for cases where the start and end widths are identical and non-0.
			double width = this._objectReader.ReadBitDouble();
			if (width < 0.0)
			{
				vertex.StartWidth = -width;
				vertex.EndWidth = -width;
			}
			else
			{
				vertex.StartWidth = width;
				//End width BD 41 Not present if the start width is < 0.0; see above.
				vertex.EndWidth = this._objectReader.ReadBitDouble();
			}

			//Bulge BD 42
			vertex.Bulge = this._objectReader.ReadBitDouble();

			//R2010+:
			if (this.R2010Plus)
				//Vertex ID BL 91
				vertex.Id = this._objectReader.ReadBitLong();

			//Common:
			//Tangent dir BD 50
			vertex.CurveTangent = this._objectReader.ReadBitDouble();

			return template;
		}

		private CadTemplate readVertex3D()
		{
			Vertex3D vertex = new Vertex3D();
			CadEntityTemplate template = new CadEntityTemplate(vertex);

			this.readCommonEntityData(template);

			//Flags EC 70 NOT bit-pair-coded.
			vertex.Flags = (VertexFlags)this._objectReader.ReadByte();
			//Point 3BD 10
			vertex.Location = this._objectReader.Read3BitDouble();

			return template;
		}

		private CadTemplate readPfaceVertex()
		{
			Vertex2D vertex = new Vertex2D();
			CadEntityTemplate template = new CadEntityTemplate(vertex);

			this.readCommonEntityData(template);

			//Vert index BS 71 1 - based vertex index(see DXF doc)
			this._objectReader.ReadBitShort();
			//Vert index BS 72 1 - based vertex index(see DXF doc)
			this._objectReader.ReadBitShort();
			//Vert index BS 73 1 - based vertex index(see DXF doc)
			this._objectReader.ReadBitShort();
			//Vert index BS 74 1 - based vertex index(see DXF doc)
			this._objectReader.ReadBitShort();

			return template;
		}

		private CadTemplate readPolyline2D()
		{
			Polyline2D pline = new Polyline2D();
			CadPolyLineTemplate template = new CadPolyLineTemplate(pline);

			this.readCommonEntityData(template);

			//Flags BS 70
			pline.Flags = (PolylineFlags)this._objectReader.ReadBitShort();
			//Curve type BS 75 Curve and smooth surface type.
			pline.SmoothSurface = (SmoothSurfaceType)this._objectReader.ReadBitShort();
			//Start width BD 40 Default start width
			pline.StartWidth = this._objectReader.ReadBitDouble();
			//End width BD 41 Default end width
			pline.EndWidth = this._objectReader.ReadBitDouble();
			//Thickness BT 39
			pline.Thickness = this._objectReader.ReadBitThickness();
			//Elevation BD 10 The 10-pt is (0,0,elev)
			pline.Elevation = this._objectReader.ReadBitDouble();
			//Extrusion BE 210
			pline.Normal = this._objectReader.ReadBitExtrusion();

			//R2004+:
			if (this.R2004Plus)
			{
				//Owned Object Count BL Number of objects owned by this object.
				int nownedObjects = this._objectReader.ReadBitLong();

				for (int i = 0; i < nownedObjects; ++i)
					template.VertexHandles.Add(this.handleReference());
			}

			//R13-R2000:
			if (this._version >= ACadVersion.AC1012 && this._version <= ACadVersion.AC1015)
			{
				//H first VERTEX (soft pointer)
				template.FirstVertexHandle = this.handleReference();
				//H last VERTEX (soft pointer)
				template.LastVertexHandle = this.handleReference();
			}

			//Common:
			//H SEQEND(hard owner)
			template.SeqendHandle = this.handleReference();

			return template;
		}

		private CadTemplate readPolyline3D()
		{
			Polyline3D pline = new Polyline3D();
			CadPolyLineTemplate template = new CadPolyLineTemplate(pline);

			this.readCommonEntityData(template);

			//Flags RC 70 NOT DIRECTLY THE 75. Bit-coded (76543210):
			byte num1 = this._objectReader.ReadByte();
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
			bool closed = (this._objectReader.ReadByte() & 1U) > 0U;

			//R2004+:
			if (this.R2004Plus)
			{
				//Owned Object Count BL Number of objects owned by this object.
				int nownedObjects = this._objectReader.ReadBitLong();

				for (int i = 0; i < nownedObjects; ++i)
					template.VertexHandles.Add(this.handleReference());
			}

			//R13-R2000:
			if (this._version >= ACadVersion.AC1012 && this._version <= ACadVersion.AC1015)
			{
				//H first VERTEX (soft pointer)
				template.FirstVertexHandle = this.handleReference();
				//H last VERTEX (soft pointer)
				template.LastVertexHandle = this.handleReference();
			}

			//Common:
			//H SEQEND(hard owner)
			template.SeqendHandle = this.handleReference();

			return template;
		}

		private CadTemplate readArc()
		{
			Arc arc = new Arc();
			CadEntityTemplate template = new CadEntityTemplate(arc);

			this.readCommonEntityData(template);

			//Center 3BD 10
			arc.Center = this._objectReader.Read3BitDouble();
			//Radius BD 40
			arc.Radius = this._objectReader.ReadBitDouble();
			//Thickness BT 39
			arc.Thickness = this._objectReader.ReadBitThickness();
			//Extrusion BE 210
			arc.Normal = this._objectReader.ReadBitExtrusion();
			//Start angle BD 50
			arc.StartAngle = this._objectReader.ReadBitDouble();
			//End angle BD 51
			arc.EndAngle = this._objectReader.ReadBitDouble();

			return template;
		}

		private CadTemplate readCircle()
		{
			Circle circle = new Circle();
			CadEntityTemplate template = new CadEntityTemplate(circle);

			this.readCommonEntityData(template);

			//Center 3BD 10
			circle.Center = this._objectReader.Read3BitDouble();
			//Radius BD 40
			circle.Radius = this._objectReader.ReadBitDouble();
			//Thickness BT 39
			circle.Thickness = this._objectReader.ReadBitThickness();
			//Extrusion BE 210
			circle.Normal = this._objectReader.ReadBitExtrusion();

			return template;
		}

		private CadTemplate readLine()
		{
			Line line = new Line();
			CadEntityTemplate template = new CadEntityTemplate(line);

			this.readCommonEntityData(template);

			//R13-R14 Only:
			if (this.R13_14Only)
			{
				//Start pt 3BD 10
				line.StartPoint = this._objectReader.Read3BitDouble();
				//End pt 3BD 11
				line.EndPoint = this._objectReader.Read3BitDouble();
			}

			//R2000+:
			if (this.R2000Plus)
			{
				//Z’s are zero bit B
				bool flag = this._objectReader.ReadBit();
				//Start Point x RD 10
				double startX = this._objectReader.ReadDouble();
				//End Point x DD 11 Use 10 value for default
				double endX = this._objectReader.ReadBitDoubleWithDefault(startX);
				//Start Point y RD 20
				double startY = this._objectReader.ReadDouble();
				//End Point y DD 21 Use 20 value for default
				double endY = this._objectReader.ReadBitDoubleWithDefault(startY);

				double startZ = 0.0;
				double endZ = 0.0;

				if (!flag)
				{
					//Start Point z RD 30 Present only if “Z’s are zero bit” is 0
					startZ = this._objectReader.ReadDouble();
					//End Point z DD 31 Present only if “Z’s are zero bit” is 0, use 30 value for default.
					endZ = this._objectReader.ReadBitDoubleWithDefault(startZ);
				}

				line.StartPoint = new XYZ(startX, startY, startZ);
				line.EndPoint = new XYZ(endX, endY, endZ);
			}

			//Common:
			//Thickness BT 39
			line.Thickness = this._objectReader.ReadBitThickness();
			//Extrusion BE 210
			line.Normal = this._objectReader.ReadBitExtrusion();

			return template;
		}

		#region Dimensions

		private CadTemplate readDimOrdinate()
		{
			DimensionOrdinate dimension = new DimensionOrdinate();
			CadDimensionTemplate template = new CadDimensionTemplate(dimension);

			this.readCommonDimensionData(template);

			//Common:
			//10 - pt 3BD 10 See DXF documentation.
			dimension.DefinitionPoint = this._objectReader.Read3BitDouble();
			//13 - pt 3BD 13 See DXF documentation.
			dimension.FeatureLocation = this._objectReader.Read3BitDouble();
			//14 - pt 3BD 14 See DXF documentation.
			dimension.LeaderEndpoint = this._objectReader.Read3BitDouble();

			this.readCommonDimensionHandles(template);

			return template;
		}

		private CadTemplate readDimLinear()
		{
			DimensionLinear dimension = new DimensionLinear();
			CadDimensionTemplate template = new CadDimensionTemplate(dimension);

			this.readCommonDimensionData(template);

			this.readCommonDimensionAlignedData(template);

			//Dim rot BD 50 Linear dimension rotation; see DXF documentation.
			dimension.Rotation = this._objectReader.ReadBitDouble();

			this.readCommonDimensionHandles(template);

			return template;
		}

		private CadTemplate readDimAligned()
		{
			DimensionLinear dimension = new DimensionLinear();
			CadDimensionTemplate template = new CadDimensionTemplate(dimension);

			this.readCommonDimensionData(template);

			this.readCommonDimensionAlignedData(template);

			this.readCommonDimensionHandles(template);

			return template;
		}

		private CadTemplate readDimAngular3pt()
		{
			DimensionAngular3Pt dimension = new DimensionAngular3Pt();
			CadDimensionTemplate template = new CadDimensionTemplate(dimension);

			this.readCommonDimensionData(template);

			//Common:
			//10 - pt 3BD 10 See DXF documentation.
			dimension.DefinitionPoint = this._objectReader.Read3BitDouble();
			//13 - pt 3BD 13 See DXF documentation.
			dimension.FirstPoint = this._objectReader.Read3BitDouble();
			//14 - pt 3BD 14 See DXF documentation.
			dimension.SecondPoint = this._objectReader.Read3BitDouble();
			//15-pt 3BD 15 See DXF documentation.
			dimension.AngleVertex = this._objectReader.Read3BitDouble();

			this.readCommonDimensionHandles(template);

			return template;
		}

		private CadTemplate readDimLine2pt()
		{
			DimensionAngular2Line dimension = new DimensionAngular2Line();
			CadDimensionTemplate template = new CadDimensionTemplate(dimension);

			this.readCommonDimensionData(template);

			//Common:
			//16-pt 2RD 16 See DXF documentation.
			XY xy = this._objectReader.Read2RawDouble();
			dimension.DimensionArc = new XYZ(xy.X, xy.Y, dimension.TextMiddlePoint.Z);

			//13 - pt 3BD 13 See DXF documentation.
			dimension.FirstPoint = this._objectReader.Read3BitDouble();
			//14 - pt 3BD 14 See DXF documentation.
			dimension.SecondPoint = this._objectReader.Read3BitDouble();
			//15-pt 3BD 15 See DXF documentation.
			dimension.AngleVertex = this._objectReader.Read3BitDouble();
			//10 - pt 3BD 10 See DXF documentation.
			dimension.DefinitionPoint = this._objectReader.Read3BitDouble();

			this.readCommonDimensionHandles(template);

			return template;
		}

		private CadTemplate readDimRadius()
		{
			DimensionRadius dimension = new DimensionRadius();
			CadDimensionTemplate template = new CadDimensionTemplate(dimension);

			this.readCommonDimensionData(template);

			//Common:
			//10 - pt 3BD 10 See DXF documentation.
			dimension.DefinitionPoint = this._objectReader.Read3BitDouble();
			//15-pt 3BD 15 See DXF documentation.
			dimension.AngleVertex = this._objectReader.Read3BitDouble();
			//Leader len D 40 Leader length.
			dimension.LeaderLength = this._objectReader.ReadBitDouble();

			this.readCommonDimensionHandles(template);

			return template;
		}

		private CadTemplate readDimDiameter()
		{
			DimensionDiameter dimension = new DimensionDiameter();
			CadDimensionTemplate template = new CadDimensionTemplate(dimension);

			this.readCommonDimensionData(template);

			//Common:
			//10 - pt 3BD 10 See DXF documentation.
			dimension.DefinitionPoint = this._objectReader.Read3BitDouble();
			//15-pt 3BD 15 See DXF documentation.
			dimension.AngleVertex = this._objectReader.Read3BitDouble();
			//Leader len D 40 Leader length.
			dimension.LeaderLength = this._objectReader.ReadBitDouble();

			this.readCommonDimensionHandles(template);

			return template;
		}

		private void readCommonDimensionData(CadDimensionTemplate template)
		{
			this.readCommonEntityData(template);

			Dimension dimension = template.CadObject as Dimension;

			//R2010:
			if (this.R2010Plus)
				//Version RC 280 0 = R2010
				dimension.Version = this._objectReader.ReadByte();

			//Common:
			//Extrusion 3BD 210
			dimension.Normal = this._objectReader.Read3BitDouble();
			//Text midpt 2RD 11 See DXF documentation.
			XY midpt = this._objectReader.Read2RawDouble();
			//Elevation BD 11 Z - coord for the ECS points(11, 12, 16).
			//12 (The 16 remains (0,0,0) in entgets of this entity,
			//since the 16 is not used in this type of dimension
			//and is not present in the binary form here.)
			double elevation = this._objectReader.ReadBitDouble();
			dimension.TextMiddlePoint = new XYZ(midpt.X, midpt.Y, elevation);

			//Flags 1 RC 70 Non - bit - pair - coded.
			//NOT the 70 group, but helps define it.
			//Apparently only the two lowest bit are used:
			//76543210:
			//Bit 0 : The OPPOSITE of bit 7(128) of 70.
			//Bit 1 : Same as bit 5(32) of the 70(but 32 is not doc'd by ACAD).
			//The actual 70 - group value comes from 3 things:
			//6 for being an ordinate DIMENSION, plus whatever bits "Flags 1" and "Flags 2" specify.
			byte dimensionType = this._objectReader.ReadByte();//TODO: set dimension type

			// if (!this.ModelBuilder.IsDxf21Superior)

			//User text TV 1
			dimension.Text = this._textReader.ReadVariableText();

			//Text rot BD 53 See DXF documentation.
			dimension.TextRotation = this._objectReader.ReadBitDouble();
			//Horiz dir BD 51 See DXF documentation.
			dimension.HorizontalDirection = this._objectReader.ReadBitDouble();

			//TODO: readDimension insert scale and rotation not implemented

			//Ins X - scale BD 41 Undoc'd. These apply to the insertion of the
			//Ins Y - scale BD 42 anonymous block. None of them can be
			//Ins Z - scale BD 43 dealt with via entget/entmake/entmod.
			var insertionScaleFactor = new XYZ(this._objectReader.ReadBitDouble(), this._objectReader.ReadBitDouble(), this._objectReader.ReadBitDouble());

			//Ins rotation BD 54 The last 2(43 and 54) are reported by DXFOUT(when not default values).
			//ALL OF THEM can be set via DXFIN, however.
			var insertionRotation = this._objectReader.ReadBitDouble();

			//R2000 +:
			if (this.R2000Plus)
			{
				//Attachment Point BS 71
				dimension.AttachmentPoint = (AttachmentPointType)this._objectReader.ReadBitShort();
				//Linespacing Style BS 72
				dimension.LineSpacingStyle = (LineSpacingStyleType)this._objectReader.ReadBitShort();
				//Linespacing Factor BD 41
				dimension.LineSpacingFactor = this._objectReader.ReadBitDouble();
				//Actual Measurement BD 42
				dimension.Measurement = this._objectReader.ReadBitDouble();
			}

			//R2007 +:
			if (this.R2007Plus)
			{
				//Unknown B 73
				this._objectReader.ReadBit();
				//Flip arrow1 B 74
				this._objectReader.ReadBit();
				//Flip arrow2 B 75
				this._objectReader.ReadBit();
			}

			//Common:
			//12 - pt 2RD 12 See DXF documentation.
			XY pt = this._objectReader.Read2RawDouble();
			dimension.InsertionPoint = new XYZ((double)pt.X, (double)pt.Y, elevation);
		}

		private void readCommonDimensionAlignedData(CadDimensionTemplate template)
		{
			DimensionAligned dimension = (DimensionAligned)template.CadObject;

			//Common:
			//13 - pt 3BD 13 See DXF documentation.
			dimension.FirstPoint = this._objectReader.Read3BitDouble();
			//14 - pt 3BD 14 See DXF documentation.
			dimension.SecondPoint = this._objectReader.Read3BitDouble();
			//10 - pt 3BD 10 See DXF documentation.
			dimension.DefinitionPoint = this._objectReader.Read3BitDouble();

			//Ext ln rot BD 52 Extension line rotation; see DXF documentation.
			dimension.ExtLineRotation = this._objectReader.ReadBitDouble();
		}

		private void readCommonDimensionHandles(CadDimensionTemplate template)
		{
			//Common Entity Handle Data
			//H 3 DIMSTYLE(hard pointer)
			template.StyleHandle = this.handleReference();
			//H 2 anonymous BLOCK(hard pointer)
			template.BlockHandle = this.handleReference();
		}

		#endregion

		private CadTemplate readPoint()
		{
			Point pt = new Point();
			CadEntityTemplate template = new CadEntityTemplate(pt);

			this.readCommonEntityData(template);

			//Point 3BD 10
			pt.Location = this._objectReader.Read3BitDouble();
			//Thickness BT 39
			pt.Thickness = this._objectReader.ReadBitThickness();
			//Extrusion BE 210
			pt.Normal = this._objectReader.ReadBitExtrusion();
			//X - axis ang BD 50 See DXF documentation
			pt.Rotation = this._objectReader.ReadBitDouble();

			return template;
		}

		private CadTemplate read3dFace()
		{
			Face3D face = new Face3D();
			CadEntityTemplate template = new CadEntityTemplate(face);

			this.readCommonEntityData(template);

			//R13 - R14 Only:
			if (this.R13_14Only)
			{
				//1st corner 3BD 10
				face.FirstCorner = this._objectReader.Read3BitDouble();
				//2nd corner 3BD 11
				face.SecondCorner = this._objectReader.Read3BitDouble();
				//3rd corner 3BD 12
				face.ThirdCorner = this._objectReader.Read3BitDouble();
				//4th corner 3BD 13
				face.FourthCorner = this._objectReader.Read3BitDouble();
				//Invis flags BS 70 Invisible edge flags
				face.Flags = (InvisibleEdgeFlags)this._objectReader.ReadBitShort();
			}

			//R2000 +:
			if (this.R2000Plus)
			{
				//Has no flag ind. B
				bool noFlags = this._objectReader.ReadBit();
				//Z is zero bit B
				bool zIsZero = this._objectReader.ReadBit();

				//1st corner x RD 10
				double x = this._objectReader.ReadDouble();
				//1st corner y RD 20
				double y = this._objectReader.ReadDouble();
				//1st corner z RD 30 Present only if “Z is zero bit” is 0.
				double z = 0.0;

				if (!zIsZero)
					z = this._objectReader.ReadDouble();

				face.FirstCorner = new XYZ(x, y, z);

				//2nd corner 3DD 11 Use 10 value as default point
				face.SecondCorner = this._objectReader.Read3BitDoubleWithDefault(face.FirstCorner);
				//3rd corner 3DD 12 Use 11 value as default point
				face.ThirdCorner = this._objectReader.Read3BitDoubleWithDefault(face.SecondCorner);
				//4th corner 3DD 13 Use 12 value as default point
				face.FourthCorner = this._objectReader.Read3BitDoubleWithDefault(face.ThirdCorner);

				//Invis flags BS 70 Present it “Has no flag ind.” is 0.
				if (!noFlags)
					face.Flags = (InvisibleEdgeFlags)this._objectReader.ReadBitShort();
			}

			return template;
		}

		private CadTemplate readPolylinePface()
		{
			return null;
		}

		private CadTemplate readPolylineMesh()
		{
			return null;
		}

		private CadTemplate readSolid()
		{
			Solid solid = new Solid();
			CadEntityTemplate template = new CadEntityTemplate(solid);

			//Common Entity Data
			this.readCommonEntityData(template);

			//Thickness BT 39
			solid.Thickness = this._objectReader.ReadBitThickness();

			//Elevation BD ---Z for 10 - 13.
			double elevation = this._objectReader.ReadBitDouble();

			//1st corner 2RD 10
			XY firstCorner = this._objectReader.Read2RawDouble();
			solid.FirstCorner = new XYZ(firstCorner.X, firstCorner.Y, elevation);

			//2nd corner 2RD 11
			XY point2D2 = this._objectReader.Read2RawDouble();
			solid.SecondCorner = new XYZ(point2D2.X, point2D2.Y, elevation);

			//3rd corner 2RD 12
			XY point2D3 = this._objectReader.Read2RawDouble();
			solid.ThirdCorner = new XYZ(point2D3.X, point2D3.Y, elevation);

			//4th corner 2RD 13
			XY point2D4 = this._objectReader.Read2RawDouble();
			solid.FourthCorner = new XYZ(point2D4.X, point2D4.Y, elevation);

			//Extrusion BE 210
			solid.Normal = this._objectReader.ReadBitExtrusion();

			return template;
		}

		private CadTemplate readShape()
		{
			Shape shape = new Shape();
			CadShapeTemplate template = new CadShapeTemplate(shape);

			this.readCommonEntityData(template);

			//Ins pt 3BD 10
			shape.InsertionPoint = this._objectReader.Read3BitDouble();
			//Scale BD 40 Scale factor, default value 1.
			shape.Size = this._objectReader.ReadBitDouble();
			//Rotation BD 50 Rotation in radians, default value 0.
			shape.Rotation = this._objectReader.ReadBitDouble();
			//Width factor BD 41 Width factor, default value 1.
			shape.RelativeXScale = this._objectReader.ReadBitDouble();
			//Oblique BD 51 Oblique angle in radians, default value 0.
			shape.ObliqueAngle = this._objectReader.ReadBitDouble();
			//Thickness BD 39
			shape.Thickness = this._objectReader.ReadBitDouble();

			//Shapeno BS 2
			//This is the shape index.
			//In DXF the shape name is stored.
			//When reading from DXF, the shape is found by iterating over all the text styles
			//(SHAPEFILE, see paragraph 20.4.56) and when the text style contains a shape file,
			//iterating over all the shapes until the one with the matching name is found.
			template.ShapeIndex = (ushort)this._objectReader.ReadBitShort();

			//Extrusion 3BD 210
			shape.Extrusion = this._objectReader.Read3BitDouble();

			//H SHAPEFILE (hard pointer)
			template.ShapeFileHandle = this.handleReference();

			return template;
		}

		private CadTemplate readViewport()
		{
			Viewport viewport = new Viewport();
			CadViewportTemplate template = new CadViewportTemplate(viewport);

			//Common Entity Data
			this.readCommonEntityData(template);

			//Center 3BD 10
			viewport.Center = this._objectReader.Read3BitDouble();
			//Width BD 40
			viewport.Width = this._objectReader.ReadBitDouble();
			//Height BD 41
			viewport.Height = this._objectReader.ReadBitDouble();

			//R2000 +:
			if (this.R2000Plus)
			{
				//View Target 3BD 17
				viewport.ViewTarget = this._objectReader.Read3BitDouble();
				//View Direction 3BD 16
				viewport.ViewDirection = this._objectReader.Read3BitDouble();
				//View Twist Angle BD 51
				viewport.TwistAngle = this._objectReader.ReadBitDouble();
				//View Height BD 45
				viewport.ViewHeight = this._objectReader.ReadBitDouble();
				//Lens Length BD 42
				viewport.LensLength = this._objectReader.ReadBitDouble();
				//Front Clip Z BD 43
				viewport.FrontClipPlane = this._objectReader.ReadBitDouble();
				//Back Clip Z BD 44
				viewport.BackClipPlane = this._objectReader.ReadBitDouble();
				//Snap Angle BD 50
				viewport.SnapAngle = this._objectReader.ReadBitDouble();
				//View Center 2RD 12
				viewport.ViewCenter = this._objectReader.Read2RawDouble();
				//Snap Base 2RD 13
				viewport.SnapBase = this._objectReader.Read2RawDouble();
				//Snap Spacing 2RD 14
				viewport.SnapSpacing = this._objectReader.Read2RawDouble();
				//Grid Spacing 2RD 15
				viewport.GridSpacing = this._objectReader.Read2RawDouble();
				//Circle Zoom BS 72
				viewport.CircleZoomPercent = this._objectReader.ReadBitShort();
			}

			//R2007 +:
			if (this.R2007Plus)
				//Grid Major BS 61
				viewport.MajorGridLineFrequency = this._objectReader.ReadBitShort();

			int frozenLayerCount = 0;
			//R2000 +:
			if (this.R2000Plus)
			{
				//Frozen Layer Count BL
				frozenLayerCount = this._objectReader.ReadBitLong();
				//Status Flags BL 90
				viewport.Status = (ViewportStatusFlags)this._objectReader.ReadBitLong();
				//Style Sheet TV 1
				viewport.StyleSheetName = this._textReader.ReadVariableText();
				//Render Mode RC 281
				viewport.RenderMode = (RenderMode)this._objectReader.ReadByte();
				//UCS at origin B 74
				viewport.DisplayUcsIcon = this._objectReader.ReadBit();
				//UCS per Viewport B 71
				viewport.UcsPerViewport = this._objectReader.ReadBit();
				//UCS Origin 3BD 110
				viewport.UcsOrigin = this._objectReader.Read3BitDouble();
				//UCS X Axis 3BD 111
				viewport.UcsXAxis = this._objectReader.Read3BitDouble();
				//UCS Y Axis 3BD 112
				viewport.UcsYAxis = this._objectReader.Read3BitDouble();
				//UCS Elevation BD 146
				viewport.Elevation = this._objectReader.ReadBitDouble();
				//UCS Ortho View Type BS 79
				viewport.UcsOrthographicType = (OrthographicType)this._objectReader.ReadBitShort();
			}

			//R2004 +:
			if (this.R2004Plus)
				//ShadePlot Mode BS 170
				viewport.ShadePlotMode = (ShadePlotMode)this._objectReader.ReadBitShort();

			//R2007 +:
			if (this.R2007Plus)
			{
				//Use def. lights B 292
				viewport.UseDefaultLighting = this._objectReader.ReadBit();
				//Def.lighting type RC 282
				viewport.DefaultLightingType = (LightingType)this._objectReader.ReadByte();
				//Brightness BD 141
				viewport.Brightness = this._objectReader.ReadBitDouble();
				//Contrast BD 142
				viewport.Constrast = this._objectReader.ReadBitDouble();
				//Ambient light color CMC 63
				viewport.AmbientLightColor = this._objectReader.ReadCmColor();
			}

			//R13 - R14 Only:
			if (this.R13_14Only)
				//H VIEWPORT ENT HEADER(hard pointer)
				template.ViewportHeaderHandle = this.handleReference();

			//R2000 +:
			if (this.R2000Plus)
			{
				for (int i = 0; i < frozenLayerCount; ++i)
					//H 341 Frozen Layer Handles(use count from above)(hard pointer until R2000, soft pointer from R2004 onwards)
					template.FrozenLayerHandles.Add(this.handleReference());

				//H 340 Clip boundary handle(soft pointer)
				template.BoundaryHandle = this.handleReference();
			}

			//R2000:
			if (this._version == ACadVersion.AC1015)
				//H VIEWPORT ENT HEADER((hard pointer))
				template.ViewportHeaderHandle = this.handleReference();

			//R2000 +:
			if (this.R2000Plus)
			{
				//H 345 Named UCS Handle(hard pointer)
				template.NamedUcsHandle = this.handleReference();
				//H 346 Base UCS Handle(hard pointer)
				template.BaseUcsHandle = this.handleReference();
			}

			//R2007 +:
			if (this.R2007Plus)
			{
				//H 332 Background(soft pointer)
				long backgroundHandle = (long)this.handleReference();
				//H 348 Visual Style(hard pointer)
				long visualStyleHandle = (long)this.handleReference();
				//H 333 Shadeplot ID(soft pointer)
				long shadePlotIdHandle = (long)this.handleReference();
				//H 361 Sun(hard owner)
				long sunHandle = (long)this.handleReference();
			}

			return template;
		}

		private CadTemplate readEllipse()
		{
			Ellipse ellipse = new Ellipse();
			CadEntityTemplate template = new CadEntityTemplate(ellipse);

			this.readCommonEntityData(template);

			//Center 3BD 10 (WCS)
			ellipse.Center = this._objectReader.Read3BitDouble();
			//SM axis vec 3BD 11 Semi-major axis vector (WCS)
			ellipse.EndPoint = this._objectReader.Read3BitDouble();
			//Extrusion 3BD 210
			ellipse.Normal = this._objectReader.Read3BitDouble();
			//Axis ratio BD 40 Minor/major axis ratio
			ellipse.RadiusRatio = this._objectReader.ReadBitDouble();
			//Beg angle BD 41 Starting angle (eccentric anomaly, radians)
			ellipse.StartParameter = this._objectReader.ReadBitDouble();
			//End angle BD 42 Ending angle (eccentric anomaly, radians)
			ellipse.EndParameter = this._objectReader.ReadBitDouble();

			return template;
		}

		private CadTemplate readSpline()
		{
			Spline spline = new Spline();
			CadEntityTemplate template = new CadEntityTemplate(spline);

			this.readCommonEntityData(template);

			//Scenario BL a flag which is 2 for fitpts only, 1 for ctrlpts/knots.
			//In 2013 the meaning is somehwat more sophisticated, see knot parameter below.
			int scenario = this._objectReader.ReadBitLong();

			//R2013+:
			if (this.R2013Plus)
			{
				//Spline flags 1 BL Spline flags 1:
				//method fit points = 1,
				//CV frame show = 2,
				//Is closed = 4. 
				//At this point the regular spline flags closed bit is made equal to this bit.
				//Value is overwritten below in scenario 2 though, 
				//Use knot parameter = 8
				spline.Flags1 = (SplineFlags1)this._mergedReaders.ReadBitLong();

				//Knot parameter BL Knot parameter:
				//Chord = 0,
				//Square root = 1,
				//Uniform = 2,
				//Custom = 15
				//The scenario flag becomes 1 if the knot parameter is Custom or has no fit data, otherwise 2.
				spline.KnotParameterization = (KnotParameterization)this._mergedReaders.ReadBitLong();

				scenario = ((spline.KnotParameterization == KnotParameterization.Custom || (spline.Flags1 & SplineFlags1.UseKnotParameter) == 0) ? 1 : 2);
			}
			else if (scenario == 2)
			{
				spline.Flags1 |= SplineFlags1.MethodFitPoints;
			}
			else
			{
				//If the spline does not have fit data, then the knot parameter should become Custom.
				spline.KnotParameterization = KnotParameterization.Custom;
			}

			//Common:
			//Degree BL degree of this spline
			spline.Degree = this._objectReader.ReadBitLong();

			int numfitpts = 0;
			int numknots = 0;
			int numctrlpts = 0;
			bool flag = false;
			switch (scenario)
			{
				case 2:
					//Fit Tol BD 44
					spline.FitTolerance = this._objectReader.ReadBitDouble();
					//Beg tan vec 3BD 12 Beginning tangent direction vector (normalized).
					spline.StartTangent = this._objectReader.Read3BitDouble();
					//End tan vec 3BD 13 Ending tangent direction vector (normalized).
					spline.EndTangent = this._objectReader.Read3BitDouble();
					//num fit pts BL 74 Number of fit points.
					//Stored as a LONG, although it is defined in DXF as a short.
					//You can see this if you create a spline with >=256 fit points
					numfitpts = this._objectReader.ReadBitLong();
					break;
				case 1:
					//Rational B flag bit 2
					if (this._objectReader.ReadBit())
						spline.Flags |= SplineFlags.Rational;
					//Closed B flag bit 0
					if (this._objectReader.ReadBit())
						spline.Flags |= SplineFlags.Closed;
					//Periodic B flag bit 1
					if (this._objectReader.ReadBit())
						spline.Flags |= SplineFlags.Periodic;
					//Knot tol BD 42
					spline.KnotTolerance = this._objectReader.ReadBitDouble();
					//Ctrl tol BD 43
					spline.ControlPointTolerance = this._objectReader.ReadBitDouble();

					//Numknots BL 72 This is stored as a LONG
					//although it is defined in DXF as a short.
					//You can see this if you create a spline with >=256 knots.
					numknots = this._objectReader.ReadBitLong();
					//Numctrlpts BL 73 Number of 10's (and 41's, if weighted) that follow.
					//Same, stored as LONG, defined in DXF as a short.
					numctrlpts = this._objectReader.ReadBitLong();
					//Weight B Seems to be an echo of the 4 bit on the flag for "weights present".
					flag = this._objectReader.ReadBit();
					break;
			}
			for (int i = 0; i < numknots; i++)
			{
				//Knot BD knot value
				spline.Knots.Add(this._objectReader.ReadBitDouble());
			}
			for (int j = 0; j < numctrlpts; j++)
			{
				//Control pt 3BD 10
				spline.ControlPoints.Add(this._objectReader.Read3BitDouble());
				if (flag)
				{
					//Weight D 41 if present as indicated by 4 bit on flag
					spline.Weights.Add(this._objectReader.ReadBitDouble());
				}
			}
			for (int k = 0; k < numfitpts; k++)
			{
				//Fit pt 3BD
				spline.FitPoints.Add(this._objectReader.Read3BitDouble());
			}

			return template;
		}

		private CadTemplate readRay()
		{
			Ray ray = new Ray();
			CadEntityTemplate template = new CadEntityTemplate(ray);

			this.readCommonEntityData(template);

			//Point 3BD 10
			ray.StartPoint = this._objectReader.Read3BitDouble();
			//Vector 3BD 11
			ray.Direction = this._objectReader.Read3BitDouble();

			return template;
		}

		private CadTemplate readXLine()
		{
			XLine xline = new XLine();
			CadEntityTemplate template = new CadEntityTemplate(xline);

			this.readCommonEntityData(template);

			//3 RD: a point on the construction line
			xline.FirstPoint = this._objectReader.Read3BitDouble();
			//3 RD : another point
			xline.Direction = this._objectReader.Read3BitDouble();

			return template;
		}

		private CadTemplate readDictionary()
		{
			CadDictionary cadDictionary = new CadDictionary();
			CadDictionaryTemplate template = new CadDictionaryTemplate(cadDictionary);

			this.readCommonNonEntityData(template);

			//Common:
			//Numitems L number of dictonary items
			int nentries = this._objectReader.ReadBitLong();

			//R14 Only:
			if (this._version == ACadVersion.AC1014)
			{
				//Unknown R14 RC Unknown R14 byte, has always been 0
				byte zero = this._objectReader.ReadByte();
			}
			//R2000 +:
			if (this.R2000Plus)
			{
				//Cloning flag BS 281
				cadDictionary.ClonningFlags = (DictionaryCloningFlags)this._objectReader.ReadBitShort();
				//Hard Owner flag RC 280
				cadDictionary.HardOwnerFlag = this._objectReader.ReadByte() > 0;
			}

			//Common:
			for (int i = 0; i < nentries; ++i)
			{
				//Text TV string name of dictionary entry, numitems entries
				string name = this._textReader.ReadVariableText();
				//Handle refs H parenthandle (soft relative pointer)
				//[Reactors(soft pointer)]
				//xdicobjhandle(hard owner)
				//itemhandles (soft owner)
				ulong handle = this.handleReference();

				template.Entries.Add(name, handle);
			}

			return template;
		}

		private CadTemplate readDictionaryVar()
		{
			DictionaryVariable dictvar = new DictionaryVariable();
			CadTemplate<DictionaryVariable> template = new CadTemplate<DictionaryVariable>(dictvar);

			this.readCommonNonEntityData(template);

			//Intval RC an integer value
			this._objectReader.ReadByte();

			//BS a string
			dictvar.Value = this._textReader.ReadVariableText();

			return template;
		}

		private CadTemplate readMText()
		{
			MText mtext = new MText();
			CadTextEntityTemplate template = new CadTextEntityTemplate(mtext);

			this.readCommonEntityData(template);

			//Insertion pt3 BD 10 First picked point. (Location relative to text depends on attachment point (71).)
			mtext.InsertPoint = this._objectReader.Read3BitDouble();
			//Extrusion 3BD 210 Undocumented; appears in DXF and entget, but ACAD doesn't even bother to adjust it to unit length.
			mtext.Normal = this._objectReader.Read3BitDouble();
			//X-axis dir 3BD 11 Apparently the text x-axis vector. (Why not just a rotation?) ACAD maintains it as a unit vector.
			mtext.AlignmentPoint = this._objectReader.Read3BitDouble();
			//Rect width BD 41 Reference rectangle width (width picked by the user).
			mtext.RectangleWitdth = this._objectReader.ReadBitDouble();

			//R2007+:
			if (this.R2007Plus)
			{
				//Rect height BD 46 Reference rectangle height.
				mtext.RectangleHeight = this._objectReader.ReadBitDouble();
			}

			//Common:
			//Text height BD 40 Undocumented
			mtext.Height = this._objectReader.ReadBitDouble();
			//Attachment BS 71 Similar to justification; see DXF doc
			mtext.AttachmentPoint = (AttachmentPointType)this._objectReader.ReadBitShort();
			//Drawing dir BS 72 Left to right, etc.; see DXF doc
			mtext.DrawingDirection = (DrawingDirectionType)this._objectReader.ReadBitShort();
			//Extents ht BD ---Undocumented and not present in DXF or entget
			this._objectReader.ReadBitDouble();
			//Extents wid BD ---Undocumented and not present in DXF or entget
			this._objectReader.ReadBitDouble();
			//Text TV 1 All text in one long string (Autocad format)
			mtext.Value = this._textReader.ReadVariableText();

			//H 7 STYLE (hard pointer)
			template.StyleHandle = this.handleReference();

			//R2000+:
			if (this.R2000Plus)
			{
				//Linespacing Style BS 73
				mtext.LineSpacingStyle = (LineSpacingStyleType)this._objectReader.ReadBitShort();
				//Linespacing Factor BD 44
				mtext.LineSpacing = this._objectReader.ReadBitDouble();
				//Unknown bit B
				this._objectReader.ReadBit();
			}

			//R2004+:
			if (this.R2004Plus)
			{
				//Background flags BL 90 0 = no background, 1 = background fill, 2 = background fill with drawing fill color, 0x10 = text frame (R2018+)
				mtext.BackgroundFillFlags = (BackgroundFillFlags)this._objectReader.ReadBitLong();

				//background flags has bit 0x01 set, or in case of R2018 bit 0x10:
				if ((mtext.BackgroundFillFlags & BackgroundFillFlags.UseBackgroundFillColor) != BackgroundFillFlags.None
					|| this._version > ACadVersion.AC1027
					&& (mtext.BackgroundFillFlags & BackgroundFillFlags.TextFrame) > 0)
				{
					//Background scale factor	BL 45 default = 1.5
					mtext.BackgroundScale = this._objectReader.ReadBitDouble();
					//Background color CMC 63
					mtext.BackgroundColor = this._mergedReaders.ReadCmColor();
					//Background transparency BL 441
					mtext.BackgroundTransparency = new Transparency((short)this._objectReader.ReadBitLong());
				}
			}

			//R2018+
			if (!this.R2018Plus)
				return template;

			//Is NOT annotative B
			mtext.IsAnnotative = !this._objectReader.ReadBit();

			//IF MTEXT is not annotative
			if (!mtext.IsAnnotative)
			{
				//Version BS Default 0
				var version = this._objectReader.ReadBitShort();
				//Default flag B Default true
				var defaultFlag = this._objectReader.ReadBit();

				//BEGIN REDUNDANT FIELDS(see above for descriptions)
				//Registered application H Hard pointer
				ulong appHandle = this.handleReference();

				//TODO: finish Mtext reader, save redundant fields??

				//Attachment point BL
				AttachmentPointType attachmentPoint = (AttachmentPointType)this._objectReader.ReadBitLong();
				//X - axis dir 3BD 10
				this._objectReader.Read3BitDouble();
				//Insertion point 3BD 11
				this._objectReader.Read3BitDouble();
				//Rect width BD 40
				this._objectReader.ReadBitDouble();
				//Rect height BD 41
				this._objectReader.ReadBitDouble();
				//Extents width BD 42
				this._objectReader.ReadBitDouble();
				//Extents height BD 43
				this._objectReader.ReadBitDouble();
				//END REDUNDANT FIELDS

				//Column type BS 71 0 = No columns, 1 = static columns, 2 = dynamic columns
				mtext.Column.ColumnType = (ColumnType)this._objectReader.ReadBitShort();
				//IF Has Columns data(column type is not 0)
				if (mtext.Column.ColumnType != ColumnType.NoColumns)
				{
					//Column height count BL 72
					int count = this._objectReader.ReadBitLong();
					//Columnn width BD 44
					mtext.Column.ColumnWidth = this._objectReader.ReadBitDouble();
					//Gutter BD 45
					mtext.Column.ColumnGutter = this._objectReader.ReadBitDouble();
					//Auto height? B 73
					mtext.Column.ColumnAutoHeight = this._objectReader.ReadBit();
					//Flow reversed? B 74
					mtext.Column.ColumnFlowReversed = this._objectReader.ReadBit();

					//IF not auto height and column type is dynamic columns
					if (!mtext.Column.ColumnAutoHeight && mtext.Column.ColumnType == ColumnType.DynamicColumns && count > 0)
					{
						for (int i = 0; i < count; ++i)
						{
							//Column height BD 46
							mtext.Column.ColumnHeights.Add(this._objectReader.ReadBitDouble());
						}
					}
				}
			}

			return template;
		}

		private CadTemplate readLeader()
		{
			Leader leader = new Leader();
			CadLeaderTemplate template = new CadLeaderTemplate(leader);

			this.readCommonEntityData(template);

			//Unknown bit B --- Always seems to be 0.
			this._objectReader.ReadBit();

			//Annot type BS --- Annotation type (NOT bit-coded):
			//Value 0 : MTEXT
			//Value 1 : TOLERANCE
			//Value 2 : INSERT
			//Value 3 : None
			leader.CreationType = (LeaderCreationType)this._objectReader.ReadBitShort();
			//path type BS ---
			leader.PathType = (LeaderPathType)this._objectReader.ReadBitShort();

			//numpts BL --- number of points
			int npts = this._objectReader.ReadBitLong();
			for (int i = 0; i < npts; i++)
			{
				//point 3BD 10 As many as counter above specifies.
				leader.Vertices.Add(this._objectReader.Read3BitDouble());
			}

			//Origin 3BD --- The leader plane origin (by default it’s the first point).
			this._objectReader.Read3BitDouble();    //Is necessary to store this value?
													//Extrusion 3BD 210
			leader.Normal = this._objectReader.Read3BitDouble();
			//x direction 3BD 211
			leader.HorizontalDirection = this._objectReader.Read3BitDouble();
			//offsettoblockinspt 3BD 212 Used when the BLOCK option is used. Seems to be an unused feature.
			leader.BlockOffset = this._objectReader.Read3BitDouble();

			//R14+:
			if (this._version >= ACadVersion.AC1014)
			{
				//Endptproj 3BD --- A non-planar leader gives a point that projects the endpoint back to the annotation.
				//It's the offset from the endpoint of the leader to the annotation, taking into account the extrusion direction.
				leader.AnnotationOffset = this._objectReader.Read3BitDouble();
			}

			//R13-R14 Only:
			if (this.R13_14Only)
			{
				//DIMGAP BD --- The value of DIMGAP in the associated DIMSTYLE at the time of creation, multiplied by the dimscale in that dimstyle.
				leader.Style.DimensionLineGap = this._objectReader.ReadBitDouble();
			}

			//Common:
			if (this._version <= ACadVersion.AC1021)
			{
				//For higher versions this values are wrong and it works best if they are not read
				//Box height BD 40 MTEXT extents height. (A text box is slightly taller, probably by some DIMvar amount.)
				leader.TextHeight = this._objectReader.ReadBitDouble();
				//Box width BD 41 MTEXT extents width. (A text box is slightly wider, probably by some DIMvar amount.)
				leader.TextWidth = this._objectReader.ReadBitDouble();
			}

			//Hooklineonxdir B hook line is on x direction if 1
			leader.HookLineDirection = this._objectReader.ReadBit();
			//Arrowheadon B arrowhead on indicator
			leader.ArrowHeadEnabled = this._objectReader.ReadBit();

			//R13-R14 Only:
			if (this.R13_14Only)
			{
				//Arrowheadtype BS arrowhead type
				this._objectReader.ReadBitShort();
				//Dimasz BD DIMASZ at the time of creation, multiplied by DIMSCALE
				template.Dimasz = this._objectReader.ReadBitDouble();
				//Unknown B
				this._objectReader.ReadBit();
				//Unknown B
				this._objectReader.ReadBit();
				//Unknown BS
				this._objectReader.ReadBitShort();
				//Byblockcolor BS
				this._objectReader.ReadBitShort();
				//Unknown B
				this._objectReader.ReadBit();
				//Unknown B
				this._objectReader.ReadBit();
			}

			//R2000+:
			if (this.R2000Plus)
			{
				//Unknown BS
				this._objectReader.ReadBitShort();
				//Unknown B
				this._objectReader.ReadBit();
				//Unknown B
				this._objectReader.ReadBit();
			}

			//H 340 Associated annotation
			template.AnnotationHandle = this.handleReference();
			//H 2 DIMSTYLE (hard pointer)
			template.DIMSTYLEHandle = this.handleReference();

			return template;
		}

		private CadTemplate readMLine()
		{
			MLine mline = new MLine();
			CadMLineTemplate template = new CadMLineTemplate(mline);

			this.readCommonEntityData(template);

			//Scale BD 40
			mline.ScaleFactor = this._objectReader.ReadBitDouble();
			//Just EC top (0), bottom(2), or center(1)
			mline.Justification = (MLineJustification)this._objectReader.ReadByte();
			//Base point 3BD 10
			mline.StartPoint = this._objectReader.Read3BitDouble();
			//Extrusion 3BD 210 etc.
			mline.Extrusion = this._objectReader.Read3BitDouble();

			//Openclosed BS open (1), closed(3)
			mline.Flags |= this._objectReader.ReadBitShort() == 3 ? MLineFlags.Closed : MLineFlags.Has;

			//Linesinstyle RC 73
			int nlines = (int)this._objectReader.ReadByte();

			//Numverts BS 72
			int nverts = (int)this._objectReader.ReadBitShort();
			for (int i = 0; i < nverts; ++i)
			{
				MLine.Vertex vertex = new MLine.Vertex();

				//vertex 3BD
				vertex.Position = this._objectReader.Read3BitDouble();
				//vertex direction 3BD
				vertex.Direction = this._objectReader.Read3BitDouble();
				//miter direction 3BD
				vertex.Miter = this._objectReader.Read3BitDouble();

				for (int j = 0; j < nlines; ++j)
				{
					MLine.Vertex.Segment element = new MLine.Vertex.Segment();

					//numsegparms BS
					int nsegparms = (int)this._objectReader.ReadBitShort();
					for (int k = 0; k < nsegparms; ++k)
					{
						//segparm BD segment parameter
						element.Parameters.Add(this._objectReader.ReadBitDouble());
					}

					//numareafillparms BS
					int nfillparms = (int)this._objectReader.ReadBitShort();
					for (int k = 0; k < nfillparms; ++k)
					{
						//areafillparm BD area fill parameter
						element.AreaFillParameters.Add(this._objectReader.ReadBitDouble());
					}

					vertex.Segments.Add(element);
				}

				mline.Vertices.Add(vertex);
			}

			//H mline style oject handle (hard pointer)
			template.MLineStyleHandle = this.handleReference();

			return template;
		}

		private CadTemplate readBlockControlObject()
		{
			CadBlockCtrlObjectTemplate template = new CadBlockCtrlObjectTemplate(
				new BlockRecordsTable());

			this.readDocumentTable(template.CadObject, template);

			//*MODEL_SPACE and *PAPER_SPACE(hard owner).
			template.ModelSpaceHandle = this.handleReference();
			template.PaperSpaceHandle = this.handleReference();

			return template;
		}

		private CadTemplate readBlockHeader()
		{
			BlockRecord record = new BlockRecord();
			Block block = record.BlockEntity;

			CadBlockRecordTemplate template = new CadBlockRecordTemplate(record);
			this._builder.BlockRecordTemplates.Add(template);

			this.readCommonNonEntityData(template);

			//Common:
			//Entry name TV 2
			//Warning: names ended with a number are not readed in this method
			string name = this._textReader.ReadVariableText();
			if (name.Equals(BlockRecord.ModelSpaceName, System.StringComparison.CurrentCultureIgnoreCase) ||
				name.Equals(BlockRecord.PaperSpaceName, System.StringComparison.CurrentCultureIgnoreCase))
				record.Name = name;

			this.readXrefDependantBit(template.CadObject);

			//Anonymous B 1 if this is an anonymous block (1 bit)
			if (this._objectReader.ReadBit())
				block.Flags |= BlockTypeFlags.Anonymous;

			//Hasatts B 1 if block contains attdefs (2 bit)
			bool hasatts = this._objectReader.ReadBit();

			//Blkisxref B 1 if block is xref (4 bit)
			if (this._objectReader.ReadBit())
				block.Flags |= BlockTypeFlags.XRef;

			//Xrefoverlaid B 1 if an overlaid xref (8 bit)
			if (this._objectReader.ReadBit())
				block.Flags |= BlockTypeFlags.XRefOverlay;

			//R2000+:
			if (this.R2000Plus)
				//Loaded Bit B 0 indicates loaded for an xref
				if (this._objectReader.ReadBit())
					block.Flags |= BlockTypeFlags.XRef;

			//R2004+:
			int nownedObjects = 0;
			if (this.R2004Plus
				&& !block.Flags.HasFlag(BlockTypeFlags.XRef)
				&& !block.Flags.HasFlag(BlockTypeFlags.XRefOverlay))
				//Owned Object Count BL Number of objects owned by this object.
				nownedObjects = this._objectReader.ReadBitLong();

			//Common:
			//Base pt 3BD 10 Base point of block.
			block.BasePoint = this._objectReader.Read3BitDouble();
			//Xref pname TV 1 Xref pathname. That's right: DXF 1 AND 3!
			//3 1 appears in a tblnext/ search elist; 3 appears in an entget.
			block.XrefPath = this._textReader.ReadVariableText();

			//R2000+:
			int insertCount = 0;
			if (this.R2000Plus)
			{
				//Insert Count RC A sequence of zero or more non-zero RC’s, followed by a terminating 0 RC.The total number of these indicates how many insert handles will be present.
				for (byte i = this._objectReader.ReadByte(); i > 0; i = this._objectReader.ReadByte())
					++insertCount;

				//Block Description TV 4 Block description.
				block.Comments = this._textReader.ReadVariableText();

				//Size of preview data BL Indicates number of bytes of data following.
				int n = this._objectReader.ReadBitLong();
				List<byte> data = new List<byte>();
				for (int index = 0; index < n; ++index)
				{
					//Binary Preview Data N*RC 310
					data.Add(this._objectReader.ReadByte());
				}

				record.Preview = data.ToArray();
			}

			//R2007+:
			if (this.R2007Plus)
			{
				//Insert units BS 70
				record.Units = (UnitsType)this._objectReader.ReadBitShort();
				//Explodable B 280
				record.IsExplodable = this._objectReader.ReadBit();
				//Block scaling RC 281
				record.CanScale = this._objectReader.ReadByte() > 0;
			}

			//NULL(hard pointer)
			this.handleReference();
			//BLOCK entity. (hard owner)
			//Block begin object
			template.BeginBlockHandle = this.handleReference();

			//R13-R2000:
			if (this._version >= ACadVersion.AC1012 && this._version <= ACadVersion.AC1015
					&& !block.Flags.HasFlag(BlockTypeFlags.XRef)
					&& !block.Flags.HasFlag(BlockTypeFlags.XRefOverlay))
			{
				//first entity in the def. (soft pointer)
				template.FirstEntityHandle = this.handleReference();
				//last entity in the def. (soft pointer)
				template.LastEntityHandle = this.handleReference();
			}

			//R2004+:
			if (this.R2004Plus)
			{
				for (int i = 0; i < nownedObjects; ++i)
					//H[ENTITY(hard owner)] Repeats “Owned Object Count” times.
					template.OwnedObjectsHandlers.Add(this.handleReference());
			}

			//Common:
			//ENDBLK entity. (hard owner)
			template.EndBlockHandle = this.handleReference();

			//R2000+:
			if (this.R2000Plus)
			{
				//Insert Handles H N insert handles, where N corresponds to the number of insert count entries above(soft pointer).
				for (int i = 0; i < insertCount; ++i)
				{
					//Entries	//TODO: necessary to store the insert handles??
					template.InsertHandles.Add(this.handleReference());
				}
				//Layout Handle H(hard pointer)
				template.LayoutHandle = this.handleReference();
			}

			return template;
		}

		private CadTemplate readLayer()
		{
			//Initialize the template with the default layer
			Layer layer = new Layer();
			CadLayerTemplate template = new CadLayerTemplate(layer);

			this.readCommonNonEntityData(template);

			//Common:
			//Entry name TV 2
			layer.Name = this._textReader.ReadVariableText();
			//layer.Name = m_objectReader.ReadVariableText();

			this.readXrefDependantBit(template.CadObject);

			//R13-R14 Only:
			if (this.R13_14Only)
			{
				//Frozen B 70 if frozen (1 bit)
				if (this._objectReader.ReadBit())
					layer.Flags |= LayerFlags.Frozen;

				//On B if on.
				layer.IsOn = this._objectReader.ReadBit();

				//Frz in new B 70 if frozen by default in new viewports (2 bit)
				if (this._objectReader.ReadBit())
					layer.Flags |= LayerFlags.FrozenNewViewports;

				//Locked B 70 if locked (4 bit)
				if (this._objectReader.ReadBit())
					layer.Flags |= LayerFlags.Locked;
			}
			//R2000+:
			if (this.R2000Plus)
			{
				//Values BS 70,290,370
				short values = this._objectReader.ReadBitShort();

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
				layer.LineWeight = (LineweightType)((values & 0x3E0) >> 5);
			}

			//Common:
			//Color CMC 62
			layer.Color = this._mergedReaders.ReadCmColor();

			//Handle refs H Layer control (soft pointer)
			//[Reactors(soft pointer)]
			//xdicobjhandle(hard owner)
			//External reference block handle(hard pointer)
			template.LayerControlHandle = this.handleReference();

			//R2000+:
			if (this.R2000Plus)
				//H 390 Plotstyle (hard pointer), by default points to PLACEHOLDER with handle 0x0f.
				template.PlotStyleHandle = this.handleReference();

			//R2007+:
			if (this.R2007Plus)
			{
				//H 347 Material
				template.MaterialHandle = this.handleReference();
			}

			//Common:
			//H 6 linetype (hard pointer)
			template.LineTypeHandle = this.handleReference();

			//H Unknown handle (hard pointer). Always seems to be NULL.
			//Some times is not...
			//handleReference();

			return template;
		}

		private CadTemplate readTextStyle()
		{
			TextStyle style = new TextStyle();
			CadTableEntryTemplate<TextStyle> template = new CadTableEntryTemplate<TextStyle>(style);

			this.readCommonNonEntityData(template);

			//Common:
			//Entry name TV 2
			style.Name = this._textReader.ReadVariableText();

			this.readXrefDependantBit(template.CadObject);

			//shape file B 1 if a shape file rather than a font (1 bit)
			if (this._objectReader.ReadBit())
				style.Flags |= StyleFlags.IsShape;
			//Vertical B 1 if vertical (4 bit of flag)
			if (this._objectReader.ReadBit())
				style.Flags |= StyleFlags.VerticalText;
			//Fixed height BD 40
			style.Height = this._objectReader.ReadBitDouble();
			//Width factor BD 41
			style.Width = this._objectReader.ReadBitDouble();
			//Oblique ang BD 50
			style.ObliqueAngle = this._objectReader.ReadBitDouble();
			//Generation RC 71 Generation flags (not bit-pair coded).
			style.MirrorFlag = (TextMirrorFlag)this._objectReader.ReadByte();
			//Last height BD 42
			style.LastHeight = this._objectReader.ReadBitDouble();
			//Font name TV 3
			style.Filename = this._textReader.ReadVariableText();
			//Bigfont name TV 4
			style.BigFontFilename = this._textReader.ReadVariableText();

			ulong styleControl = this.handleReference();

			return template;
		}

		private CadTemplate readLTypeControlObject()
		{
			CadTableTemplate<LineType> template = new CadTableTemplate<LineType>(
				new LineTypesTable());

			this.readDocumentTable(template.CadObject, template);

			//the linetypes, ending with BYLAYER and BYBLOCK.
			template.EntryHandles.Add(this.handleReference());
			template.EntryHandles.Add(this.handleReference());

			return template;
		}

		private CadTemplate readLType()
		{
			LineType ltype = new LineType();
			CadLineTypeTemplate template = new CadLineTypeTemplate(ltype);

			this.readCommonNonEntityData(template);

			//Common:
			//Entry name TV 2
			ltype.Name = this._textReader.ReadVariableText();

			this.readXrefDependantBit(template.CadObject);

			//Description TV 3
			ltype.Description = this._textReader.ReadVariableText();
			//Pattern Len BD 40
			ltype.PatternLen = this._objectReader.ReadBitDouble();
			//Alignment RC 72 Always 'A'.
			ltype.Alignment = this._objectReader.ReadRawChar();

			//Numdashes RC 73 The number of repetitions of the 49...74 data.
			int ndashes = this._objectReader.ReadByte();
			//Hold the text flag
			bool isText = false;
			for (int i = 0; i < ndashes; i++)
			{
				CadLineTypeTemplate.SegmentTemplate segment = new CadLineTypeTemplate.SegmentTemplate();

				//Dash length BD 49 Dash or dot specifier.
				segment.Segment.Length = this._objectReader.ReadBitDouble();
				//Complex shapecode BS 75 Shape number if shapeflag is 2, or index into the string area if shapeflag is 4.
				segment.Segment.ShapeNumber = this._objectReader.ReadBitShort();

				//X - offset RD 44 (0.0 for a simple dash.)
				//Y - offset RD 45(0.0 for a simple dash.)
				XY offset = new XY(this._objectReader.ReadDouble(), this._objectReader.ReadDouble());
				segment.Segment.Offset = offset;

				//Scale BD 46 (1.0 for a simple dash.)
				segment.Segment.Scale = this._objectReader.ReadBitDouble();
				//Rotation BD 50 (0.0 for a simple dash.)
				segment.Segment.Rotation = this._objectReader.ReadBitDouble();
				//Shapeflag BS 74 bit coded:
				segment.Segment.Shapeflag = (LinetypeShapeFlags)this._objectReader.ReadBitShort();

				if (segment.Segment.Shapeflag.HasFlag(LinetypeShapeFlags.Text))
					isText = true;

				//Add the segment to the type
				template.SegmentTemplates.Add(segment);
			}

			//R2004 and earlier:
			if (this._version <= ACadVersion.AC1018)
			{
				//Strings area X 9 256 bytes of text area. The complex dashes that have text use this area via the 75-group indices. It's basically a pile of 0-terminated strings. First byte is always 0 for R13 and data starts at byte 1. In R14 it is not a valid data start from byte 0.
				//(The 9 - group is undocumented.)
				byte[] textarea = this._objectReader.ReadBytes(256);
				//TODO: Read the line type text area
			}
			//R2007+:
			if (this.R2007Plus && isText)
			{
				byte[] textarea = this._objectReader.ReadBytes(256);
				//TODO: Read the line type text area
			}

			//Common:
			//Handle refs H Ltype control(soft pointer)
			//[Reactors (soft pointer)]
			//xdicobjhandle(hard owner)
			//External reference block handle(hard pointer)
			template.LtypeControlHandle = this.handleReference();

			//340 shapefile for dash/shape (1 each) (hard pointer)
			for (int i = 0; i < ndashes; i++)
			{
				template.SegmentTemplates[i].StyleHandle = this.handleReference();
			}

			this._builder.LineTypes.Add(ltype.Name, ltype);

			return template;
		}

		private CadTemplate readView()
		{
			View view = new View();
			CadViewTemplate template = new CadViewTemplate(view);

			this.readCommonNonEntityData(template);

			//Common:
			//Entry name TV 2
			view.Name = this._textReader.ReadVariableText();

			this.readXrefDependantBit(view);

			//View height BD 40
			view.Height = this._objectReader.ReadBitDouble();
			//View width BD 41
			view.Width = this._objectReader.ReadBitDouble();
			//View center 2RD 10(Not bit - pair coded.)
			view.Center = this._objectReader.Read2RawDouble();
			//Target 3BD 12
			view.Target = this._objectReader.Read3BitDouble();
			//View dir 3BD 11 DXF doc suggests from target toward camera.
			view.Direction = this._objectReader.Read3BitDouble();
			//Twist angle BD 50 Radians
			view.Angle = this._objectReader.ReadBitDouble();
			//Lens length BD 42
			view.LensLength = this._objectReader.ReadBitDouble();
			//Front clip BD 43
			view.FrontClipping = this._objectReader.ReadBitDouble();
			//Back clip BD 44
			view.BackClipping = this._objectReader.ReadBitDouble();

			//View mode X 71 4 bits: 0123
			//Note that only bits 0, 1, 2, and 4 of the 71 can be specified -- not bit 3 (8).
			//0 : 71's bit 0 (1)
			if (this._objectReader.ReadBit())
				view.ViewMode |= ViewModeType.PerspectiveView;
			//1 : 71's bit 1 (2)
			if (this._objectReader.ReadBit())
				view.ViewMode |= ViewModeType.FrontClipping;
			//2 : 71's bit 2 (4)
			if (this._objectReader.ReadBit())
				view.ViewMode |= ViewModeType.BackClipping;
			//3 : OPPOSITE of 71's bit 4 (16)
			if (this._objectReader.ReadBit())
				view.ViewMode |= ViewModeType.FrontClippingZ;

			//R2000+:
			if (this.R2000Plus)
			{
				//Render Mode RC 281
				view.RenderMode = (RenderMode)this._objectReader.ReadByte();
			}

			//R2007+:
			if (this.R2007Plus)
			{
				//Use default lights B ? Default value is true
				this._mergedReaders.ReadBit();
				//Default lighting RC ? Default value is 1
				this._mergedReaders.ReadByte();
				//Brightness BD ? Default value is 0
				this._mergedReaders.ReadBitDouble();
				//Contrast BD ? Default value is 0
				this._mergedReaders.ReadBitDouble();
				//Abient color CMC? Default value is AutoCAD indexed color 250
				this._mergedReaders.ReadCmColor();
			}

			//Common:
			//Pspace flag B 70 Bit 0(1) of the 70 - group.
			if (this._objectReader.ReadBit())
				view.Flags |= (StandardFlags)0b1;

			if (this.R2000Plus)
			{
				view.IsUcsAssociated = this._objectReader.ReadBit();
				if (view.IsUcsAssociated)
				{
					//Origin 3BD 10 This and next 4 R2000 items are present only if 72 value is 1.
					view.UcsOrigin = this._objectReader.Read3BitDouble();
					//X-direction 3BD 11
					view.UcsXAxis = this._objectReader.Read3BitDouble();
					//Y-direction 3BD 12
					view.UcsYAxis = this._objectReader.Read3BitDouble();
					//Elevation BD 146
					view.UcsElevation = this._objectReader.ReadBitDouble();
					//OrthographicViewType BS 79
					view.UcsOrthographicType = (OrthographicType)this._objectReader.ReadBitShort();
				}
			}

			//Common:
			//Handle refs H view control object (soft pointer)
			this.handleReference();

			//R2007+:
			if (this.R2007Plus)
			{
				//Camera plottable B 73
				view.IsPlottable = this._objectReader.ReadBit();

				//Background handle H 332 soft pointer
				this.handleReference();
				//Visual style H 348 hard pointer
				this.handleReference();
				//Sun H 361 hard owner
				this.handleReference();
			}

			if (this.R2000Plus && view.IsUcsAssociated)
			{
				//Base UCS Handle H 346 hard pointer
				template.UcsHandle = this.handleReference();
				//Named UCS Handle H 345 hard pointer
				template.NamedUcsHandle = this.handleReference();
			}

			//R2007+:
			if (this.R2007Plus)
			{
				//Live section H 334 soft pointer
				this.handleReference();
			}

			return template;
		}

		private CadTemplate readUcs()
		{
			UCS ucs = new UCS();
			CadTemplate<UCS> template = new CadTemplate<UCS>(ucs);

			this.readCommonNonEntityData(template);

			//Common:
			//Entry name TV 2
			ucs.Name = this._textReader.ReadVariableText();

			this.readXrefDependantBit(ucs);

			//Origin 3BD 10
			ucs.Origin = this._objectReader.Read3BitDouble();
			//X - direction 3BD 11
			ucs.XAxis = this._objectReader.Read3BitDouble();
			//Y - direction 3BD 12
			ucs.YAxis = this._objectReader.Read3BitDouble();

			//R2000+:
			if (this.R2000Plus)
			{
				//Elevation BD 146
				ucs.Elevation = this._objectReader.ReadBitDouble();
				//OrthographicViewType BS 79	//dxf docs: 79	Always 0
				ucs.OrthographicViewType = (OrthographicType)this._objectReader.ReadBitShort();
				//OrthographicType BS 71
				ucs.OrthographicType = (OrthographicType)this._objectReader.ReadBitShort();
			}

			//Common:
			//Handle refs H ucs control object (soft pointer)
			long control = (long)this.handleReference();

			//R2000 +:
			if (this.R2000Plus)
			{
				//Base UCS Handle H 346 hard pointer
				long baseUcs = (long)this.handleReference();
				//Named UCS Handle H -hard pointer, not present in DXF
				long namedHandle = (long)this.handleReference();
			}

			return template;
		}

		private CadTemplate readVPort()
		{
			VPort vport = new VPort();
			CadVPortTemplate template = new CadVPortTemplate(vport);

			this.readCommonNonEntityData(template);

			//Common:
			//Entry name TV 2
			vport.Name = this._textReader.ReadVariableText();

			this.readXrefDependantBit(vport);

			//View height BD 40
			vport.ViewHeight = this._objectReader.ReadBitDouble();
			//Aspect ratio BD 41 The number stored here is actually the aspect ratio times the view height (40),
			//so this number must be divided by the 40-value to produce the aspect ratio that entget gives.
			//(R13 quirk; R12 has just the aspect ratio.)
			vport.AspectRatio = this._objectReader.ReadBitDouble() / vport.ViewHeight;
			//View Center 2RD 12 DCS. (If it's plan view, add the view target (17) to get the WCS coordinates.
			//Careful! Sometimes you have to SAVE/OPEN to update the .dwg file.) Note that it's WSC in R12.
			vport.Center = this._objectReader.Read2RawDouble();
			//View target 3BD 17
			vport.Target = this._objectReader.Read3BitDouble();
			//View dir 3BD 16
			vport.Direction = this._objectReader.Read3BitDouble();
			//View twist BD 51
			vport.TwistAngle = this._objectReader.ReadBitDouble();
			//Lens length BD 42
			vport.LensLength = this._objectReader.ReadBitDouble();
			//Front clip BD 43
			vport.FrontClippingPlane = this._objectReader.ReadBitDouble();
			//Back clip BD 44
			vport.BackClippingPlane = this._objectReader.ReadBitDouble();

			//View mode X 71 4 bits: 0123
			//Note that only bits 0, 1, 2, and 4 are given here; see UCSFOLLOW below for bit 3(8) of the 71.
			//0 : 71's bit 0 (1)
			if (this._objectReader.ReadBit())
				vport.ViewMode |= ViewModeType.PerspectiveView;
			//1 : 71's bit 1 (2)
			if (this._objectReader.ReadBit())
				vport.ViewMode |= ViewModeType.FrontClipping;
			//2 : 71's bit 2 (4)
			if (this._objectReader.ReadBit())
				vport.ViewMode |= ViewModeType.BackClipping;
			//3 : OPPOSITE of 71's bit 4 (16)
			if (this._objectReader.ReadBit())
				vport.ViewMode |= ViewModeType.FrontClippingZ;

			//R2000+:
			if (this.R2000Plus)
			{
				//Render Mode RC 281
				vport.RenderMode = (RenderMode)this._objectReader.ReadByte();
			}

			//R2007+:
			if (this.R2007Plus)
			{
				//Use default lights B 292
				vport.UseDefaultLighting = this._objectReader.ReadBit();
				//Default lighting type RC 282
				vport.DefaultLighting = (DefaultLightingType)this._objectReader.ReadByte();
				//Brightness BD 141
				vport.Brightness = this._objectReader.ReadBitDouble();
				//Constrast BD 142
				vport.Contrast = this._objectReader.ReadBitDouble();
				//Ambient Color CMC 63
				vport.AmbientColor = this._mergedReaders.ReadCmColor();
			}

			//Common:
			//Lower left 2RD 10 In fractions of screen width and height.
			vport.BottomLeft = this._objectReader.Read2RawDouble();
			//Upper right 2RD 11 In fractions of screen width and height.
			vport.TopRight = this._objectReader.Read2RawDouble();

			//UCSFOLLOW B 71 UCSFOLLOW. Bit 3 (8) of the 71-group.
			if (this._objectReader.ReadBit())
				vport.ViewMode |= ViewModeType.Follow;

			//Circle zoom BS 72 Circle zoom percent.
			vport.CircleZoomPercent = this._objectReader.ReadBitShort();

			//Fast zoom B 73
			this._objectReader.ReadBit();

			//UCSICON X 74 2 bits: 01
			//0 : 74's bit 0 (1)
			if (this._objectReader.ReadBit())
				vport.UcsIconDisplay = UscIconType.OnLower;
			//1 : 74's bit 1 (2)
			if (this._objectReader.ReadBit())
				vport.UcsIconDisplay = UscIconType.OnOrigin;

			//Grid on/off B 76
			vport.ShowGrid = this._objectReader.ReadBit();
			//Grd spacing 2RD 15
			vport.GridSpacing = this._objectReader.Read2RawDouble();
			//Snap on/off B 75
			vport.SnapOn = this._objectReader.ReadBit();

			//Snap style B 77
			vport.IsometricSnap = this._objectReader.ReadBit();

			//Snap isopair BS 78
			vport.SnapIsoPair = this._objectReader.ReadBitShort();
			//Snap rot BD 50
			vport.SnapRotation = this._objectReader.ReadBitDouble();
			//Snap base 2RD 13
			vport.SnapBasePoint = this._objectReader.Read2RawDouble();
			//Snp spacing 2RD 14
			vport.SnapSpacing = this._objectReader.Read2RawDouble();

			//R2000+:
			if (this.R2000Plus)
			{
				//Unknown B
				this._objectReader.ReadBit();

				//UCS per Viewport B 71
				bool ucsPerViewport = this._objectReader.ReadBit();
				//UCS Origin 3BD 110
				vport.Origin = this._objectReader.Read3BitDouble();
				//UCS X Axis 3BD 111
				vport.XAxis = this._objectReader.Read3BitDouble();
				//UCS Y Axis 3BD 112
				vport.YAxis = this._objectReader.Read3BitDouble();
				//UCS Elevation BD 146
				vport.Elevation = this._objectReader.ReadBitDouble();
				//UCS Orthographic type BS 79
				vport.OrthographicType = (OrthographicType)this._objectReader.ReadBitShort();
			}

			//R2007+:
			if (this.R2007Plus)
			{
				//Grid flags BS 60
				vport.GridFlags = (GridFlags)this._objectReader.ReadBitShort();
				//Grid major BS 61
				vport.MinorGridLinesPerMajorGridLine = this._objectReader.ReadBitShort();
			}

			//Common:
			//Handle refs H Vport control(soft pointer)
			//[Reactors(soft pointer)]
			//xdicobjhandle(hard owner)
			//External reference block handle(hard pointer)
			template.VportControlHandle = this.handleReference();

			//R2007+:
			if (this.R2007Plus)
			{
				//Background handle H 332 soft pointer
				template.BackgroundHandle = this.handleReference();
				//Visual Style handle H 348 hard pointer
				template.StyleHandle = this.handleReference();
				//Sun handle H 361 hard owner
				template.SunHandle = this.handleReference();
			}

			//R2000+:
			if (this.R2000Plus)
			{
				//Named UCS Handle H 345 hard pointer
				template.NamedUcsHandle = this.handleReference();
				//Base UCS Handle H 346 hard pointer
				template.BaseUcsHandle = this.handleReference();
			}

			return template;
		}

		private CadTemplate readAppId()
		{
			AppId appId = new AppId();
			CadTemplate template = new CadTemplate<AppId>(appId);

			this.readCommonNonEntityData(template);

			appId.Name = this._textReader.ReadVariableText();

			this.readXrefDependantBit(appId);

			//Unknown RC 71 Undoc'd 71-group; doesn't even appear in DXF or an entget if it's 0.
			this._objectReader.ReadByte();
			//Handle refs H The app control(soft pointer)
			//[Reactors(soft pointer)]
			//xdicobjhandle(hard owner)
			//External reference block handle(hard pointer)
			this.handleReference();

			return template;
		}

		private CadTemplate readDimStyle()
		{
			DimensionStyle dimStyle = new DimensionStyle();
			CadDimensionStyleTemplate template = new CadDimensionStyleTemplate(dimStyle);

			this.readCommonNonEntityData(template);

			//Common:
			//Entry name TV 2
			dimStyle.Name = this._textReader.ReadVariableText();

			this.readXrefDependantBit(dimStyle);

			//R13 & R14 Only:
			if (this.R13_14Only)
			{
				//DIMTOL B 71
				dimStyle.GenerateTolerances = this._objectReader.ReadBit();
				//DIMLIM B 72
				dimStyle.LimitsGeneration = this._objectReader.ReadBit();
				//DIMTIH B 73
				dimStyle.TextOutsideHorizontal = this._objectReader.ReadBit();
				//DIMTOH B 74
				dimStyle.SuppressFirstExtensionLine = this._objectReader.ReadBit();
				//DIMSE1 B 75
				dimStyle.SuppressSecondExtensionLine = this._objectReader.ReadBit();
				//DIMSE2 B 76
				dimStyle.TextInsideHorizontal = this._objectReader.ReadBit();
				//DIMALT B 170
				dimStyle.AlternateUnitDimensioning = this._objectReader.ReadBit();
				//DIMTOFL B 172
				dimStyle.TextOutsideExtensions = this._objectReader.ReadBit();
				//DIMSAH B 173
				dimStyle.SeparateArrowBlocks = this._objectReader.ReadBit();
				//DIMTIX B 174
				dimStyle.TextInsideExtensions = this._objectReader.ReadBit();
				//DIMSOXD B 175
				dimStyle.SuppressOutsideExtensions = this._objectReader.ReadBit();
				//DIMALTD RC 171
				dimStyle.AlternateUnitDecimalPlaces = this._objectReader.ReadByte();
				//DIMZIN RC 78
				dimStyle.ZeroHandling = (ZeroHandling)this._objectReader.ReadRawChar();
				//DIMSD1 B 281
				dimStyle.SuppressFirstDimensionLine = this._objectReader.ReadBit();
				//DIMSD2 B 282
				dimStyle.SuppressSecondDimensionLine = this._objectReader.ReadBit();
				//DIMTOLJ RC 283
				dimStyle.ToleranceAlignment = (ToleranceAlignment)this._objectReader.ReadRawChar();
				//DIMJUST RC 280
				dimStyle.TextHorizontalAlignment = (DimensionTextHorizontalAlignment)this._objectReader.ReadByte();
				//DIMFIT RC 287
				dimStyle.DimensionFit = this._objectReader.ReadRawChar();
				//DIMUPT B 288
				dimStyle.CursorUpdate = this._objectReader.ReadBit();
				//DIMTZIN RC 284
				dimStyle.ToleranceZeroHandling = (ZeroHandling)this._objectReader.ReadByte();
				//DIMALTZ RC 285
				dimStyle.AlternateUnitZeroHandling = (ZeroHandling)this._objectReader.ReadByte();
				//DIMALTTZ RC 286
				dimStyle.AlternateUnitToleranceZeroHandling = (ZeroHandling)this._objectReader.ReadByte();
				//DIMTAD RC 77
				dimStyle.TextVerticalAlignment = (DimensionTextVerticalAlignment)this._objectReader.ReadByte();
				//DIMUNIT BS 270
				dimStyle.DimensionUnit = this._objectReader.ReadBitShort();
				//DIMAUNIT BS 275
				dimStyle.AngularUnit = (AngularUnitFormat)this._objectReader.ReadBitShort();
				//DIMDEC BS 271
				dimStyle.DecimalPlaces = this._objectReader.ReadBitShort();
				//DIMTDEC BS 272
				dimStyle.ToleranceDecimalPlaces = this._objectReader.ReadBitShort();
				//DIMALTU BS 273
				dimStyle.AlternateUnitFormat = (LinearUnitFormat)this._objectReader.ReadBitShort();
				//DIMALTTD BS 274
				dimStyle.AlternateUnitToleranceDecimalPlaces = this._objectReader.ReadBitShort();
				//DIMSCALE BD 40
				dimStyle.ScaleFactor = this._objectReader.ReadBitDouble();
				//DIMASZ BD 41
				dimStyle.ArrowSize = this._objectReader.ReadBitDouble();
				//DIMEXO BD 42
				dimStyle.ExtensionLineOffset = this._objectReader.ReadBitDouble();
				//DIMDLI BD 43
				dimStyle.DimensionLineIncrement = this._objectReader.ReadBitDouble();
				//DIMEXE BD 44
				dimStyle.ExtensionLineExtension = this._objectReader.ReadBitDouble();
				//DIMRND BD 45
				dimStyle.Rounding = this._objectReader.ReadBitDouble();
				//DIMDLE BD 46
				dimStyle.DimensionLineExtension = this._objectReader.ReadBitDouble();
				//DIMTP BD 47
				dimStyle.PlusTolerance = this._objectReader.ReadBitDouble();
				//DIMTM BD 48
				dimStyle.MinusTolerance = this._objectReader.ReadBitDouble();
				//DIMTXT BD 140
				dimStyle.TextHeight = this._objectReader.ReadBitDouble();
				//DIMCEN BD 141
				dimStyle.CenterMarkSize = this._objectReader.ReadBitDouble();
				//DIMTSZ BD 142
				dimStyle.TickSize = this._objectReader.ReadBitDouble();
				//DIMALTF BD 143
				dimStyle.AlternateUnitScaleFactor = this._objectReader.ReadBitDouble();
				//DIMLFAC BD 144
				dimStyle.LinearScaleFactor = this._objectReader.ReadBitDouble();
				//DIMTVP BD 145
				dimStyle.TextVerticalPosition = this._objectReader.ReadBitDouble();
				//DIMTFAC BD 146
				dimStyle.ToleranceScaleFactor = this._objectReader.ReadBitDouble();
				//DIMGAP BD 147
				dimStyle.DimensionLineGap = this._objectReader.ReadBitDouble();
				//DIMPOST T 3
				dimStyle.PostFix = this._textReader.ReadVariableText();
				//DIMAPOST T 4
				dimStyle.AlternateDimensioningSuffix = this._textReader.ReadVariableText();

				//DIMBLK T 5
				template.DIMBL_Name = this._textReader.ReadVariableText();
				//DIMBLK1 T 6
				template.DIMBLK1_Name = this._textReader.ReadVariableText();
				//DIMBLK2 T 7
				template.DIMBLK2_Name = this._textReader.ReadVariableText();

				//DIMCLRD BS 176
				dimStyle.DimensionLineColor = this._objectReader.ReadColorByIndex();
				//DIMCLRE BS 177
				dimStyle.ExtensionLineColor = this._objectReader.ReadColorByIndex();
				//DIMCLRT BS 178
				dimStyle.TextColor = this._objectReader.ReadColorByIndex();
			}

			//R2000+:
			if (this.R2000Plus)
			{
				//DIMPOST TV 3
				dimStyle.PostFix = this._textReader.ReadVariableText();
				//DIMAPOST TV 4
				dimStyle.AlternateDimensioningSuffix = this._textReader.ReadVariableText();
				//DIMSCALE BD 40
				dimStyle.ScaleFactor = this._objectReader.ReadBitDouble();
				//DIMASZ BD 41
				dimStyle.ArrowSize = this._objectReader.ReadBitDouble();
				//DIMEXO BD 42
				dimStyle.ExtensionLineOffset = this._objectReader.ReadBitDouble();
				//DIMDLI BD 43
				dimStyle.DimensionLineIncrement = this._objectReader.ReadBitDouble();
				//DIMEXE BD 44
				dimStyle.ExtensionLineExtension = this._objectReader.ReadBitDouble();
				//DIMRND BD 45
				dimStyle.Rounding = this._objectReader.ReadBitDouble();
				//DIMDLE BD 46
				dimStyle.DimensionLineExtension = this._objectReader.ReadBitDouble();
				//DIMTP BD 47
				dimStyle.PlusTolerance = this._objectReader.ReadBitDouble();
				//DIMTM BD 48
				dimStyle.MinusTolerance = this._objectReader.ReadBitDouble();
			}

			//R2007+:
			if (this.R2007Plus)
			{
				//DIMFXL BD 49
				dimStyle.FixedExtensionLineLength = this._objectReader.ReadBitDouble();
				//DIMJOGANG BD 50
				dimStyle.JoggedRadiusDimensionTransverseSegmentAngle = this._objectReader.ReadBitDouble();
				//DIMTFILL BS 69
				dimStyle.TextBackgroundFillMode = (DimensionTextBackgroundFillMode)this._objectReader.ReadBitShort();
				//DIMTFILLCLR CMC 70
				dimStyle.TextBackgroundColor = this._mergedReaders.ReadCmColor();
			}

			//R2000+:
			if (this.R2000Plus)
			{
				//DIMTOL B 71
				dimStyle.GenerateTolerances = this._objectReader.ReadBit();
				//DIMLIM B 72
				dimStyle.LimitsGeneration = this._objectReader.ReadBit();
				//DIMTIH B 73
				dimStyle.TextInsideHorizontal = this._objectReader.ReadBit();
				//DIMTOH B 74
				dimStyle.TextOutsideHorizontal = this._objectReader.ReadBit();
				//DIMSE1 B 75
				dimStyle.SuppressFirstExtensionLine = this._objectReader.ReadBit();
				//DIMSE2 B 76
				dimStyle.SuppressSecondExtensionLine = this._objectReader.ReadBit();
				//DIMTAD BS 77
				dimStyle.TextVerticalAlignment = (DimensionTextVerticalAlignment)this._objectReader.ReadBitShort();
				//DIMZIN BS 78
				dimStyle.ZeroHandling = (ZeroHandling)this._objectReader.ReadBitShort();
				//DIMAZIN BS 79
				dimStyle.AngularZeroHandling = (ZeroHandling)this._objectReader.ReadBitShort();
			}

			//R2007 +:
			if (this.R2007Plus)
				//DIMARCSYM BS 90
				dimStyle.ArcLengthSymbolPosition = (ArcLengthSymbolPosition)this._objectReader.ReadBitShort();

			//R2000 +:
			if (this.R2000Plus)
			{
				//DIMTXT BD 140
				dimStyle.TextHeight = this._objectReader.ReadBitDouble();
				//DIMCEN BD 141
				dimStyle.CenterMarkSize = this._objectReader.ReadBitDouble();
				//DIMTSZ BD 142
				dimStyle.TickSize = this._objectReader.ReadBitDouble();
				//DIMALTF BD 143
				dimStyle.AlternateUnitScaleFactor = this._objectReader.ReadBitDouble();
				//DIMLFAC BD 144
				dimStyle.LinearScaleFactor = this._objectReader.ReadBitDouble();
				//DIMTVP BD 145
				dimStyle.TextVerticalPosition = this._objectReader.ReadBitDouble();
				//DIMTFAC BD 146
				dimStyle.ToleranceScaleFactor = this._objectReader.ReadBitDouble();
				//DIMGAP BD 147
				dimStyle.DimensionLineGap = this._objectReader.ReadBitDouble();
				//DIMALTRND BD 148
				dimStyle.AlternateUnitRounding = this._objectReader.ReadBitDouble();
				//DIMALT B 170
				dimStyle.AlternateUnitDimensioning = this._objectReader.ReadBit();
				//DIMALTD BS 171
				dimStyle.AlternateUnitDecimalPlaces = this._objectReader.ReadBitShort();
				//DIMTOFL B 172
				dimStyle.TextOutsideExtensions = this._objectReader.ReadBit();
				//DIMSAH B 173
				dimStyle.SeparateArrowBlocks = this._objectReader.ReadBit();
				//DIMTIX B 174
				dimStyle.TextInsideExtensions = this._objectReader.ReadBit();
				//DIMSOXD B 175
				dimStyle.SuppressOutsideExtensions = this._objectReader.ReadBit();
				//DIMCLRD BS 176
				dimStyle.DimensionLineColor = this._mergedReaders.ReadCmColor();
				//DIMCLRE BS 177
				dimStyle.ExtensionLineColor = this._mergedReaders.ReadCmColor();
				//DIMCLRT BS 178
				dimStyle.TextColor = this._mergedReaders.ReadCmColor();
				//DIMADEC BS 179
				dimStyle.AngularDimensionDecimalPlaces = this._objectReader.ReadBitShort();
				//DIMDEC BS 271
				dimStyle.DecimalPlaces = this._objectReader.ReadBitShort();
				//DIMTDEC BS 272
				dimStyle.ToleranceDecimalPlaces = this._objectReader.ReadBitShort();
				//DIMALTU BS 273
				dimStyle.AlternateUnitFormat = (LinearUnitFormat)this._objectReader.ReadBitShort();
				//DIMALTTD BS 274
				dimStyle.AlternateUnitToleranceDecimalPlaces = this._objectReader.ReadBitShort();
				//DIMAUNIT BS 275
				dimStyle.AngularUnit = (AngularUnitFormat)this._objectReader.ReadBitShort();
				//DIMFRAC BS 276
				dimStyle.FractionFormat = (FractionFormat)this._objectReader.ReadBitShort();
				//DIMLUNIT BS 277
				dimStyle.LinearUnitFormat = (LinearUnitFormat)this._objectReader.ReadBitShort();
				//DIMDSEP BS 278
				dimStyle.DecimalSeparator = (char)this._objectReader.ReadBitShort();
				//DIMTMOVE BS 279
				dimStyle.TextMovement = (TextMovement)this._objectReader.ReadBitShort();
				//DIMJUST BS 280
				dimStyle.TextHorizontalAlignment = (DimensionTextHorizontalAlignment)this._objectReader.ReadBitShort();
				//DIMSD1 B 281
				dimStyle.SuppressFirstDimensionLine = this._objectReader.ReadBit();
				//DIMSD2 B 282
				dimStyle.SuppressSecondDimensionLine = this._objectReader.ReadBit();
				//DIMTOLJ BS 283
				dimStyle.ToleranceAlignment = (ToleranceAlignment)this._objectReader.ReadBitShort();
				//DIMTZIN BS 284
				dimStyle.ToleranceZeroHandling = (ZeroHandling)this._objectReader.ReadBitShort();
				//DIMALTZ BS 285
				dimStyle.AlternateUnitZeroHandling = (ZeroHandling)this._objectReader.ReadBitShort();
				//DIMALTTZ BS 286
				dimStyle.AlternateUnitToleranceZeroHandling = (ZeroHandling)this._objectReader.ReadBitShort();
				//DIMUPT B 288
				dimStyle.CursorUpdate = this._objectReader.ReadBit();
				//DIMFIT BS 287
				this._objectReader.ReadBitShort();
			}

			//R2007+:
			if (this.R2007Plus)
				//DIMFXLON B 290
				dimStyle.IsExtensionLineLengthFixed = this._objectReader.ReadBit();

			//R2010+:
			if (this.R2010Plus)
			{
				//DIMTXTDIRECTION B 295
				dimStyle.TextDirection = this._objectReader.ReadBit() ? TextDirection.RightToLeft : TextDirection.LeftToRight;
				//DIMALTMZF BD ?
				dimStyle.AltMzf = this._objectReader.ReadBitDouble();
				//DIMALTMZS T ?
				dimStyle.AltMzs = this._textReader.ReadVariableText();
				//DIMMZF BD ?
				dimStyle.Mzf = this._objectReader.ReadBitDouble();
				//DIMMZS T ?
				dimStyle.Mzs = this._textReader.ReadVariableText();
			}

			//R2000+:
			if (this.R2000Plus)
			{
				//DIMLWD BS 371
				dimStyle.DimensionLineWeight = (LineweightType)this._objectReader.ReadBitShort();
				//DIMLWE BS 372
				dimStyle.ExtensionLineWeight = (LineweightType)this._objectReader.ReadBitShort();
			}

			//Common:
			//Unknown B 70 Seems to set the 0 - bit(1) of the 70 - group.
			this._objectReader.ReadBit();

			//Handle refs H Dimstyle control(soft pointer)
			//[Reactors(soft pointer)]
			//xdicobjhandle(hard owner)
			//External reference block handle(hard pointer)
			long block = (long)this.handleReference();
			//340 shapefile(DIMTXSTY)(hard pointer)
			template.DIMTXSTY = this.handleReference();

			//R2000+:
			if (this.R2000Plus)
			{
				//341 leader block(DIMLDRBLK) (hard pointer)
				template.DIMLDRBLK = this.handleReference();
				//342 dimblk(DIMBLK)(hard pointer)
				template.DIMBLK = this.handleReference();
				//343 dimblk1(DIMBLK1)(hard pointer)
				template.DIMBLK1 = this.handleReference();
				//344 dimblk2(DIMBLK2)(hard pointer)
				template.DIMBLK2 = this.handleReference();
			}

			//R2007+:
			if (this.R2007Plus)
			{
				//345 dimltype(hard pointer)
				template.Dimltype = this.handleReference();
				//346 dimltex1(hard pointer)
				template.Dimltex1 = this.handleReference();
				//347 dimltex2(hard pointer)
				template.Dimltex2 = this.handleReference();
			}

			return template;
		}

		private CadTemplate readViewportEntityControl()
		{
			return null;

			DwgViewportEntityControlTemplate template = new DwgViewportEntityControlTemplate(
				this._builder.DocumentToBuild.Viewports);

			this.readCommonNonEntityData(template);

			//Common:
			//Numentries BL 70
			int numentries = this._objectReader.ReadBitLong();
			for (int i = 0; i < numentries; ++i)
				//Handle refs H NULL(soft pointer)	xdicobjhandle(hard owner)	the apps(soft owner)
				template.EntryHandles.Add(this.handleReference());

			return template;
		}

		private CadTemplate readViewportEntityHeader()
		{
			//Viewport viewport = new Viewport();
			//DwgViewportTemplate template = new DwgViewportTemplate(viewport);

			//this.readCommonNonEntityData(template);

			////Common:
			////Entry name TV 2
			//viewport.StyleSheetName = this._textReader.ReadVariableText();

			//this.readXrefDependantBit(viewport);

			////1 flag B The 1 bit of the 70 group
			//this._objectReader.ReadBit();

			////Handle refs H viewport entity control (soft pointer)
			//this.handleReference();
			////xdicobjhandle (hard owner)
			//this.handleReference();
			////External reference block handle (hard pointer)
			//this.handleReference();

			//TODO: transform the viewport ent into the viewport

			return null;
		}

		private CadTemplate readGroup()
		{
			Group group = new Group();
			DwgGroupTemplate template = new DwgGroupTemplate(group);

			this.readCommonNonEntityData(template);

			//Str TV name of group
			group.Description = this._textReader.ReadVariableText();

			//Unnamed BS 1 if group has no name
			this._objectReader.ReadBitShort();
			//Selectable BS 1 if group selectable
			group.Selectable = this._objectReader.ReadBitShort() > 0;

			//Numhandles BL # objhandles in this group
			int numhandles = this._objectReader.ReadBitLong();
			for (int index = 0; index < numhandles; ++index)
				//Handle refs H parenthandle (soft pointer)
				template.Handles.Add(this.handleReference());

			return template;
		}

		private CadTemplate readMLStyle()
		{
			MLStyle mlineStyle = new MLStyle();
			CadMLStyleTemplate template = new CadMLStyleTemplate(mlineStyle);

			this.readCommonNonEntityData(template);

			//Common:
			//Name TV Name of this style
			mlineStyle.Name = this._textReader.ReadVariableText();
			//Desc TV Description of this style
			mlineStyle.Description = this._textReader.ReadVariableText();
			//Flags BS A short which reconstitutes the mlinestyle flags as defined in DXF.
			//Here are the bits as they relate to DXF:
			/*
			 DWG bit goes with DXF bit
				1				2
				2				1
				16				16
				32				64
				64				32
				256				256
				512				1024
				1024			512
			 */
			short flags = this._objectReader.ReadBitShort();
			if (((uint)flags & 1U) > 0U)
				mlineStyle.Flags |= MLineStyleFlags.DisplayJoints;
			if (((uint)flags & 2U) > 0U)
				mlineStyle.Flags |= MLineStyleFlags.FillOn;
			if (((uint)flags & 16U) > 0U)
				mlineStyle.Flags |= MLineStyleFlags.StartSquareCap;
			if (((uint)flags & 32U) > 0U)
				mlineStyle.Flags |= MLineStyleFlags.StartRoundCap;
			if (((uint)flags & 64U) > 0U)
				mlineStyle.Flags |= MLineStyleFlags.StartInnerArcsCap;
			if (((uint)flags & 256U) > 0U)
				mlineStyle.Flags |= MLineStyleFlags.EndSquareCap;
			if (((uint)flags & 512U) > 0U)
				mlineStyle.Flags |= MLineStyleFlags.EndRoundCap;
			if (((uint)flags & 1024U) > 0U)
				mlineStyle.Flags |= MLineStyleFlags.EndInnerArcsCap;

			//fillcolor CMC Fill color for this style
			mlineStyle.FillColor = this._mergedReaders.ReadCmColor();
			//startang BD Start angle
			mlineStyle.StartAngle = this._objectReader.ReadBitDouble();
			//endang BD End angle
			mlineStyle.EndAngle = this._objectReader.ReadBitDouble();

			//linesinstyle RC Number of lines in this style
			int nlines = this._objectReader.ReadByte();
			for (int i = 0; i < nlines; ++i)
			{
				MLStyle.Element element = new MLStyle.Element();
				CadMLStyleTemplate.ElementTemplate elementTemplate = new CadMLStyleTemplate.ElementTemplate(element);

				//Offset BD Offset of this segment
				element.Offset = this._objectReader.ReadBitDouble();
				//Color CMC Color of this segment
				element.Color = this._mergedReaders.ReadCmColor();

				//R2018+:
				if (this.R2018Plus)
					//Line type handle H Line type handle (hard pointer)
					elementTemplate.LinetypeHandle = this.handleReference();
				//Before R2018:
				else
					//Ltindex BS Linetype index (yes, index)
					elementTemplate.LinetypeIndex = this._objectReader.ReadBitShort();

				template.ElementTemplates.Add(elementTemplate);
				mlineStyle.Elements.Add(element);
			}

			return template;
		}

		private CadTemplate readLWPolyline()
		{
			LwPolyline lwPolyline = new LwPolyline();
			CadEntityTemplate template = new CadEntityTemplate(lwPolyline);

			try
			{
				this.readCommonEntityData(template);

				//B : bytes containing the LWPOLYLINE entity data.
				//This excludes the common entity data.
				//More specifically: it starts at the LWPOLYLINE flags (BS), and ends with the width array (BD).

				short flags = this._objectReader.ReadBitShort();
				if ((flags & 0x100) != 0)
					lwPolyline.Flags |= LwPolylineFlags.Plinegen;
				if ((flags & 0x200) != 0)
					lwPolyline.Flags |= LwPolylineFlags.Closed;

				if ((flags & 4u) != 0)
				{
					lwPolyline.ConstantWidth = this._objectReader.ReadBitDouble();
				}

				if ((flags & 8u) != 0)
				{
					lwPolyline.Elevation = this._objectReader.ReadBitDouble();
				}

				if ((flags & 2u) != 0)
				{
					lwPolyline.Thickness = this._objectReader.ReadBitDouble();
				}

				if ((flags & (true ? 1u : 0u)) != 0)
				{
					lwPolyline.Normal = this._objectReader.Read3BitDouble();
				}

				int nvertices = this._objectReader.ReadBitLong();
				int nbulges = 0;

				if (((uint)flags & 0x10u) != 0)
				{
					nbulges = this._objectReader.ReadBitLong();
				}
				int nids = 0;
				if (((uint)flags & 0x400u) != 0)
				{
					nids = this._objectReader.ReadBitLong();
				}

				int ndiffwidth = 0;
				if (((uint)flags & 0x20u) != 0)
				{
					ndiffwidth = this._objectReader.ReadBitLong();
				}

				if (this.R13_14Only)
				{
					for (int i = 0; i < nvertices; i++)
					{
						Vertex2D v = new Vertex2D();
						XY loc = this._objectReader.Read2RawDouble();
						lwPolyline.Vertices.Add(new LwPolyline.Vertex(loc));
					}
				}

				if (this.R2000Plus && nvertices > 0)
				{
					XY loc = this._objectReader.Read2RawDouble();
					lwPolyline.Vertices.Add(new LwPolyline.Vertex(loc));
					for (int j = 1; j < nvertices; j++)
					{
						loc = this._objectReader.Read2BitDoubleWithDefault(loc);
						lwPolyline.Vertices.Add(new LwPolyline.Vertex(loc));
					}
				}

				for (int k = 0; k < nbulges; k++)
				{
					lwPolyline.Vertices[k].Bulge = this._objectReader.ReadBitDouble();
				}

				for (int l = 0; l < nids; l++)
				{
					lwPolyline.Vertices[l].Id = this._objectReader.ReadBitLong();
				}

				for (int m = 0; m < ndiffwidth; m++)
				{
					LwPolyline.Vertex vertex = lwPolyline.Vertices[m];
					vertex.StartWidth = this._objectReader.ReadBitDouble();
					vertex.EndWidth = this._objectReader.ReadBitDouble();
				}
			}
			catch (System.Exception ex)
			{
				this._builder.Notify(new NotificationEventArgs($"Exception while reading LwPolyline: {ex.GetType().FullName}"));
				return template;
			}

			return template;
		}

		private CadTemplate readHatch()
		{
			Hatch hatch = new Hatch();
			CadHatchTemplate template = new CadHatchTemplate(hatch);

			this.readCommonEntityData(template);

			//R2004+:
			if (this.R2004Plus)
			{
				//Is Gradient Fill BL 450 Non-zero indicates a gradient fill is used.
				hatch.GradientColor.Enabled = this._objectReader.ReadBitLong() != 0;

				//Reserved BL 451
				hatch.GradientColor.Reserved = this._objectReader.ReadBitLong();
				//Gradient Angle BD 460
				hatch.GradientColor.Angle = this._objectReader.ReadBitDouble();
				//Gradient Shift BD 461
				hatch.GradientColor.Shift = this._objectReader.ReadBitDouble();
				//Single Color Grad.BL 452
				hatch.GradientColor.IsSingleColorGradient = (uint)this._objectReader.ReadBitLong() > 0U;
				//Gradient Tint BD 462
				hatch.GradientColor.ColorTint = this._objectReader.ReadBitDouble();

				//# of Gradient Colors BL 453
				int ncolors = this._objectReader.ReadBitLong();
				for (int i = 0; i < ncolors; ++i)
				{
					GradientColor color = new GradientColor();

					//Gradient Value double BD 463
					color.Value = this._objectReader.ReadBitDouble();
					//RGB Color
					color.Color = this._mergedReaders.ReadCmColor();

					hatch.GradientColor.Colors.Add(color);
				}

				//Gradient Name TV 470
				hatch.GradientColor.Name = this._textReader.ReadVariableText();
			}

			//Common:
			//Z coord BD 30 X, Y always 0.0
			hatch.Elevation = this._objectReader.ReadBitDouble();
			//Extrusion 3BD 210
			hatch.Normal = this._objectReader.Read3BitDouble();
			//Name TV 2 name of hatch
			hatch.Pattern = new HatchPattern(this._textReader.ReadVariableText());
			//Solidfill B 70 1 if solidfill, else 0
			hatch.IsSolid = this._objectReader.ReadBit();
			//Associative B 71 1 if associative, else 0
			hatch.IsAssociative = this._objectReader.ReadBit();

			//Numpaths BL 91 Number of paths enclosing the hatch
			int npaths = this._objectReader.ReadBitLong();
			bool hasDerivedBoundary = false;

			#region Read the boundary path data

			for (int i = 0; i < npaths; i++)
			{
				CadHatchTemplate.CadBoundaryPathTemplate pathTemplate = new CadHatchTemplate.CadBoundaryPathTemplate();

				//Pathflag BL 92 Path flag
				pathTemplate.Path.Flags = (BoundaryPathFlags)this._objectReader.ReadBitLong();

				if (pathTemplate.Path.Flags.HasFlag(BoundaryPathFlags.Derived))
					hasDerivedBoundary = true;

				if (!pathTemplate.Path.Flags.HasFlag(BoundaryPathFlags.Polyline))
				{
					//Numpathsegs BL 93 number of segments in this path
					int nsegments = this._objectReader.ReadBitLong();
					for (int j = 0; j < nsegments; ++j)
					{
						//pathtypestatus RC 72 type of path
						Hatch.BoundaryPath.EdgeType pathTypeStatus = (Hatch.BoundaryPath.EdgeType)this._objectReader.ReadByte();
						switch (pathTypeStatus)
						{
							case Hatch.BoundaryPath.EdgeType.Line:
								pathTemplate.Path.Edges.Add(new Hatch.BoundaryPath.Line
								{
									//pt0 2RD 10 first endpoint
									Start = this._objectReader.Read2RawDouble(),
									//pt1 2RD 11 second endpoint
									End = this._objectReader.Read2RawDouble()
								});
								break;
							case Hatch.BoundaryPath.EdgeType.CircularArc:
								pathTemplate.Path.Edges.Add(new Hatch.BoundaryPath.Arc
								{
									//pt0 2RD 10 center
									Center = this._objectReader.Read2RawDouble(),
									//radius BD 40 radius
									Radius = this._objectReader.ReadBitDouble(),
									//startangle BD 50 start angle
									StartAngle = this._objectReader.ReadBitDouble(),
									//endangle BD 51 endangle
									EndAngle = this._objectReader.ReadBitDouble(),
									//isccw B 73 1 if counter clockwise, otherwise 0
									CounterClockWise = this._objectReader.ReadBit()
								});
								break;
							case Hatch.BoundaryPath.EdgeType.EllipticArc:
								pathTemplate.Path.Edges.Add(new Hatch.BoundaryPath.Ellipse
								{
									//pt0 2RD 10 center
									Center = this._objectReader.Read2RawDouble(),
									//endpoint 2RD 11 endpoint of major axis
									MajorAxisEndPoint = this._objectReader.Read2RawDouble(),
									//minormajoratio BD 40 ratio of minor to major axis
									Radius = this._objectReader.ReadBitDouble(),
									//startangle BD 50 start angle
									StartAngle = this._objectReader.ReadBitDouble(),
									//endangle BD 51 endangle
									EndAngle = this._objectReader.ReadBitDouble(),
									//isccw B 73 1 if counter clockwise, otherwise 0
									CounterClockWise = this._objectReader.ReadBit()
								});
								break;
							case Hatch.BoundaryPath.EdgeType.Spline:
								Hatch.BoundaryPath.Spline splineEdge = new Hatch.BoundaryPath.Spline();

								//degree BL 94 degree of the spline
								splineEdge.Degree = this._objectReader.ReadBitLong();
								//isrational B 73 1 if rational(has weights), else 0
								splineEdge.Rational = this._objectReader.ReadBit();
								//isperiodic B 74 1 if periodic, else 0
								splineEdge.Periodic = this._objectReader.ReadBit();

								//numknots BL 95 number of knots
								int numknots = this._objectReader.ReadBitLong();
								//numctlpts BL 96 number of control points
								int numctlpts = this._objectReader.ReadBitLong();

								for (int k = 0; k < numknots; ++k)
									//knot BD 40 knot value
									splineEdge.Knots.Add(this._objectReader.ReadBitDouble());

								for (int p = 0; p < numctlpts; ++p)
								{
									//pt0 2RD 10 control point
									var cp = this._objectReader.Read2RawDouble();

									double wheight = 0;
									if (splineEdge.Rational)
										//weight BD 40 weight
										wheight = this._objectReader.ReadBitDouble();

									//Add the control point and its wheight
									splineEdge.ControlPoints.Add(new XYZ(cp.X, cp.Y, wheight));
								}

								//R24:
								if (this.R2010Plus)
								{
									//Numfitpoints BL 97 number of fit points
									int nfitPoints = this._objectReader.ReadBitLong();
									if (nfitPoints > 0)
									{
										for (int fp = 0; fp < nfitPoints; ++fp)
										{
											//Fitpoint 2RD 11
											splineEdge.FitPoints.Add(this._objectReader.Read2RawDouble());
										}

										//Start tangent 2RD 12
										splineEdge.StartTangent = this._objectReader.Read2RawDouble();
										//End tangent 2RD 13
										splineEdge.EndTangent = this._objectReader.Read2RawDouble();
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
					Hatch.BoundaryPath.Polyline pline = new Hatch.BoundaryPath.Polyline();
					//bulgespresent B 72 bulges are present if 1
					bool bulgespresent = this._objectReader.ReadBit();
					//closed B 73 1 if closed
					pline.IsClosed = this._objectReader.ReadBit();

					//numpathsegs BL 91 number of path segments
					int numpathsegs = this._objectReader.ReadBitLong();
					for (int index = 0; index < numpathsegs; ++index)
					{
						//pt0 2RD 10 point on polyline
						XY vertex = this._objectReader.Read2RawDouble();

						if (bulgespresent)
							//bulge BD 42 bulge
							pline.Bulge = this._objectReader.ReadBitDouble();

						//Add the vertex
						pline.Vertices.Add(new XY(vertex.X, vertex.Y));
					}
				}

				//numboundaryobjhandles BL 97 Number of boundary object handles for this path
				int numboundaryobjhandles = this._objectReader.ReadBitLong();

				for (int h = 0; h < numboundaryobjhandles; h++)
				{
					//boundaryhandle H 330 boundary handle(soft pointer)
					pathTemplate.Handles.Add(this.handleReference());
				}

				template.PathTempaltes.Add(pathTemplate);
			}

			#endregion Read the boundary path data

			//style BS 75 style of hatch 0==odd parity, 1==outermost, 2==whole area
			hatch.HatchStyle = (HatchStyleType)this._objectReader.ReadBitShort();
			//patterntype BS 76 pattern type 0==user-defined, 1==predefined, 2==custom
			hatch.HatchPatternType = (HatchPatternType)this._objectReader.ReadBitShort();

			if (!hatch.IsSolid)
			{
				//angle BD 52 hatch angle
				hatch.PatternAngle = this._objectReader.ReadBitDouble();
				//scaleorspacing BD 41 scale or spacing(pattern fill only)
				hatch.PatternScale = this._objectReader.ReadBitDouble();
				//doublehatch B 77 1 for double hatch
				hatch.IsDouble = this._objectReader.ReadBit();

				//numdeflines BS 78 number of definition lines
				int numdeflines = this._objectReader.ReadBitShort();
				for (int li = 0; li < numdeflines; ++li)
				{
					HatchPattern.Line line = new HatchPattern.Line();
					//angle BD 53 line angle
					line.Angle = this._objectReader.ReadBitDouble();
					//pt0 2BD 43 / 44 pattern through this point(X, Y)
					line.BasePoint = this._objectReader.Read2BitDouble();
					//offset 2BD 45 / 56 pattern line offset
					line.Offset = this._objectReader.Read2BitDouble();

					//  numdashes BS 79 number of dash length items
					int ndashes = this._objectReader.ReadBitShort();
					for (int ds = 0; ds < ndashes; ++ds)
					{
						//dashlength BD 49 dash length
						line.DashLengths.Add(this._objectReader.ReadBitDouble());
					}

					hatch.Pattern.Lines.Add(line);
				}
			}

			if (hasDerivedBoundary)
				//pixelsize BD 47 pixel size
				hatch.PixelSize = this._objectReader.ReadBitDouble();

			//numseedpoints BL 98 number of seed points
			int numseedpoints = this._objectReader.ReadBitLong();
			for (int sp = 0; sp < numseedpoints; ++sp)
			{
				//pt0 2RD 10 seed point
				XY spt = this._objectReader.Read2RawDouble();
				hatch.SeedPoints.Add(spt);
			}

			return template;
		}

		private CadTemplate readXRecord()
		{
			XRecrod xRecord = new XRecrod();
			CadXRecordTemplate template = new CadXRecordTemplate(xRecord);

			this.readCommonNonEntityData(template);

			//Common:
			//Numdatabytes BL number of databytes
			long offset = this._objectReader.ReadBitLong();

			//Databytes X databytes, however many there are to the handles
			while (this._objectReader.Position < offset)
			{
				//Common:
				//XRECORD data is pairs of:
				//RS indicator number, then data. The indicator number indicates the DXF number of the data,
				//then the data follows, so for instance an indicator of 1 would be followed by the string length (RC),
				//the dwgcodepage (RC), and then the string, for R13-R2004 files. For R2007+,
				//a string contains a short length N, and then N Unicode characters (2 bytes each).
				//An indicator of 70 would mean a 2 byte short following. An indicator of 10 indicates
				//3 8-byte doubles following. An indicator of 40 means 1 8-byte double. These indicator
				//numbers all follow the normal AutoCAD DXF convention for group codes.
				var code = this._objectReader.ReadShort();
				var groupCode = GroupCodeValue.TransformValue(code);

				switch (groupCode)
				{
					case GroupCodeValueType.None:
						break;
					case GroupCodeValueType.String:
						xRecord.Entries.Add(new XRecrod.Entry(code, this._objectReader.ReadTextUnicode()));
						break;
					case GroupCodeValueType.Point3D:
						xRecord.Entries.Add(new XRecrod.Entry(code,
							new XYZ(
								this._objectReader.ReadDouble(),
								this._objectReader.ReadDouble(),
								this._objectReader.ReadDouble()
								)));
						break;
					case GroupCodeValueType.Double:
						xRecord.Entries.Add(new XRecrod.Entry(code, this._objectReader.ReadDouble()));
						break;
					case GroupCodeValueType.Int16:
						xRecord.Entries.Add(new XRecrod.Entry(code, this._objectReader.ReadShort()));
						break;
					case GroupCodeValueType.Int32:
						xRecord.Entries.Add(new XRecrod.Entry(code, this._objectReader.ReadRawLong()));
						break;
					case GroupCodeValueType.Int64:
						xRecord.Entries.Add(new XRecrod.Entry(code, this._objectReader.ReadRawLong()));
						break;
					case GroupCodeValueType.Handle:
						xRecord.Entries.Add(new XRecrod.Entry(code, this._objectReader.ReadTextUnicode()));
						break;
					case GroupCodeValueType.Bool:
						xRecord.Entries.Add(new XRecrod.Entry(code, this._objectReader.ReadByte() > 0));
						break;
					case GroupCodeValueType.Chunk:
						xRecord.Entries.Add(new XRecrod.Entry(code, this._objectReader.ReadBytes(this._objectReader.ReadByte())));
						break;
					default:
						break;
				}
			}

			//R2000+:
			if (this.R2000Plus)
				//Cloning flag BS 280
				xRecord.ClonningFlags = (DictionaryCloningFlags)this._objectReader.ReadBitShort();

			long size = this._objectInitialPos + (long)(this._size * 8U) - 7L;
			while (this._handlesReader.PositionInBits() < size)
			{
				//Handle refs H parenthandle (soft pointer)
				//[Reactors(soft pointer)]
				//xdictionary(hard owner)
				//objid object handles, as many as you can read until you run out of data
				this.handleReference();
			}

			return template;
		}

		private CadTemplate readLayout()
		{
			Layout layout = new Layout();
			CadLayoutTemplate template = new CadLayoutTemplate(layout);

			this.readCommonNonEntityData(template);

			this.readPlotSettings(layout);

			//Common:
			//Layout name TV 1 layout name
			layout.Name = this._textReader.ReadVariableText();
			//Tab order BL 71 layout tab order
			layout.TabOrder = this._objectReader.ReadBitLong();
			//Flag BS 70 layout flags
			layout.Flags = (LayoutFlags)this._objectReader.ReadBitShort();
			//Ucs origin 3BD 13 layout ucs origin
			layout.Origin = this._objectReader.Read3BitDouble();
			//Limmin 2RD 10 layout minimum limits
			layout.MinLimits = this._objectReader.Read2RawDouble();
			//Limmax 2RD 11 layout maximum limits
			layout.MaxLimits = this._objectReader.Read2RawDouble();
			//Inspoint 3BD 12 layout insertion base point
			layout.InsertionBasePoint = this._objectReader.Read3BitDouble();
			//Ucs x axis 3BD 16 layout ucs x axis direction
			layout.XAxis = this._objectReader.Read3BitDouble();
			//Ucs y axis 3BD 17 layout ucs y axis direction
			layout.YAxis = this._objectReader.Read3BitDouble();
			//Elevation BD 146 layout elevation
			layout.Elevation = this._objectReader.ReadBitDouble();
			//Orthoview type BS 76 layout orthographic view type of UCS
			layout.UcsOrthographicType = (OrthographicType)this._objectReader.ReadBitShort();
			//Extmin 3BD 14 layout extent min
			layout.MinExtents = this._objectReader.Read3BitDouble();
			//Extmax 3BD 15 layout extent max
			layout.MaxExtents = this._objectReader.Read3BitDouble();

			int nLayouts = 0;
			//R2004 +:
			if (this.R2004Plus)
				//Viewport count RL # of viewports in this layout
				nLayouts = this._objectReader.ReadBitLong();

			//Common:
			//330 associated paperspace block record handle(soft pointer)
			template.PaperSpaceBlockHandle = this.handleReference();
			//331 last active viewport handle(soft pointer)
			template.ActiveViewportHandle = this.handleReference();
			//346 base ucs handle(hard pointer)
			template.BaseUcsHandle = this.handleReference();
			//345 named ucs handle(hard pointer)
			template.NamesUcsHandle = this.handleReference();

			//R2004+:
			if (this.R2004Plus)
			{
				//Viewport handle(repeats Viewport count times) (soft pointer)
				for (int i = 0; i < nLayouts; ++i)
					template.ViewportHandles.Add(this.handleReference());
			}

			return template;
		}

		private void readPlotSettings(PlotSettings plot)
		{
			//Common:
			//Page setup name TV 1 plotsettings page setup name
			plot.PageName = this._textReader.ReadVariableText();
			//Printer / Config TV 2 plotsettings printer or configuration file
			plot.SystemPrinterName = this._textReader.ReadVariableText();
			//Plot layout flags BS 70 plotsettings plot layout flag
			plot.Flags = (PlotFlags)this._objectReader.ReadBitShort();

			PaperMargin margin = new PaperMargin()
			{
				//Left Margin BD 40 plotsettings left margin in millimeters
				Left = this._objectReader.ReadBitDouble(),
				//Bottom Margin BD 41 plotsettings bottom margin in millimeters
				Bottom = this._objectReader.ReadBitDouble(),
				//Right Margin BD 42 plotsettings right margin in millimeters
				Right = this._objectReader.ReadBitDouble(),
				//Top Margin BD 43 plotsettings top margin in millimeters
				Top = this._objectReader.ReadBitDouble()
			};
			plot.UnprintableMargin = margin;

			//Paper Width BD 44 plotsettings paper width in millimeters
			plot.PaperWidth = this._objectReader.ReadBitDouble();
			//Paper Height BD 45 plotsettings paper height in millimeters
			plot.PaperHeight = this._objectReader.ReadBitDouble();

			//Paper Size TV 4 plotsettings paper size
			plot.PaperSize = this._textReader.ReadVariableText();

			//Plot origin 2BD 46,47 plotsettings origin offset in millimeters
			plot.PlotOrigin = this._objectReader.Read2BitDouble();
			//Paper units BS 72 plotsettings plot paper units
			plot.PaperUnits = (PlotPaperUnits)this._objectReader.ReadBitShort();
			//Plot rotation BS 73 plotsettings plot rotation
			plot.PaperRotation = (PlotRotation)this._objectReader.ReadBitShort();
			//Plot type BS 74 plotsettings plot type
			plot.PlotType = (PlotType)this._objectReader.ReadBitShort();

			//Window min 2BD 48,49 plotsettings plot window area lower left
			plot.WindowLowerLeft = this._objectReader.Read2BitDouble();
			//Window max 2BD 140,141 plotsettings plot window area upper right
			plot.WindowUpperLeft = this._objectReader.Read2BitDouble();

			//R13 - R2000 Only:
			if (this._version >= ACadVersion.AC1012 && this._version <= ACadVersion.AC1015)
				//Plot view name T 6 plotsettings plot view name
				plot.PlotViewName = this._textReader.ReadVariableText();

			//Common:
			//Real world units BD 142 plotsettings numerator of custom print scale
			plot.NumeratorScale = this._objectReader.ReadBitDouble();
			//Drawing units BD 143 plotsettings denominator of custom print scale
			plot.DenominatorScale = this._objectReader.ReadBitDouble();
			//Current style sheet TV 7 plotsettings current style sheet
			plot.StyleSheet = this._textReader.ReadVariableText();
			//Scale type BS 75 plotsettings standard scale type
			plot.ScaledFit = (ScaledType)this._objectReader.ReadBitShort();
			//Scale factor BD 147 plotsettings scale factor
			plot.StandardScale = this._objectReader.ReadBitDouble();
			//Paper image origin 2BD 148,149 plotsettings paper image origin
			plot.PaperImageOrigin = this._objectReader.Read2BitDouble();

			//R2004+:
			if (this.R2004Plus)
			{
				//Shade plot mode BS 76
				plot.ShadePlotMode = (ShadePlotMode)this._objectReader.ReadBitShort();
				//Shade plot res.Level BS 77
				plot.ShadePlotResolutionMode = (ShadePlotResolutionMode)this._objectReader.ReadBitShort();
				//Shade plot custom DPI BS 78
				plot.ShadePlotDPI = this._objectReader.ReadBitShort();

				//6 plot view handle(hard pointer)
				ulong plotViewHandle = this.handleReference();
			}

			//R2007 +:
			if (this.R2007Plus)
				//Visual Style handle(soft pointer)
				this.handleReference();
		}

		#endregion Object readers

		private CadTemplate readDwgColor()
		{
			DwgColorTemplate.DwgColor dwgColor = new DwgColorTemplate.DwgColor();
			DwgColorTemplate template = new DwgColorTemplate(dwgColor);

			this.readCommonNonEntityData(template);

			short colorIndex = this._objectReader.ReadBitShort();

			if (this.R2004Plus && this._version < ACadVersion.AC1032)
			{
				short index = (short)this._objectReader.ReadBitLong();
				byte flags = this._objectReader.ReadByte();

				if ((flags & 1U) > 0U)
					template.Name = this._textReader.ReadVariableText();

				if ((flags & 2U) > 0U)
					template.BookName = this._textReader.ReadVariableText();

				dwgColor.Color = new Color(index);
			}

			dwgColor.Color = new Color(colorIndex);

			return template;
		}
	}
}