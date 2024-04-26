﻿using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.Header;
using ACadSharp.Objects;
using ACadSharp.Objects.Collections;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp
{
	/// <summary>
	/// A CAD drawing
	/// </summary>
	public class CadDocument : IHandledCadObject
	{
		/// <summary>
		/// The document handle is always 0, this field makes sure that no object overrides this value
		/// </summary>
		public ulong Handle { get { return 0; } }

		/// <summary>
		/// Contains all the header variables for this document.
		/// </summary>
		public CadHeader Header { get; internal set; }

		/// <summary>
		/// Accesses drawing properties such as the Title, Subject, Author, and Keywords properties
		/// </summary>
		public CadSummaryInfo SummaryInfo { get; set; }

		/// <summary>
		/// Dxf classes defined in this document
		/// </summary>
		public DxfClassCollection Classes { get; set; } = new DxfClassCollection();

		/// <summary>
		/// The collection of all registered applications in the drawing
		/// </summary>
		public AppIdsTable AppIds { get; private set; }

		/// <summary>
		/// The collection of all block records in the drawing
		/// </summary>
		public BlockRecordsTable BlockRecords { get; private set; }

		/// <summary>
		/// The collection of all dimension styles in the drawing
		/// </summary>
		public DimensionStylesTable DimensionStyles { get; private set; }

		/// <summary>
		/// The collection of all layers in the drawing
		/// </summary>
		public LayersTable Layers { get; private set; }

		/// <summary>
		/// The collection of all linetypes in the drawing
		/// </summary>
		public LineTypesTable LineTypes { get; private set; }

		/// <summary>
		/// The collection of all text styles in the drawing
		/// </summary>
		public TextStylesTable TextStyles { get; private set; }

		/// <summary>
		/// The collection of all user coordinate systems (UCSs) in the drawing
		/// </summary>
		public UCSTable UCSs { get; private set; }

		/// <summary>
		/// The collection of all views in the drawing
		/// </summary>
		public ViewsTable Views { get; private set; }

		/// <summary>
		/// The collection of all vports in the drawing
		/// </summary>
		public VPortsTable VPorts { get; private set; }

		/// <summary>
		/// The collection of all layouts in the drawing.
		/// </summary>
		/// <remarks>
		/// The collection is null if the <see cref="CadDictionary.AcadLayout"/> doesn't exist in the root dictionary.
		/// </remarks>
		public LayoutCollection Layouts { get; private set; }

		/// <summary>
		/// The collection of all groups in the drawing. 
		/// </summary>
		/// <remarks>
		/// The collection is null if the <see cref="CadDictionary.AcadGroup"/> doesn't exist in the root dictionary.
		/// </remarks>
		public GroupCollection Groups { get; private set; }

		/// <summary>
		/// The collection of all scales in the drawing. 
		/// </summary>
		/// <remarks>
		/// The collection is null if the <see cref="CadDictionary.AcadScaleList"/> doesn't exist in the root dictionary.
		/// </remarks>
		public ScaleCollection Scales { get; private set; }

		/// <summary>
		/// The collection of all Multi line styles in the drawing. 
		/// </summary>
		/// <remarks>
		/// The collection is null if the <see cref="CadDictionary.AcadMLineStyle"/> doesn't exist in the root dictionary.
		/// </remarks>
		public MLineStyleCollection MLineStyles { get; private set; }

		/// <summary>
		/// The collection of all Multi leader styles in the drawing. 
		/// </summary>
		/// <remarks>
		/// The collection is null if the <see cref="CadDictionary.AcadMLeaderStyle"/> doesn't exist in the root dictionary.
		/// </remarks>
		public MLeaderStyleCollection MLeaderStyles { get; private set; }

		/// <summary>
		/// Root dictionary of the document
		/// </summary>
		public CadDictionary RootDictionary
		{
			get { return this._rootDictionary; }
			internal set
			{
				this._rootDictionary = value;
				this._rootDictionary.Owner = this;
				this.RegisterCollection(this._rootDictionary);
			}
		}

		/// <summary>
		/// Collection with all the entities in the drawing
		/// </summary>
		public CadObjectCollection<Entity> Entities { get { return this.ModelSpace.Entities; } }

		/// <summary>
		/// Model space block record containing the drawing
		/// </summary>
		public BlockRecord ModelSpace { get { return this.BlockRecords[BlockRecord.ModelSpaceName]; } }

		/// <summary>
		/// Default paper space of the model
		/// </summary>
		public BlockRecord PaperSpace { get { return this.BlockRecords[BlockRecord.PaperSpaceName]; } }

		private CadDictionary _rootDictionary = null;

		//Contains all the objects in the document
		private readonly Dictionary<ulong, IHandledCadObject> _cadObjects = new Dictionary<ulong, IHandledCadObject>();

		internal CadDocument(bool createDefaults)
		{
			this._cadObjects.Add(this.Handle, this);

			if (createDefaults)
			{
				DxfClassCollection.UpdateDxfClasses(this);

				//Header and summary
				this.Header = new CadHeader(this);
				this.SummaryInfo = new CadSummaryInfo();

				//The order of the elements is rellevant for the handles assignation

				//Initialize tables
				this.BlockRecords = new BlockRecordsTable(this);
				this.Layers = new LayersTable(this);
				this.DimensionStyles = new DimensionStylesTable(this);
				this.TextStyles = new TextStylesTable(this);
				this.LineTypes = new LineTypesTable(this);
				this.Views = new ViewsTable(this);
				this.UCSs = new UCSTable(this);
				this.VPorts = new VPortsTable(this);
				this.AppIds = new AppIdsTable(this);

				//Root dictionary
				this.RootDictionary = CadDictionary.CreateRoot();

				//Entries
				Layout modelLayout = Layout.Default;
				Layout paperLayout = new Layout("Layout1");
				(this.RootDictionary[CadDictionary.AcadLayout] as CadDictionary).Add(paperLayout);
				(this.RootDictionary[CadDictionary.AcadLayout] as CadDictionary).Add(modelLayout);

				//Default variables
				this.AppIds.Add(AppId.Default);

				this.LineTypes.Add(LineType.ByLayer);
				this.LineTypes.Add(LineType.ByBlock);
				this.LineTypes.Add(LineType.Continuous);

				this.Layers.Add(Layer.Default);

				this.TextStyles.Add(TextStyle.Default);

				this.DimensionStyles.Add(DimensionStyle.Default);

				this.VPorts.Add(VPort.Default);

				//Blocks
				BlockRecord model = BlockRecord.ModelSpace;
				model.Layout = modelLayout;
				this.BlockRecords.Add(model);

				BlockRecord pspace = BlockRecord.PaperSpace;
				pspace.Layout = paperLayout;
				this.BlockRecords.Add(pspace);

				this.UpdateCollections(false);
			}
		}

		/// <summary>
		/// Creates a document with the default objects
		/// </summary>
		/// <remarks>
		/// Default version <see cref="ACadVersion.AC1018"/>
		/// </remarks>
		public CadDocument() : this(ACadVersion.AC1018) { }

		/// <summary>
		/// Creates a document with the default objects and a specific version
		/// </summary>
		/// <param name="version">Version of the document</param>
		public CadDocument(ACadVersion version) : this(true)
		{
			this.Header.Version = version;
		}

		/// <summary>
		/// Gets an object in the document by it's handle
		/// </summary>
		/// <param name="handle"></param>
		/// <returns>the cadObject or null if doesn't exists in the document</returns>
		public CadObject GetCadObject(ulong handle)
		{
			return this.GetCadObject<CadObject>(handle);
		}

		/// <summary>
		/// Gets an object in the document by it's handle
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="handle"></param>
		/// <returns>the cadObject or null if doesn't exists in the document</returns>
		public T GetCadObject<T>(ulong handle)
			where T : CadObject
		{
			if (this._cadObjects.TryGetValue(handle, out IHandledCadObject obj))
			{
				return obj as T;
			}

			return null;
		}

		/// <summary>
		/// Gets an object in the document by it's handle
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="handle"></param>
		/// <param name="cadObject"></param>
		/// <returns></returns>
		public bool TryGetCadObject<T>(ulong handle, out T cadObject)
			where T : CadObject
		{
			cadObject = null;

			if (handle == this.Handle)
				return false;

			if (this._cadObjects.TryGetValue(handle, out IHandledCadObject obj))
			{
				cadObject = obj as T;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Updates the collections in the document and link them to it's dictionary
		/// </summary>
		/// <param name="createDictionaries"></param>
		public void UpdateCollections(bool createDictionaries)
		{
			if (createDictionaries && this.RootDictionary == null)
			{
				this.RootDictionary = CadDictionary.CreateRoot();
			}
			else if (this.RootDictionary == null)
			{
				return;
			}

			if (this.updateCollection(CadDictionary.AcadLayout, createDictionaries, out CadDictionary layout))
			{
				this.Layouts = new LayoutCollection(layout);
			}

			if (this.updateCollection(CadDictionary.AcadGroup, createDictionaries, out CadDictionary groups))
			{
				this.Groups = new GroupCollection(groups);
			}

			if (this.updateCollection(CadDictionary.AcadScaleList, createDictionaries, out CadDictionary scales))
			{
				this.Scales = new ScaleCollection(scales);
			}

			if (this.updateCollection(CadDictionary.AcadMLineStyle, createDictionaries, out CadDictionary mlineStyles))
			{
				this.MLineStyles = new MLineStyleCollection(mlineStyles);
			}

			if (this.updateCollection(CadDictionary.AcadMLineStyle, createDictionaries, out CadDictionary mleaderStyles))
			{
				this.MLeaderStyles = new MLeaderStyleCollection(mleaderStyles);
			}
		}

		private bool updateCollection(string dictName, bool createDictionary, out CadDictionary dictionary)
		{
			if (this.RootDictionary.TryGetEntry(dictName, out dictionary))
			{
				return true;
			}
			else if (createDictionary)
			{
				this.RootDictionary.Add(new CadDictionary(dictName));
			}

			return dictionary != null;
		}

		private void addCadObject(CadObject cadObject)
		{
			if (cadObject.Document != null)
			{
				throw new ArgumentException($"The item with handle {cadObject.Handle} is already assigned to a document");
			}

			if (cadObject.Handle == 0 || this._cadObjects.ContainsKey(cadObject.Handle))
			{
				var nextHandle = this._cadObjects.Keys.Max() + 1;

				this.Header.HandleSeed = nextHandle + 1;

				cadObject.Handle = nextHandle;
			}

			this._cadObjects.Add(cadObject.Handle, cadObject);

			if (cadObject is BlockRecord record)
			{
				this.addCadObject(record.BlockEntity);
				this.addCadObject(record.BlockEnd);
			}

			cadObject.AssignDocument(this);
		}

		private void removeCadObject(CadObject cadObject)
		{
			if (!this.TryGetCadObject(cadObject.Handle, out CadObject _)
				|| !this._cadObjects.Remove(cadObject.Handle))
			{
				return;
			}

			cadObject.UnassignDocument();
		}

		private void onAdd(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item is CadDictionary dictionary)
			{
				this.RegisterCollection(dictionary);
			}
			else
			{
				this.addCadObject(e.Item);
			}
		}

		private void onRemove(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item is CadDictionary dictionary)
			{
				this.UnregisterCollection(dictionary);
			}
			else
			{
				this.removeCadObject(e.Item);
			}
		}

		internal void RegisterCollection<T>(IObservableCollection<T> collection)
			where T : CadObject
		{
			switch (collection)
			{
				case AppIdsTable:
					this.AppIds = (AppIdsTable)collection;
					this.AppIds.Owner = this;
					break;
				case BlockRecordsTable:
					this.BlockRecords = (BlockRecordsTable)collection;
					this.BlockRecords.Owner = this;
					break;
				case DimensionStylesTable:
					this.DimensionStyles = (DimensionStylesTable)collection;
					this.DimensionStyles.Owner = this;
					break;
				case LayersTable:
					this.Layers = (LayersTable)collection;
					this.Layers.Owner = this;
					break;
				case LineTypesTable:
					this.LineTypes = (LineTypesTable)collection;
					this.LineTypes.Owner = this;
					break;
				case TextStylesTable:
					this.TextStyles = (TextStylesTable)collection;
					this.TextStyles.Owner = this;
					break;
				case UCSTable:
					this.UCSs = (UCSTable)collection;
					this.UCSs.Owner = this;
					break;
				case ViewsTable:
					this.Views = (ViewsTable)collection;
					this.Views.Owner = this;
					break;
				case VPortsTable:
					this.VPorts = (VPortsTable)collection;
					this.VPorts.Owner = this;
					break;
			}

			collection.OnAdd += this.onAdd;
			collection.OnRemove += this.onRemove;

			if (collection is CadObject cadObject)
			{
				this.addCadObject(cadObject);
			}

			if (collection is ISeqendCollection seqendColleciton)
			{
				seqendColleciton.OnSeqendAdded += this.onAdd;
				seqendColleciton.OnSeqendRemoved += this.onRemove;

				if (seqendColleciton.Seqend != null)
				{
					this.addCadObject(seqendColleciton.Seqend);
				}
			}

			foreach (T item in collection)
			{
				if (item is CadDictionary dictionary)
				{
					this.RegisterCollection(dictionary);
				}
				else
				{
					this.addCadObject(item);
				}
			}
		}

		internal void UnregisterCollection<T>(IObservableCollection<T> collection)
			where T : CadObject
		{
			switch (collection)
			{
				case AppIdsTable:
				case BlockRecordsTable:
				case DimensionStylesTable:
				case LayersTable:
				case LineTypesTable:
				case TextStylesTable:
				case UCSTable:
				case ViewsTable:
				case VPortsTable:
					throw new InvalidOperationException($"The collection {collection.GetType()} cannot be removed from a document.");
			}

			collection.OnAdd -= this.onAdd;
			collection.OnRemove -= this.onRemove;

			if (collection is CadObject cadObject)
			{
				this.removeCadObject(cadObject);
			}

			if (collection is ISeqendCollection seqendColleciton)
			{
				seqendColleciton.OnSeqendAdded -= this.onAdd;
				seqendColleciton.OnSeqendRemoved -= this.onRemove;

				if (seqendColleciton.Seqend != null)
				{
					this.removeCadObject(seqendColleciton.Seqend);
				}
			}

			foreach (T item in collection)
			{
				if (item is CadDictionary dictionary)
				{
					this.UnregisterCollection(dictionary);
				}
				else
				{
					this.removeCadObject(item);
				}
			}
		}
	}
}
