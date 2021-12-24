using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.Header;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using CSMath;
using CSUtilities.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ACadSharp.IO.DXF
{
	public class DxfReader : CadReaderBase
	{
		private CadDocument _document;
		private DxfDocumentBuilder _builder;
		private IDxfStreamReader _reader;

		private event NotificationEventHandler _notificationHandler;

		/// <summary>
		/// Initializes a new instance of the <see cref="DxfReader" /> class.
		/// </summary>
		/// <param name="filename">The filename of the file to open.</param>
		/// <param name="notification">Notification handler, sends any message or notification about the reading process.</param>
		public DxfReader(string filename, NotificationEventHandler notification = null) : base(filename, notification) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="DxfReader" /> class.
		/// </summary>
		/// <param name="stream">The stream to read from.</param>
		/// <param name="notification">Notification handler, sends any message or notification about the reading process.</param>
		public DxfReader(Stream stream, NotificationEventHandler notification = null) : base(stream, notification) { }

		/// <summary>
		/// Check if the file format is in binary.
		/// </summary>
		/// <returns></returns>
		public bool IsBinary()
		{
			return IsBinary(this._fileStream.Stream);
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
		/// Read a dxf document in a stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="notification">Notification handler, sends any message or notification about the reading process.</param>
		/// <returns></returns>
		public static CadDocument Read(Stream stream, NotificationEventHandler notification = null)
		{
			CadDocument doc = null;

			using (DxfReader reader = new DxfReader(stream, notification))
			{
				doc = reader.Read();
			}

			return doc;
		}

		/// <summary>
		/// Read a dxf document from a file
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="notification">Notification handler, sends any message or notification about the reading process.</param>
		/// <returns></returns>
		public static CadDocument Read(string filename, NotificationEventHandler notification = null)
		{
			CadDocument doc = null;

			using (DxfReader reader = new DxfReader(filename, notification))
			{
				doc = reader.Read();
			}

			return doc;
		}

		/// <inheritdoc/>
		public override CadDocument Read()
		{
			this._document = new CadDocument();
			this._builder = new DxfDocumentBuilder(this._document, this._notificationHandler);

			this._document.Header = this.ReadHeader();
			this._document.Classes = this.readClasses();

			this.readTables();

			this.readBlocks();

			this.readEntities();

			return this._document;
		}

		/// <inheritdoc/>
		public override CadHeader ReadHeader()
		{
			//Get the needed handler
			this._reader = this.goToSection(DxfFileToken.HeaderSection);

			CadHeader header = new CadHeader();

			Dictionary<string, DxfCode[]> headerMap = CadHeader.GetHeaderMap();

			//Loop until the section ends
			while (!this._reader.EndSectionFound)
			{
				//Get next key/value
				this._reader.ReadNext();

				//Get the current header variable
				string currVar = this._reader.LastValueAsString;

				if (!headerMap.TryGetValue(currVar, out var codes))
					continue;

				object[] parameters = new object[codes.Length];
				for (int i = 0; i < codes.Length; i++)
				{
					this._reader.ReadNext();
					parameters[i] = this._reader.LastValue;
				}

				//Set the header value by name
				header.SetValue(currVar, parameters);
			}

			return header;
		}

		#region DxfClasses

		/// <summary>
		/// Read the CLASSES section of the DXF file.
		/// </summary>
		/// <returns></returns>
		private DxfClassCollection readClasses()
		{
			//Get the needed handler
			this._reader = this.goToSection(DxfFileToken.ClassesSection);

			DxfClassCollection classes = new DxfClassCollection();

			//Advance to the first value in the section
			this._reader.ReadNext();
			//Loop until the section ends
			while (!this._reader.EndSectionFound)
			{
				if (this._reader.LastValueAsString == DxfFileToken.ClassEntry)
					classes.Add(this.readClass());
				else
					this._reader.ReadNext();
			}

			return classes;
		}

		private DxfClass readClass()
		{
			DxfClass curr = new DxfClass();

			Debug.Assert(this._reader.LastValueAsString == DxfFileToken.ClassEntry);

			this._reader.ReadNext();
			//Loop until the next class or the end of the section
			while (this._reader.LastDxfCode != DxfCode.Start)
			{
				switch (this._reader.LastCode)
				{
					//Class DXF record name; always unique
					case 1:
						curr.DxfName = this._reader.LastValueAsString;
						break;
					//C++ class name. Used to bind with software that defines object class behavior; always unique
					case 2:
						curr.CppClassName = this._reader.LastValueAsString;
						break;
					//Application name. Posted in Alert box when a class definition listed in this section is not currently loaded
					case 3:
						curr.ApplicationName = this._reader.LastValueAsString;
						break;
					//Proxy capabilities flag.
					case 90:
						curr.ProxyFlags = (ProxyFlags)this._reader.LastValueAsShort;
						break;
					//Instance count for a custom class
					case 91:
						curr.InstanceCount = this._reader.LastValueAsInt;
						break;
					//Was-a-proxy flag. Set to 1 if class was not loaded when this DXF file was created, and 0 otherwise
					case 280:
						curr.WasAProxy = this._reader.LastValueAsBool;
						break;
					//Is - an - entity flag.
					case 281:
						curr.IsAnEntity = this._reader.LastValueAsBool;
						break;
					default:
						break;
				}

				this._reader.ReadNext();
			}

			return curr;
		}

		#endregion

		#region Tables

		/// <summary>
		/// Read the TABLES section of the DXF file.
		/// </summary>
		private void readTables()
		{
			//Get the needed handler
			this._reader = this.goToSection(DxfFileToken.TablesSection);

			DxfTablesSectionReader reader = new DxfTablesSectionReader(
				this._reader,
				this._builder,
				this._notificationHandler);

			reader.Read();
		}

		#endregion

		/// <summary>
		/// Read the BLOCKS section of the DXF file.
		/// </summary>
		private void readBlocks()
		{
			//Get the needed handler
			this._reader = this.goToSection(DxfFileToken.BlocksSection);

			DxfBlockSectionReader reader = new DxfBlockSectionReader(
				this._reader,
				this._builder,
				this._notificationHandler);

			reader.Read();
		}

		/// <summary>
		/// Read the ENTITIES section of the DXF file.
		/// </summary>
		private void readEntities()
		{
			//Get the needed handler
			this._reader = this.goToSection(DxfFileToken.EntitiesSection);

			DxfEntitiesSectionReader reader = new DxfEntitiesSectionReader(
				this._reader,
				this._builder,
				this._notificationHandler);

			reader.Read();
		}

		/// <summary>
		/// Read the OBJECTS section of the DXF file.
		/// </summary>
		private void readObjects()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Read the THUMBNAILIMAGE section of the DXF file.
		/// </summary>
		private void ReadThumbnailImage()
		{
			throw new NotImplementedException();
		}

		private IDxfStreamReader getHandler()
		{
			if (this.IsBinary())
				throw new NotImplementedException();
			else
				return new DxfTextReader(this._fileStream.Stream);

			//TODO: Setup encoding
			//AutoCAD 2007 DXF and later format - UTF-8
			//AutoCAD 2004 DXF and earlier format - Plain ASCII and CIF
		}

		private IDxfStreamReader goToSection(string sectionName)
		{
			//Get the needed handler
			this._reader = this._reader ?? this.getHandler();
			//Go to the start of header section
			this._reader.Find(sectionName);

			return this._reader;
		}

		#region Entities section methods

		private Entity readEntity()
		{
			//https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-3610039E-27D1-4E23-B6D3-7E60B22BB5BD

			DxfEntityTemplate template = new DxfEntityTemplate();
			//Loop until the common data
			while (this._reader.LastDxfCode != DxfCode.Subclass)
			{
				switch (this._reader.LastCode)
				{
					//APP: entity name(changes each time a drawing is opened)
					case -1:
						break;
					//Entity type
					case 0:
						template.EntityName = this._reader.LastValueAsString;
						break;
					//Handle
					case 5:
						template.Handle = this._reader.LastValueAsHandle;
						break;
					//Start of application - defined group
					case 102:
						//TODO: read dictionary groups for entities
						while (this._reader.LastDxfCode != DxfCode.ControlString)
						{
							this._reader.ReadNext();
						}
						break;
					//Soft - pointer ID / handle to owner BLOCK_RECORD object
					case 330:
						template.OwnerHandle = this._reader.LastValueAsHandle;
						break;
					default:
						//Debug.Fail($"Unhandeled dxf code {m_reader.LastCode} at line {m_reader.Line}.");
						break;
				}

				//Get the next code/value
				this._reader.ReadNext();
			}

			//Get the subclass common entity data
			Debug.Assert(this._reader.LastValueAsString == DxfSubclassMarker.Entity);
			this._reader.ReadNext();

			while (this._reader.LastDxfCode != DxfCode.Subclass)
			{
				switch (this._reader.LastCode)
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
						template.Layer = new Layer(this._reader.LastValueAsString);
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
						template.Lineweight = (LineweightType)this._reader.LastValueAsShort;
						break;
					//Linetype scale (optional)
					case 48:
						template.LinetypeScale = this._reader.LastValueAsDouble;
						break;
					//Object visibility (optional)
					case 60:
						template.IsInvisible = this._reader.LastValueAsBool;
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
						template.Transparency = new Transparency(this._reader.LastValueAsShort);
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
				this._reader.ReadNext();
			}

			Entity entity = null;

			switch (template.EntityName)
			{
				case DxfFileToken.EntityArc:
					entity = this.readArc(template);
					break;
				case DxfFileToken.EntityCircle:
					entity = this.readCircle(template);
					break;
				case DxfFileToken.EntityPolyline:
				//	entity = readPolyline(template);
				//	break;
				//case DxfFileToken.EntityText:
				//	entity = readText(template);
				//	break;
				default:
					entity = this.readEntity(template);
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
			this._reader.ReadNext();

			Dictionary<DxfCode, object> map = entity?.GetCadObjectMap() ?? new Dictionary<DxfCode, object>();

			while (this._reader.LastValueAsString != DxfFileToken.EndSection)
			{
				if (this._reader.LastDxfCode == DxfCode.Start)
				{
					//Check if the entity has children in it
					Dictionary<string, PropertyInfo> subEntity = entity?.GetSubEntitiesMap() ?? new Dictionary<string, PropertyInfo>();

					if (!subEntity.ContainsKey(this._reader.LastValueAsString))
						//Is a separated entity
						break;

					//Read the sequence
					while (this._reader.LastValueAsString != DxfFileToken.EndSequence)
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
					this._reader.ReadNext();
					while (this._reader.LastDxfCode != DxfCode.Start)
					{
						this._reader.ReadNext();
					}

					//The end of the sequence is the end of the entity
					break;
				}
				else if (map.ContainsKey(this._reader.LastDxfCode))
				{
					//Set the value
					map[this._reader.LastDxfCode] = this._reader.LastValue;
				}

				//Get the next line
				this._reader.ReadNext();
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
			XYZ normal = XYZ.AxisZ;

			while (this._reader.LastDxfCode != DxfCode.Start)
			{
				switch (this._reader.LastCode)
				{
					//Subclass marker (AcDbCircle)
					//Subclass marker (AcDbArc)
					case 100:
						Debug.Assert(this._reader.LastValueAsString == "AcDbArc" || this._reader.LastValueAsString == "AcDbCircle");
						break;
					//Thickness (optional; default = 0)
					case 39:
						arc.Thickness = this._reader.LastValueAsDouble;
						break;
					//Center point (in OCS)
					//DXF: X value; APP: 3D point
					case 10:
						center.X = this._reader.LastValueAsDouble;
						break;
					//DXF: Y and Z values of center point (in OCS)
					case 20:
						center.Y = this._reader.LastValueAsDouble;
						break;
					case 30:
						center.Z = this._reader.LastValueAsDouble;
						break;
					//Radius
					case 40:
						arc.Radius = this._reader.LastValueAsDouble;
						break;
					//Start angle
					case 50:
						arc.StartAngle = this._reader.LastValueAsDouble;
						break;
					//End angle
					case 51:
						arc.EndAngle = this._reader.LastValueAsDouble;
						break;
					//Extrusion direction (optional; default = 0, 0, 1)
					//DXF: X value; APP: 3D vector
					case 210:
						normal.X = this._reader.LastValueAsDouble;
						break;
					//DXF: Y and Z values of extrusion direction (optional)
					case 220:
						normal.Y = this._reader.LastValueAsDouble;
						break;
					case 230:
						normal.Z = this._reader.LastValueAsDouble;
						break;
					default:
						Debug.Fail($"Unhandeled dxf code {this._reader.LastCode} at line {this._reader.Line}.");
						break;
				}

				this._reader.ReadNext();
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
			XYZ normal = XYZ.AxisZ;

			while (this._reader.LastDxfCode != DxfCode.Start)
			{
				switch (this._reader.LastCode)
				{
					//Subclass marker (AcDbCircle)
					case 100:
						Debug.Assert(this._reader.LastValueAsString == "AcDbCircle");
						break;
					//Thickness (optional; default = 0)
					case 39:
						circle.Thickness = this._reader.LastValueAsDouble;
						break;
					//Center point (in OCS)
					//DXF: X value; APP: 3D point
					case 10:
						center.X = this._reader.LastValueAsDouble;
						break;
					//DXF: Y and Z values of center point (in OCS)
					case 20:
						center.Y = this._reader.LastValueAsDouble;
						break;
					case 30:
						center.Z = this._reader.LastValueAsDouble;
						break;
					//Radius
					case 40:
						circle.Radius = this._reader.LastValueAsDouble;
						break;
					//Extrusion direction (optional; default = 0, 0, 1)
					//DXF: X value; APP: 3D vector
					case 210:
						normal.X = this._reader.LastValueAsDouble;
						break;
					//DXF: Y and Z values of extrusion direction (optional)
					case 220:
						normal.Y = this._reader.LastValueAsDouble;
						break;
					case 230:
						normal.Z = this._reader.LastValueAsDouble;
						break;
					default:
						Debug.Fail($"Unhandeled dxf code {this._reader.LastCode} at line {this._reader.Line}.");
						break;
				}

				this._reader.ReadNext();
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
			XYZ normal = XYZ.AxisZ;

			while (this._reader.LastDxfCode != DxfCode.Start)
			{
				switch (this._reader.LastCode)
				{
					//Subclass marker(AcDbText)
					case 100:
						Debug.Assert(this._reader.LastValueAsString == "AcDbText");
						break;
					//Thickness(optional; default = 0)
					case 39:
						text.Thickness = this._reader.LastValueAsDouble;
						break;
					//First alignment point (in OCS)
					//DXF: X value; APP: 3D point
					case 10:
						firstAlignmentPoint.X = this._reader.LastValueAsDouble;
						break;
					//DXF: Y and Z values of center point (in OCS)
					case 20:
						firstAlignmentPoint.Y = this._reader.LastValueAsDouble;
						break;
					case 30:
						firstAlignmentPoint.Z = this._reader.LastValueAsDouble;
						break;
					//Text height
					case 40:
						text.Height = this._reader.LastValueAsDouble;
						break;
					//Default value(the string itself)
					case 1:
						text.Value = this._reader.LastValueAsString;
						break;
					//Text rotation (optional; default = 0)
					case 50:
						text.Rotation = this._reader.LastValueAsDouble;
						break;
					default:
						//Debug.Fail($"Unhandeled dxf code {m_reader.LastCode} at line {m_reader.Line}.");
						break;
				}

				this._reader.ReadNext();
			}

			return null;
		}

		private PolyLine readPolyline(DxfEntityTemplate template)
		{
			PolyLine polyline = new PolyLine(template);

			//Pre-declare structures
			XYZ normal = XYZ.AxisZ;

			while (this._reader.LastDxfCode != DxfCode.Start)
			{
			}

			polyline.Normal = normal;

			return polyline;
		}

		private Hatch readHatch(DxfEntityTemplate template)
		{
			Hatch hatch = new Hatch(template);

			//Pre-declare structures
			XYZ normal = XYZ.AxisZ;

			while (this._reader.LastDxfCode != DxfCode.Start)
			{
			}

			hatch.Normal = normal;

			return hatch;
		}

		#endregion Entities section methods
	}
}