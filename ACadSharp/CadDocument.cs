using ACadSharp.Blocks;
using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.Header;
using ACadSharp.Objects;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp
{
	/// <summary>
	/// An AutoCAD drawing
	/// </summary>
	public class CadDocument : CadObject	//TODO: Remove the CadObject hineritance
	{
		public override ObjectType ObjectType => ObjectType.INVALID;

		/// <summary>
		/// Contains all the header variables for this document.
		/// </summary>
		public CadHeader Header { get; set; }

		/// <summary>
		/// Accesses drawing properties such as the Title, Subject, Author, and Keywords properties
		/// </summary>
		public CadSummaryInfo SummaryInfo { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DxfClassCollection Classes { get; set; }

		/// <summary>
		/// The collection of all registered applications in the drawing
		/// </summary>
		public AppIdsTable AppIds { get; }

		/// <summary>
		/// The collection of all block records in the drawing
		/// </summary>
		public BlockRecordsTable BlockRecords { get; }

		/// <summary>
		/// The collection of all dimension styles in the drawing
		/// </summary>
		public DimensionStylesTable DimensionStyles { get; }

		/// <summary>
		/// The collection of all layers in the drawing
		/// </summary>
		public LayersTable Layers { get; }

		/// <summary>
		/// The collection of all linetypes in the drawing
		/// </summary>
		public LineTypesTable LineTypes { get; }

		/// <summary>
		/// The collection of all text styles in the drawing
		/// </summary>
		public TextStylesTable TextStyles { get; }

		/// <summary>
		/// The collection of all user coordinate systems (UCSs) in the drawing
		/// </summary>
		public UCSTable UCSs { get; }

		/// <summary>
		/// The collection of all views in the drawing
		/// </summary>
		public ViewsTable Views { get; }

		/// <summary>
		/// 
		/// </summary>
		public VPortsTable VPorts { get; }

		/// <summary>
		/// The collection of all layouts in the drawing
		/// </summary>
		public LayoutCollection Layouts { get; }

		/// <summary>
		/// The collection of all viewports in the drawing
		/// </summary>
		public ViewportCollection Viewports { get; }

		public CadDictionary RootDictionary { get; internal set; } = new CadDictionary();

		//TODO: Implement entity collection to store the document's entities
		private Dictionary<ulong, Entity> _entities { get; set; } = new Dictionary<ulong, Entity>();

		//Contains all the objects in the document
		private readonly Dictionary<ulong, CadObject> _cadObjects = new Dictionary<ulong, CadObject>();

		public CadDocument()
		{
			_cadObjects.Add(0, this);

			//Header and summary
			this.Header = new CadHeader();
			this.SummaryInfo = new CadSummaryInfo();

			//Initialize tables
			this.AppIds = new AppIdsTable(this);
			this.BlockRecords = new BlockRecordsTable(this);
			this.DimensionStyles = new DimensionStylesTable(this);
			this.Layers = new LayersTable(this);
			this.LineTypes = new LineTypesTable(this);
			this.TextStyles = new TextStylesTable(this);
			this.UCSs = new UCSTable(this);
			this.Views = new ViewsTable(this);
			this.VPorts = new VPortsTable(this);
			this.Layouts = new LayoutCollection(this);
			this.Viewports = new ViewportCollection(this);
		}

		public CadObject GetCadObject(ulong handle)
		{
			return this.GetCadObject<CadObject>(handle);
		}

		public T GetCadObject<T>(ulong handle)
			where T : CadObject
		{
			if (_cadObjects.TryGetValue(handle, out CadObject obj))
			{
				return obj as T;
			}

			return null;
		}

		[Obsolete]
		internal void AddEntity(Entity entity)
		{
			_entities.Add(entity.Handle, entity);
		}

		private void registerCadObject(CadObject cadObject)
		{
			if (cadObject.Document != null)
				throw new ArgumentException($"The item with handle {cadObject.Handle} is already assigned to a document");

			cadObject.Document = this;
		}

		private void addCadObject(CadObject cadObject)
		{
			if (cadObject.Handle == 0 || _cadObjects.ContainsKey(cadObject.Handle))
			{
				cadObject.Handle = _cadObjects.Keys.Max() + 1;
			}

			_cadObjects.Add(cadObject.Handle, cadObject);
		}

		private void onBeforeAdd(object sender, CollectionChangedEventArgs e)
		{
			registerCadObject(e.Item);
		}

		private void onAdd(object sender, CollectionChangedEventArgs e)
		{
			addCadObject(e.Item);
		}

		internal void RegisterCollection<T>(IObservableCollection<T> collection, bool addElements = true)
			where T : CadObject
		{
			collection.OnBeforeAdd += this.onBeforeAdd;
			collection.OnAdd += this.onAdd;

			if (addElements)
			{
				foreach (T item in collection)
				{
					registerCadObject(item);
					addCadObject(item);
				}
			}

			if (collection is CadObject cadObject)
			{
				addCadObject(cadObject);
			}
		}
	}
}
