using ACadSharp.Classes;
using ACadSharp.Header;
using ACadSharp.IO.DXF;
using CSUtilities.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ACadSharp.IO
{
	public class DxfReader : CadReaderBase
	{
		public DxfReaderConfiguration Configuration { get; set; } = new DxfReaderConfiguration();

		private CadDocument _document;
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
			this._document = new CadDocument(false);
			this._builder = new DxfDocumentBuilder(this._document, this.Configuration);
			this._builder.OnNotification += this.onNotificationEvent;

			this._reader = this._reader ?? this.getReader();

			while (this._reader.LastValueAsString != DxfFileToken.EndOfFile)
			{
				if (this._reader.LastValueAsString != DxfFileToken.BeginSection)
				{
					this._reader.ReadNext();
					continue;
				}
				else
				{
					this._reader.ReadNext();
				}

				switch (this._reader.LastValueAsString)
				{
					case DxfFileToken.HeaderSection:
						this._document.Header = this.ReadHeader();
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
						this.triggerNotification(($"Section not implemented {this._reader.LastValueAsString}"), NotificationType.NotImplemented);
						break;
				}

				this._reader.ReadNext();
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
			while (this._reader.LastValueAsString != DxfFileToken.EndSection)
			{
				//Get the current header variable
				string currVar = this._reader.LastValueAsString;

				if (this._reader.LastValueAsString == null || !headerMap.TryGetValue(currVar, out var data))
				{
					//this.OnNotificationHandler?.Invoke(this, new NotificationEventArgs($"Header variable not implemented {currVar}"));
					this._reader.ReadNext();
					continue;
				}

				object[] parameters = new object[data.DxfCodes.Length];
				for (int i = 0; i < data.DxfCodes.Length; i++)
				{
					this._reader.ReadNext();

					parameters[i] = this._reader.LastValue;
				}

				//Set the header value by name
				header.SetValue(currVar, parameters);

				this._reader.ReadNext();
			}

			return header;
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			base.Dispose();

			if (this.Configuration.ClearChache)
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
			while (this._reader.LastValueAsString != DxfFileToken.EndSection)
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
						curr.WasZombie = this._reader.LastValueAsBool;
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
		private void ReadThumbnailImage()
		{
			throw new NotImplementedException();
		}

		private IDxfStreamReader getReader()
		{
			if (this.IsBinary())
			{
				return new DxfBinaryReader(this._fileStream.Stream, Encoding.ASCII);
			}
			else
				return new DxfTextReader(this._fileStream.Stream);

			//TODO: Setup encoding
			//AutoCAD 2007 DXF and later format - UTF-8
			//AutoCAD 2004 DXF and earlier format - Plain ASCII and CIF
		}

		private IDxfStreamReader goToSection(string sectionName)
		{
			//Get the needed handler
			this._reader = this._reader ?? this.getReader();

			if (this._reader.LastValueAsString == sectionName)
				return this._reader;

			//Go to the start of header section
			this._reader.Find(sectionName);

			return this._reader;
		}
	}
}