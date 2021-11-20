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
	//Visual reference of the structure
	//https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-A809CD71-4655-44E2-B674-1FE200B9FE30
	public class CadDocument
	{
		/// <summary>
		/// Contains all the header variables for this document.
		/// </summary>
		public CadHeader Header { get; set; }

		public CadSummaryInfo SummaryInfo { get; set; }

		public DxfClassCollection Classes { get; set; }

		public AppIdsTable AppIds { get; }
		public BlockRecordsTable BlockRecords { get; }
		public DimensionStylesTable DimensionStyles { get; }
		public LayersTable Layers { get; }
		public LineTypesTable LineTypes { get; }
		public StylesTable Styles { get; }
		public UCSTable UCSs { get; }
		public ViewsTable Views { get; }
		public VPortsTable VPorts { get; }

		public LayoutsTable Layouts { get; }

		public CadDictionary RootDictionary { get; } = new CadDictionary();

		//TODO: Implement entity collection to store the document's entities
		private Dictionary<ulong, Entity> _entities { get; set; } = new Dictionary<ulong, Entity>();
		//Contains all the objects in the document.
		internal readonly Dictionary<ulong, CadObject> cadObjects = new Dictionary<ulong, CadObject>();

		public CadDocument()
		{
			//Header and summary
			this.Header = new CadHeader();
			this.SummaryInfo = new CadSummaryInfo();

			//Initialize tables
			this.AppIds = new AppIdsTable(this);
			this.BlockRecords = new BlockRecordsTable(this);
			this.DimensionStyles = new DimensionStylesTable(this);
			this.Layers = new LayersTable(this);
			this.LineTypes = new LineTypesTable(this);
			this.Styles = new StylesTable(this);
			this.UCSs = new UCSTable(this);
			this.Views = new ViewsTable(this);
			this.VPorts = new VPortsTable(this);
			this.Layouts = new LayoutsTable(this);
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

			if (cadObject.Handle == 0 || cadObjects.ContainsKey(cadObject.Handle))
			{
				cadObject.Handle = cadObjects.Keys.Max() + 1;
			}

			cadObjects.Add(cadObject.Handle, cadObject);
		}

		private void addCadObject(CadObject cadObject)
		{
			//if (cadObject.Handle == 0 || cadObjects.ContainsKey(cadObject.Handle))
			//{
			//	cadObject.Handle = cadObjects.Keys.Max() + 1;
			//}

			//cadObjects.Add(cadObject.Handle, cadObject);
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
		}
	}
}
