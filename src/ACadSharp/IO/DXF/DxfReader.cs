using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.Exceptions;
using ACadSharp.Header;
using ACadSharp.IO.DXF;
using CSUtilities.IO;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ACadSharp.IO
{
	/// <summary>
	/// Class for reading a DXF file into a <see cref="CadDocument"></see>.
	/// </summary>
	public class DxfReader : CadReaderBase<DxfReaderConfiguration>
	{
		private ACadVersion _version;
		private DxfDocumentBuilder _builder;
		private IDxfStreamReader _reader;

		/// <summary>
		/// Initializes a new instance of the <see cref="DxfReader"/> class
		/// </summary>
		/// <param name="filename">The file to open.</param>
		/// <param name="notification">Notification handler, sends any message or notification about the reading process.</param>
		public DxfReader(string filename, NotificationEventHandler notification = null) : base(filename, notification) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="DxfReader"/> class
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
			Stream stream = File.OpenRead(filename);
			bool result = IsBinary(stream);

			stream.Close();

			return result;
		}

		/// <summary>
		/// Check if the file format is in binary.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="resetPos"></param>
		/// <returns></returns>
		public static bool IsBinary(Stream stream, bool resetPos = false)
		{
			StreamIO sio = new StreamIO(stream);
			sio.Position = 0;
			string sn = sio.ReadString(DxfBinaryReader.Sentinel.Length);

			bool isBinary = sn == DxfBinaryReader.Sentinel;

			if (resetPos)
			{
				stream.Position = 0;
			}

			return isBinary;
		}

		public static CadDocument Read(string filename, DxfReaderConfiguration configuration, NotificationEventHandler notification = null)
		{
			CadDocument doc = null;

			using (DxfReader reader = new DxfReader(filename, notification))
			{
				reader.Configuration = configuration;
				doc = reader.Read();
			}

			return doc;
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
			return Read(File.OpenRead(filename), notification);
		}

		/// <inheritdoc/>
		public override CadDocument Read()
		{
			this._document = new CadDocument(false);
			this._document.SummaryInfo = new CadSummaryInfo();

			this._reader = this._reader ?? this.getReader();

			this._builder = new DxfDocumentBuilder(this._version, this._document, this.Configuration);
			this._builder.OnNotification += this.onNotificationEvent;

			while (this._reader.ValueAsString != DxfFileToken.EndOfFile)
			{
				if (this._reader.ValueAsString != DxfFileToken.BeginSection)
				{
					this._reader.ReadNext();
					continue;
				}
				else
				{
					this._reader.ReadNext();
				}

				switch (this._reader.ValueAsString)
				{
					case DxfFileToken.HeaderSection:
						this._document.Header = this.ReadHeader();
						this._document.Header.Document = this._document;
						this._builder.InitialHandSeed = this._document.Header.HandleSeed;
						break;
					case DxfFileToken.ClassesSection:
						this._document.Classes = this.readClasses();
						break;
					case DxfFileToken.TablesSection:
						this.readTables();
						break;
					case DxfFileToken.BlocksSection:
						this.readBlocks();
						break;
					case DxfFileToken.EntitiesSection:
						this.readEntities();
						break;
					case DxfFileToken.ObjectsSection:
						this.readObjects();
						break;
					default:
						this.triggerNotification(($"Section not implemented {this._reader.ValueAsString}"), NotificationType.NotImplemented);
						break;
				}

				this._reader.ReadNext();
			}

			if (this._document.Header == null)
			{
				this._document.Header = new CadHeader(this._document);
			}

			this._builder.BuildDocument();

			return this._document;
		}

		/// <inheritdoc/>
		public override CadHeader ReadHeader()
		{
			this._reader = this.goToSection(DxfFileToken.HeaderSection);

			CadHeader header = new CadHeader();

			Dictionary<string, CadSystemVariable> headerMap = CadHeader.GetHeaderMap();

			this._reader.ReadNext();

			//Loop until the section ends
			while (this._reader.ValueAsString != DxfFileToken.EndSection)
			{
				//Get the current header variable
				string currVar = this._reader.ValueAsString;

				if (this._reader.ValueAsString == null || !headerMap.TryGetValue(currVar, out CadSystemVariable data))
				{
#if TEST
					this.triggerNotification($"Header variable not implemented {currVar}", NotificationType.NotImplemented);
#endif
					this._reader.ReadNext();
					continue;
				}

				object[] parameters = new object[data.DxfCodes.Length];
				for (int i = 0; i < data.DxfCodes.Length; i++)
				{
					this._reader.ReadNext();

					if (this._reader.DxfCode == DxfCode.CLShapeText)
					{
						//Irregular dxf files may not follow the header type
						int c = data.DxfCodes[i];
						GroupCodeValueType g = GroupCodeValue.TransformValue(c);
						switch (g)
						{
							case GroupCodeValueType.Bool:
								parameters[i] = false;
								break;
							case GroupCodeValueType.Byte:
							case GroupCodeValueType.Int16:
							case GroupCodeValueType.Int32:
							case GroupCodeValueType.Int64:
							case GroupCodeValueType.Double:
							case GroupCodeValueType.Point3D:
								parameters[i] = 0;
								break;
							case GroupCodeValueType.None:
							case GroupCodeValueType.String:
							default:
								parameters[i] = default;
								break;
						}

						break;
					}

					parameters[i] = this._reader.Value;
				}

				try
				{
					//Set the header value by name
					header.SetValue(currVar, parameters);
				}
				catch (Exception ex)
				{
					this.triggerNotification($"Invalid value for header variable {currVar} | {parameters.FirstOrDefault()}", NotificationType.Warning, ex);
				}

				if (this._reader.DxfCode != DxfCode.CLShapeText)
				{
					this._reader.ReadNext();
				}
			}

			return header;
		}

		/// <summary>
		/// Read only the tables section in the dxf document
		/// </summary>
		/// <remarks>
		/// The <see cref="CadDocument"/> will not contain any entity, only the tables and it's records
		/// </remarks>
		/// <returns></returns>
		public CadDocument ReadTables()
		{
			this._reader = this._reader ?? this.getReader();

			this._builder = new DxfDocumentBuilder(this._version, this._document, this.Configuration);
			this._builder.OnNotification += this.onNotificationEvent;

			this.readTables();

			this._document.Header = new CadHeader(this._document);

			this._builder.RegisterTables();

			this._builder.BuildTables();

			return this._document;
		}

		/// <summary>
		/// Read only the entities section in the dxf document
		/// </summary>
		/// <remarks>
		/// The entities will be completely independent from each other and linetypes and layers will only have it's name set, all the other properties will be set as default
		/// </remarks>
		/// <returns></returns>
		public List<Entity> ReadEntities()
		{
			this._reader = this._reader ?? this.getReader();

			this._builder = new DxfDocumentBuilder(this._version, this._document, this.Configuration);
			this._builder.OnNotification += this.onNotificationEvent;

			this.readEntities();

			return this._builder.BuildEntities();
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			base.Dispose();

			if (this.Configuration.ClearCache)
			{
				DxfMap.ClearCache();
			}
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
			while (this._reader.ValueAsString != DxfFileToken.EndSection)
			{
				if (this._reader.ValueAsString == DxfFileToken.ClassEntry)
				{
					var dxfClass = this.readClass();

					if(dxfClass.ClassNumber < 500)
					{
						dxfClass.ClassNumber = (short)(500 + classes.Count);
					}

					classes.AddOrUpdate(dxfClass);
				}
				else
					this._reader.ReadNext();
			}

			return classes;
		}

		private DxfClass readClass()
		{
			DxfClass curr = new DxfClass();

			Debug.Assert(this._reader.ValueAsString == DxfFileToken.ClassEntry);

			this._reader.ReadNext();
			//Loop until the next class or the end of the section
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					//Class DXF record name; always unique
					case 1:
						curr.DxfName = this._reader.ValueAsString;
						break;
					//C++ class name. Used to bind with software that defines object class behavior; always unique
					case 2:
						curr.CppClassName = this._reader.ValueAsString;
						break;
					//Application name. Posted in Alert box when a class definition listed in this section is not currently loaded
					case 3:
						curr.ApplicationName = this._reader.ValueAsString;
						break;
					//Proxy capabilities flag.
					case 90:
						curr.ProxyFlags = (ProxyFlags)this._reader.ValueAsUShort;
						break;
					//Instance count for a custom class
					case 91:
						curr.InstanceCount = this._reader.ValueAsInt;
						break;
					//Was-a-proxy flag. Set to 1 if class was not loaded when this DXF file was created, and 0 otherwise
					case 280:
						curr.WasZombie = this._reader.ValueAsBool;
						break;
					//Is - an - entity flag.
					case 281:
						curr.IsAnEntity = this._reader.ValueAsBool;
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

		private void readTables()
		{
			//Get the needed handler
			this._reader = this.goToSection(DxfFileToken.TablesSection);

			DxfTablesSectionReader reader = new DxfTablesSectionReader(this._reader, this._builder);

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

			DxfBlockSectionReader reader = new DxfBlockSectionReader(this._reader, this._builder);

			reader.Read();
		}

		/// <summary>
		/// Read the ENTITIES section of the DXF file.
		/// </summary>
		private void readEntities()
		{
			//Get the needed handler
			this._reader = this.goToSection(DxfFileToken.EntitiesSection);

			DxfEntitiesSectionReader reader = new DxfEntitiesSectionReader(this._reader, this._builder);

			reader.Read();
		}

		/// <summary>
		/// Read the OBJECTS section of the DXF file.
		/// </summary>
		private void readObjects()
		{
			//Get the needed handler
			this._reader = this.goToSection(DxfFileToken.ObjectsSection);

			DxfObjectsSectionReader reader = new DxfObjectsSectionReader(this._reader, this._builder);

			reader.Read();
		}

		/// <summary>
		/// Read the THUMBNAILIMAGE section of the DXF file.
		/// </summary>
		private void readThumbnailImage()
		{
			throw new NotImplementedException();
		}

		private IDxfStreamReader getReader()
		{
			IDxfStreamReader tmpReader = null;
			this._version = ACadVersion.Unknown;

			bool isBinary = IsBinary(this._fileStream.Stream, false);
			bool isAC1009Format = false;

			if (isBinary && this._fileStream.Stream.ReadByte() != -1)
			{
				int flag = this._fileStream.ReadByte();
				if (flag != -1 && flag != 0)
				{
					isAC1009Format = true;
				}
			}

			tmpReader = this.createReader(isBinary, isAC1009Format);

			if (!tmpReader.Find(DxfFileToken.HeaderSection))
			{
				this.triggerNotification($"Header section not found, using a generic reader.", NotificationType.Warning);

				this._version = ACadVersion.Unknown;
				tmpReader.Start();
				return tmpReader;
			}

			while (tmpReader.ValueAsString != DxfFileToken.EndSection)
			{
				if (tmpReader.ValueAsString == "$ACADVER")
				{
					tmpReader.ReadNext();
					this._version = CadUtils.GetVersionFromName(tmpReader.ValueAsString);
					if (this._version >= ACadVersion.AC1021)
					{
						this._encoding = Encoding.UTF8;
						break;
					}

					if (this._version < ACadVersion.AC1002)
					{
						if (this._version == ACadVersion.Unknown)
						{
							throw new CadNotSupportedException();
						}
						else
						{
							throw new CadNotSupportedException(this._version);
						}
					}
				}
				else if (tmpReader.ValueAsString == "$DWGCODEPAGE")
				{
					tmpReader.ReadNext();

					string encoding = tmpReader.ValueAsString;

					CodePage code = CadUtils.GetCodePage(encoding.ToLower());
					this._encoding = this.getListedEncoding((int)code);
				}

				tmpReader.ReadNext();
			}

			if(this._version == ACadVersion.Unknown)
			{
				this.triggerNotification($"Dxf version not found, using a generic reader.", NotificationType.Warning);
			}

			return this.createReader(isBinary, isAC1009Format);
		}

		private IDxfStreamReader goToSection(string sectionName)
		{
			//Get the needed handler
			this._reader = this._reader ?? this.getReader();

			if (this._reader.ValueAsString == sectionName)
				return this._reader;

			//Go to the start of header section
			this._reader.Find(sectionName);

			return this._reader;
		}

		private IDxfStreamReader createReader(bool isBinary, bool isAC1009Format)
		{
			Encoding encoding = this._encoding;
			if (encoding == null)
			{
				encoding = Encoding.ASCII;
			}

			if (isBinary)
			{
				if (isAC1009Format)
				{
					return new DxfBinaryReaderAC1009(this._fileStream.Stream, encoding);
				}
				else
				{
					return new DxfBinaryReader(this._fileStream.Stream, encoding);
				}
			}
			else
			{
				return new DxfTextReader(this._fileStream.Stream, encoding);
			}
		}
	}
}