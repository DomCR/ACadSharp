using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.Entities.Collections;
using ACadSharp.Geometry;
using ACadSharp.Header;
using ACadSharp.IO.Templates;
using CSUtilities;
using CSUtilities.IO;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ACadSharp.IO.DXF
{
	public class DxfReader : IDisposable
	{
		private string m_filename;
		private StreamIO m_fileStream;

		private IDxfStreamReader m_reader;

		/// <summary>
		/// Initializes a new instance of the <see cref="DxfReader" /> class.
		/// </summary>
		/// <param name="filename">The filename of the file to open.</param>
		public DxfReader(string filename)
		{
			m_filename = filename;
			m_fileStream = new StreamIO(filename);
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DxfReader" /> class.
		/// </summary>
		/// <param name="stream">The stream to read from.</param>
		public DxfReader(Stream stream)
		{
			m_fileStream = new StreamIO(stream);
		}
		//**************************************************************************
		/// <summary>
		/// Check if the file format is in binary.
		/// </summary>
		/// <returns></returns>
		public bool IsBinary()
		{
			return IsBinary(m_fileStream.Stream);
		}
		/// <summary>
		/// Check if the file format is in binary.
		/// </summary>
		/// <param name="filename">Path to the dxf file.</param>
		/// <returns></returns>
		public static bool IsBinary(string filename)
		{
			return IsBinary(new StreamIO(filename).Stream);
		}
		/// <summary>
		/// Check if the file format is in binary.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static bool IsBinary(Stream stream)
		{
			string sentinel = "AutoCAD Binary DXF";

			StreamIO sio = new StreamIO(stream);
			sio.Position = 0;
			string sn = sio.ReadString(sentinel.Length);

			return sn == sentinel;
		}
		/// <summary>
		/// Read the complete document.
		/// </summary>
		public CadDocument Read()
		{
			CadDocument doc = new CadDocument();

			doc.Header = ReadHeader();
			doc.Classes = ReadClasses();
			ReadTables(doc);
			ReadEntities(doc);

			return doc;
		}
		/// <summary>
		/// Read the HEADER section of the file.
		/// </summary>
		/// <returns></returns>
		public CadHeader ReadHeader()
		{
			//Get the needed handler
			m_reader = getSectionHandler(DxfFileToken.HeaderSection);

			CadHeader header = new CadHeader();

			Dictionary<string, DxfCode[]> headerMap = CadHeader.GetHeaderMap();

			//Loop until the section ends
			while (!m_reader.EndSectionFound)
			{
				//Get next key/value
				m_reader.ReadNext();

				//Get the current header variable
				string currVar = m_reader.LastValueAsString;

				if (!headerMap.TryGetValue(currVar, out var codes))
					continue;

				object[] parameters = new object[codes.Length];
				for (int i = 0; i < codes.Length; i++)
				{
					m_reader.ReadNext();
					parameters[i] = m_reader.LastValue;
				}

				//Set the header value by name
				header.SetValue(currVar, parameters);
			}

			return header;
		}
		/// <summary>
		/// Read the CLASSES section of the DXF file.
		/// </summary>
		/// <returns></returns>
		public DxfClassCollection ReadClasses()
		{
			//Get the needed handler
			m_reader = getSectionHandler(DxfFileToken.ClassesSection);

			DxfClassCollection classes = new DxfClassCollection();

			//Advance to the first value in the section
			m_reader.ReadNext();
			//Loop until the section ends
			while (!m_reader.EndSectionFound)
			{
				if (m_reader.LastValueAsString == DxfFileToken.ClassEntry)
					classes.Add(readClass());
				else
					m_reader.ReadNext();
			}

			return classes;
		}
		/// <summary>
		/// Read the TABLES section of the DXF file.
		/// </summary>
		/// <param name="document">Document where the tables reside.</param>
		public void ReadTables(CadDocument document)
		{
			if (document == null)
				throw new ArgumentNullException(nameof(document));

			//https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-A9FD9590-C97B-4E41-9F26-BD82C34A4F9F
			//Get the needed handler
			m_reader = getSectionHandler(DxfFileToken.TablesSection);

			//Advance to the first value in the section
			m_reader.ReadNext();
			//Loop until the section ends
			while (!m_reader.EndSectionFound)
			{
				readTable(document);

				if (m_reader.LastValueAsString == DxfFileToken.EndTable)
					m_reader.ReadNext();
			}
		}
		/// <summary>
		/// Read the BLOCKS section of the DXF file.
		/// </summary>
		public void ReadBlocks()
		{
			//Get the needed handler
			m_reader = getSectionHandler(DxfFileToken.BlocksSection);

			//Advance to the first value in the section
			m_reader.ReadNext();
			//Loop until the section ends
			while (!m_reader.EndSectionFound)
			{
				//if (m_reader.LastValueAsString == DxfFileToken.ClassEntry)
				//	classes.Add(readBlock());
				//else
				//	m_reader.ReadNext();
			}

			throw new NotImplementedException();
		}
		/// <summary>
		/// Read the ENTITIES section of the DXF file.
		/// </summary>
		public void ReadEntities(CadDocument document)
		{
			//https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-7D07C886-FD1D-4A0C-A7AB-B4D21F18E484
			//Get the needed handler
			m_reader = getSectionHandler(DxfFileToken.EntitiesSection);

			//Advance to the first value in the section
			m_reader.ReadNext();
			//Loop until the section ends
			while (!m_reader.EndSectionFound)
			{
				document.AddEntity(readEntity());
			}
		}
		/// <summary>
		/// Read the OBJECTS section of the DXF file.
		/// </summary>
		public void ReadObjects()
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Read the THUMBNAILIMAGE section of the DXF file.
		/// </summary>
		public void ReadThumbnailImage()
		{
			throw new NotImplementedException();
		}
		public void Dispose()
		{
			m_fileStream.Dispose();
		}
		//**************************************************************************
		private IDxfStreamReader getHandler()
		{
			if (IsBinary())
				return null;
			else
				return new DxfTextReader(m_fileStream.Stream);

			//TODO: Setup encoding
			//AutoCAD 2007 DXF and later format -UTF - 8
			//AutoCAD 2004 DXF and earlier format -Plain ASCII and CIF
		}
		private IDxfStreamReader getSectionHandler(string sectionName)
		{
			//Get the needed handler
			m_reader = m_reader ?? getHandler();
			//Go to the start of header section
			m_reader.Find(sectionName);

			return m_reader;
		}

		private DxfClass readClass()
		{
			DxfClass curr = new DxfClass();

			Debug.Assert(m_reader.LastValueAsString == DxfFileToken.ClassEntry);

			m_reader.ReadNext();
			//Loop until the next class or the end of the section
			while (m_reader.LastDxfCode != DxfCode.Start)
			{
				switch (m_reader.LastCode)
				{
					//Class DXF record name; always unique
					case 1:
						curr.DxfName = m_reader.LastValueAsString;
						break;
					//C++ class name. Used to bind with software that defines object class behavior; always unique
					case 2:
						curr.CppClassName = m_reader.LastValueAsString;
						break;
					//Application name. Posted in Alert box when a class definition listed in this section is not currently loaded
					case 3:
						curr.ApplicationName = m_reader.LastValueAsString;
						break;
					//Proxy capabilities flag.
					case 90:
						curr.ProxyFlags = (ProxyFlags)m_reader.LastValueAsShort;
						break;
					//Instance count for a custom class
					case 91:
						curr.InstanceCount = m_reader.LastValueAsInt;
						break;
					//Was-a-proxy flag. Set to 1 if class was not loaded when this DXF file was created, and 0 otherwise
					case 280:
						curr.WasAProxy = m_reader.LastValueAsBool;
						break;
					//Is - an - entity flag.
					case 281:
						curr.IsAnEntity = m_reader.LastValueAsBool;
						break;
					default:
						break;
				}

				m_reader.ReadNext();
			}

			return curr;
		}

		#region Table section methods
		/// <summary>
		/// Read the tables in the document.
		/// </summary>
		/// <param name="document">Document where the tables reside.</param>
		private void readTable(CadDocument document)
		{
			//https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-5926A569-3E40-4ED2-AE06-6ACCE0EFC813

			Debug.Assert(m_reader.LastValueAsString == DxfFileToken.TableEntry);
			//Read the table name
			m_reader.ReadNext();

			DxfTableTemplate template = new DxfTableTemplate();
			//Loop until the common data
			while (m_reader.LastDxfCode != DxfCode.Start)
			{
				switch (m_reader.LastCode)
				{
					//Table name
					case 2:
						template.Name = m_reader.LastValueAsString;
						break;
					//Handle
					case 5:
						template.Handle = m_reader.LastValueAsHandle;
						break;
					//Start of application - defined group
					case 102:
						//TODO: read dictionary groups for entities
						do
						{
							m_reader.ReadNext();
						}
						while (m_reader.LastDxfCode != DxfCode.ControlString);
						break;
					//Soft - pointer ID / handle to owner BLOCK_RECORD object
					case 330:
						template.OwnerHandle = m_reader.LastValueAsHandle;
						break;
					//Subclass marker(AcDbSymbolTable)
					case 100:
						Debug.Assert(m_reader.LastValueAsString == DxfSubclassMarker.Table
							|| m_reader.LastValueAsString == DxfSubclassMarker.DimensionStyleTable);

						break;
					case 71:
					//Number of entries for dimension style table
					case 340:
						//Dimension table has the handles of the styles at the begining
						break;
					//Maximum number of entries in table
					case 70:
						template.MaxEntries = m_reader.LastValueAsInt;
						break;
					default:
						Debug.Fail($"Unhandeled dxf code {m_reader.LastCode} at line {m_reader.Line}.");
						break;
				}

				m_reader.ReadNext();
			}

			List<TableEntry> entries = new List<TableEntry>();

			//Check if the table is empty
			if (m_reader.LastValueAsString != DxfFileToken.EndTable)
			{
				//Get the table entries
				Debug.Assert(m_reader.LastValueAsString == template.Name);
				entries = readEntries();
			}

			switch (template.Name)
			{
				case DxfFileToken.TableAppId:
					document.AppIds = new AppIdsTable(template);
					//Add the entries
					foreach (TableEntry item in entries)
					{
						document.AppIds.Add(item as AppId);
					}
					break;
				case DxfFileToken.TableBlockRecord:
					document.BlockRecords = new BlockRecordsTable(template);
					//Add the entries
					foreach (TableEntry item in entries)
					{
						document.BlockRecords.Add(item as BlockRecord);
					}
					break;
				case DxfFileToken.TableDimstyle:
					document.DimensionStyles = new DimensionStylesTable(template);
					//Add the entries
					foreach (TableEntry item in entries)
					{
						document.DimensionStyles.Add(item as DimensionStyle);
					}
					break;
				case DxfFileToken.TableLayer:
					document.Layers = new LayersTable(template);
					//Add the entries
					foreach (TableEntry item in entries)
					{
						document.Layers.Add(item as Layer);
					}
					break;
				case DxfFileToken.TableLinetype:
					document.LineTypes = new LineTypesTable(template);
					//Add the entries
					foreach (TableEntry item in entries)
					{
						document.LineTypes.Add(item as LineType);
					}
					break;
				case DxfFileToken.TableStyle:
					document.Styles = new StylesTable(template);
					//Add the entries
					foreach (TableEntry item in entries)
					{
						document.Styles.Add(item as Style);
					}
					break;
				case DxfFileToken.TableUcs:
					document.UCSs = new UCSTable(template);
					//Add the entries
					foreach (TableEntry item in entries)
					{
						document.UCSs.Add(item as UCS);
					}
					break;
				case DxfFileToken.TableView:
					document.Views = new ViewsTable(template);
					//Add the entries
					foreach (TableEntry item in entries)
					{
						document.Views.Add(item as View);
					}
					break;
				case DxfFileToken.TableVport:
					document.ViewPorts = new ViewPortsTable(template);
					//Add the entries
					foreach (TableEntry item in entries)
					{
						document.ViewPorts.Add(item as VPort);
					}
					break;
				default:
					Debug.Fail($"Unhandeled table {template.Name}.");
					break;
			}
		}

		private List<TableEntry> readEntries()
		{
			List<TableEntry> entries = new List<TableEntry>();

			//Read all the entries until the end of the table
			while (m_reader.LastValueAsString != DxfFileToken.EndTable)
			{
				DxfEntryTemplate template = new DxfEntryTemplate();

				//Read the common entry data
				while (m_reader.LastDxfCode != DxfCode.Subclass)
				{
					switch (m_reader.LastCode)
					{
						//Entity type (table name)
						case 0:
							template.TableName = m_reader.LastValueAsString;
							break;
						//Handle (all except DIMSTYLE)
						case 5:
						//Handle(all except DIMSTYLE)
						case 105:
							template.Handle = m_reader.LastValueAsHandle;
							break;
						//Start of application - defined group
						case 102:
							//TODO: read dictionary groups for entities
							do
							{
								m_reader.ReadNext();
							}
							while (m_reader.LastDxfCode != DxfCode.ControlString);
							break;
						//Soft - pointer ID / handle to owner BLOCK_RECORD object
						case 330:
							template.OwnerHandle = m_reader.LastValueAsHandle;
							break;
						default:
							Debug.Fail($"Unhandeled dxf code {m_reader.LastCode} at line {m_reader.Line}.");
							break;
					}

					m_reader.ReadNext();
				}

				Debug.Assert(m_reader.LastValueAsString == DxfSubclassMarker.TableRecord);
				m_reader.ReadNext();

				TableEntry entry = readEntry(template);
				entries.Add(entry);
			}

			return entries;
		}

		private TableEntry readEntry(DxfEntryTemplate template)
		{
			TableEntry table = null;

			//Get the entry
			switch (template.TableName)
			{
				case DxfFileToken.TableAppId:
					table = new AppId(template);
					break;
				case DxfFileToken.TableBlockRecord:
					table = new BlockRecord(template);
					break;
				case DxfFileToken.TableDimstyle:
					table = new DimensionStyle(template);
					break;
				case DxfFileToken.TableLayer:
					table = new Layer(template);
					break;
				case DxfFileToken.TableLinetype:
					table = new LineType(template);
					break;
				case DxfFileToken.TableStyle:
					table = new Style(template);
					break;
				case DxfFileToken.TableUcs:
					table = new UCS(template);
					break;
				case DxfFileToken.TableView:
					table = new View(template);
					break;
				case DxfFileToken.TableVport:
					table = new VPort(template);
					break;
				default:
					Debug.Fail($"Unhandeled table {template.Name}.");
					break;
			}

			//Jump the SubclassMarker
			m_reader.ReadNext();

			Dictionary<DxfCode, object> map = table?.GetCadObjectMap() ?? new Dictionary<DxfCode, object>();

			while (m_reader.LastDxfCode != DxfCode.Start)
			{
				//Check if the dxf code is registered
				if (map.ContainsKey(m_reader.LastDxfCode))
				{
					//Set the value
					map[m_reader.LastDxfCode] = m_reader.LastValue;
				}

				m_reader.ReadNext();
			}

			//Build the table based on the map
			table?.Build(map);

			return table;
		}
		#endregion

		#region Entities section methods
		private Entity readEntity()
		{
			//https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-3610039E-27D1-4E23-B6D3-7E60B22BB5BD

			DxfEntityTemplate template = new DxfEntityTemplate();
			//Loop until the common data
			while (m_reader.LastDxfCode != DxfCode.Subclass)
			{
				switch (m_reader.LastCode)
				{
					//APP: entity name(changes each time a drawing is opened)
					case -1:
						break;
					//Entity type
					case 0:
						template.EntityName = m_reader.LastValueAsString;
						break;
					//Handle
					case 5:
						template.Handle = m_reader.LastValueAsHandle;
						break;
					//Start of application - defined group
					case 102:
						//TODO: read dictionary groups for entities
						while (m_reader.LastDxfCode != DxfCode.ControlString)
						{
							m_reader.ReadNext();
						}
						break;
					//Soft - pointer ID / handle to owner BLOCK_RECORD object
					case 330:
						template.OwnerHandle = m_reader.LastValueAsHandle;
						break;
					default:
						//Debug.Fail($"Unhandeled dxf code {m_reader.LastCode} at line {m_reader.Line}.");
						break;
				}

				//Get the next code/value
				m_reader.ReadNext();
			}

			//Get the subclass common entity data
			Debug.Assert(m_reader.LastValueAsString == DxfSubclassMarker.Entity);
			m_reader.ReadNext();

			while (m_reader.LastDxfCode != DxfCode.Subclass)
			{
				switch (m_reader.LastCode)
				{
					//Absent or zero indicates entity is in model space. 1 indicates entity is in paper space (optional).
					case 67:
						break;
					//APP: layout tab name
					case 410:
						break;
					//Layer name
					case 8:
						//TODO: Create a link with the file layers
						template.Layer = new Layer(m_reader.LastValueAsString);
						break;
					//Linetype name(present if not BYLAYER). The special name BYBLOCK indicates a floating linetype(optional)
					case 6:
						break;
					//Hard - pointer ID / handle to material object(present if not BYLAYER)
					case 347:
						break;
					//Color number(present if not BYLAYER); zero indicates the BYBLOCK(floating) color; 256 indicates BYLAYER; a negative value indicates that the layer is turned off (optional)
					case 62:

						break;
					//Lineweight enum value. Stored and moved around as a 16-bit integer.
					case 370:
						template.Lineweight = (Lineweight)m_reader.LastValueAsShort;
						break;
					//Linetype scale (optional)
					case 48:
						template.LinetypeScale = m_reader.LastValueAsDouble;
						break;
					//Object visibility (optional)
					case 60:
						template.IsInvisible = m_reader.LastValueAsBool;
						break;
					//Number of bytes in the proxy entity graphics represented in the subsequent 310 groups, which are binary chunk records (optional)
					case 92:
						break;
					//Proxy entity graphics data (multiple lines; 256 characters max. per line) (optional)
					case 310:
						break;
					//A 24 - bit color value that should be dealt with in terms of bytes with values of 0 to 255.The lowest byte is the blue value, the middle byte is the green value, and the third byte is the red value.The top byte is always 0.The group code cannot be used by custom entities for their own data because the group code is reserved for AcDbEntity, class-level color data and AcDbEntity, class-level transparency data
					case 420:
						break;
					//Color name. The group code cannot be used by custom entities for their own data because the group code is reserved for AcDbEntity, class-level color data and AcDbEntity, class-level transparency data
					case 430:
						break;
					//Transparency value. The group code cannot be used by custom entities for their own data because the group code is reserved for AcDbEntity, class-level color data and AcDbEntity, class-level transparency data
					case 440:
						template.Transparency = new Transparency(m_reader.LastValueAsShort);
						break;
					//Hard-pointer ID/handle to the plot style object
					case 390:
						break;
					//Shadow mode
					case 284:
						break;
					default:
						//Debug.Fail($"Unhandeled dxf code {m_reader.LastCode} at line {m_reader.Line}.");
						break;
				}

				//Get the next code/value
				m_reader.ReadNext();
			}

			Entity entity = null;

			switch (template.EntityName)
			{
				case DxfFileToken.EntityArc:
					entity = readArc(template);
					break;
				case DxfFileToken.EntityCircle:
					entity = readCircle(template);
					break;
				case DxfFileToken.EntityPolyline:
				//	entity = readPolyline(template);
				//	break;
				//case DxfFileToken.EntityText:
				//	entity = readText(template);
				//	break;
				default:
					entity = readEntity(template);
					//Debug.Fail($"Unhandeled entity {template.EntityName}.");
					break;
			}

			return entity;
		}
		/// <summary>
		/// Reflection method to read entities.
		/// </summary>
		/// <remarks>
		/// Is functional but not very reliable so it should disapear in the future and have a method for each entity.
		/// </remarks>
		/// <param name="template"></param>
		/// <returns></returns>
		private Entity readEntity(DxfEntityTemplate template)
		{
			Entity entity = null;

			//Get the current entity
			switch (template.EntityName)
			{
				//TODO: Check the SubclassMarker
				case DxfFileToken.EntityArc:
					entity = new Arc(template);
					break;
				case DxfFileToken.EntityCircle:
					entity = new Circle(template);
					break;
				case DxfFileToken.EntityPolyline:
					entity = new PolyLine(template);
					break;
				case DxfFileToken.EntityText:
					entity = new Text(template);
					break;
				case DxfFileToken.EntityVertex:
					entity = new Vertex(template);
					break;
				case DxfFileToken.EntityAttributeDefinition:
					Debug.Fail("Check the property VerticalJustification and see the assigned code, should be 74");
					entity = new AttributeDefinition(template);
					break;
				default:
					//Debug.Fail($"Unhandeled entity {template.EntityName}.");
					break;
			}

			//Jump the SubclassMarker
			m_reader.ReadNext();

			Dictionary<DxfCode, object> map = entity?.GetCadObjectMap() ?? new Dictionary<DxfCode, object>();

			while (m_reader.LastValueAsString != DxfFileToken.EndSection)
			{
				if (m_reader.LastDxfCode == DxfCode.Start)
				{
					//Check if the entity has children in it
					Dictionary<string, PropertyInfo> subEntity = entity?.GetSubEntitiesMap() ?? new Dictionary<string, PropertyInfo>();

					if (!subEntity.ContainsKey(m_reader.LastValueAsString))
						//Is a separated entity
						break;

					//Read the sequence
					while (m_reader.LastValueAsString != DxfFileToken.EndSequence)
					{
						Entity child = this.readEntity();

						PropertyInfo prop = subEntity[child.ObjectName];

						if (prop.GetValue(entity, null) is ICollection<Entity> pvalue)
						{
							pvalue.Add(child);
						}
						else
						{
							//Will be used for something??
							Debug.Fail("");
							prop.SetValue(entity, child);
						}
					}

					//Read the end of sequence
					m_reader.ReadNext();
					while (m_reader.LastDxfCode != DxfCode.Start)
					{
						m_reader.ReadNext();
					}

					//The end of the sequence is the end of the entity
					break;
				}
				else if (map.ContainsKey(m_reader.LastDxfCode))
				{
					//Set the value
					map[m_reader.LastDxfCode] = m_reader.LastValue;
				}

				//Get the next line
				m_reader.ReadNext();
			}

			//Build the entity based on the map
			entity?.Build(map);

			return entity;
		}

		private Arc readArc(DxfEntityTemplate template)
		{
			//Create the arc based on the template
			Arc arc = new Arc(template);

			//Pre-declare structures
			XYZ center = XYZ.Zero;
			XYZ normal = XYZ.Zero = XYZ.AxisZ;

			while (m_reader.LastDxfCode != DxfCode.Start)
			{
				switch (m_reader.LastCode)
				{
					//Subclass marker (AcDbCircle)
					//Subclass marker (AcDbArc)
					case 100:
						Debug.Assert(m_reader.LastValueAsString == "AcDbArc" || m_reader.LastValueAsString == "AcDbCircle");
						break;
					//Thickness (optional; default = 0)
					case 39:
						arc.Thickness = m_reader.LastValueAsDouble;
						break;
					//Center point (in OCS)
					//DXF: X value; APP: 3D point
					case 10:
						center.X = m_reader.LastValueAsDouble;
						break;
					//DXF: Y and Z values of center point (in OCS)
					case 20:
						center.Y = m_reader.LastValueAsDouble;
						break;
					case 30:
						center.Z = m_reader.LastValueAsDouble;
						break;
					//Radius
					case 40:
						arc.Radius = m_reader.LastValueAsDouble;
						break;
					//Start angle
					case 50:
						arc.StartAngle = m_reader.LastValueAsDouble;
						break;
					//End angle
					case 51:
						arc.EndAngle = m_reader.LastValueAsDouble;
						break;
					//Extrusion direction (optional; default = 0, 0, 1)
					//DXF: X value; APP: 3D vector
					case 210:
						normal.X = m_reader.LastValueAsDouble;
						break;
					//DXF: Y and Z values of extrusion direction (optional)
					case 220:
						normal.Y = m_reader.LastValueAsDouble;
						break;
					case 230:
						normal.Z = m_reader.LastValueAsDouble;
						break;
					default:
						Debug.Fail($"Unhandeled dxf code {m_reader.LastCode} at line {m_reader.Line}.");
						break;
				}

				m_reader.ReadNext();
			}

			//Assign the structures
			arc.Center = center;
			arc.Normal = normal;

			return arc;
		}
		private Circle readCircle(DxfEntityTemplate template)
		{
			Circle circle = new Circle(template);

			//Pre-declare structures
			XYZ center = XYZ.Zero;
			XYZ normal = XYZ.Zero = XYZ.AxisZ;

			while (m_reader.LastDxfCode != DxfCode.Start)
			{
				switch (m_reader.LastCode)
				{
					//Subclass marker (AcDbCircle)
					case 100:
						Debug.Assert(m_reader.LastValueAsString == "AcDbCircle");
						break;
					//Thickness (optional; default = 0)
					case 39:
						circle.Thickness = m_reader.LastValueAsDouble;
						break;
					//Center point (in OCS)
					//DXF: X value; APP: 3D point
					case 10:
						center.X = m_reader.LastValueAsDouble;
						break;
					//DXF: Y and Z values of center point (in OCS)
					case 20:
						center.Y = m_reader.LastValueAsDouble;
						break;
					case 30:
						center.Z = m_reader.LastValueAsDouble;
						break;
					//Radius
					case 40:
						circle.Radius = m_reader.LastValueAsDouble;
						break;
					//Extrusion direction (optional; default = 0, 0, 1)
					//DXF: X value; APP: 3D vector
					case 210:
						normal.X = m_reader.LastValueAsDouble;
						break;
					//DXF: Y and Z values of extrusion direction (optional)
					case 220:
						normal.Y = m_reader.LastValueAsDouble;
						break;
					case 230:
						normal.Z = m_reader.LastValueAsDouble;
						break;
					default:
						Debug.Fail($"Unhandeled dxf code {m_reader.LastCode} at line {m_reader.Line}.");
						break;
				}

				m_reader.ReadNext();
			}

			circle.Center = center;
			circle.Normal = normal;

			return circle;
		}
		private Text readText(DxfEntityTemplate template)
		{
			//https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-62E5383D-8A14-47B4-BFC4-35824CAE8363

			//Create the arc based on the template
			Text text = new Text(template);

			//Pre-declare structures
			XYZ firstAlignmentPoint = XYZ.Zero;
			XYZ secondAlignmentPoint = XYZ.Zero;
			XYZ normal = XYZ.Zero = XYZ.AxisZ;

			while (m_reader.LastDxfCode != DxfCode.Start)
			{
				switch (m_reader.LastCode)
				{
					//Subclass marker(AcDbText)
					case 100:
						Debug.Assert(m_reader.LastValueAsString == "AcDbText");
						break;
					//Thickness(optional; default = 0)
					case 39:
						text.Thickness = m_reader.LastValueAsDouble;
						break;
					//First alignment point (in OCS)
					//DXF: X value; APP: 3D point
					case 10:
						firstAlignmentPoint.X = m_reader.LastValueAsDouble;
						break;
					//DXF: Y and Z values of center point (in OCS)
					case 20:
						firstAlignmentPoint.Y = m_reader.LastValueAsDouble;
						break;
					case 30:
						firstAlignmentPoint.Z = m_reader.LastValueAsDouble;
						break;
					//Text height
					case 40:
						text.Height = m_reader.LastValueAsDouble;
						break;
					//Default value(the string itself)
					case 1:
						text.Value = m_reader.LastValueAsString;
						break;
					//Text rotation (optional; default = 0)
					case 50:
						text.Rotation = m_reader.LastValueAsDouble;
						break;
					default:
						//Debug.Fail($"Unhandeled dxf code {m_reader.LastCode} at line {m_reader.Line}.");
						break;
				}

				m_reader.ReadNext();
			}

			return null;
		}
		private PolyLine readPolyline(DxfEntityTemplate template)
		{
			PolyLine polyline = new PolyLine(template);

			//Pre-declare structures
			XYZ normal = XYZ.Zero = XYZ.AxisZ;

			while (m_reader.LastDxfCode != DxfCode.Start)
			{

			}

			polyline.Normal = normal;

			return polyline;
		}
		private Hatch readHatch(DxfEntityTemplate template)
		{
			Hatch hatch = new Hatch(template);

			//Pre-declare structures
			XYZ normal = XYZ.Zero = XYZ.AxisZ;

			while (m_reader.LastDxfCode != DxfCode.Start)
			{

			}

			hatch.Normal = normal;

			return hatch;
		}
		#endregion
	}
}
