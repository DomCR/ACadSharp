#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion
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
		public BlockRecordsTable BlockRecords { get; } = new BlockRecordsTable();
		public DimensionStylesTable DimensionStyles { get; } = new DimensionStylesTable();
		public LayersTable Layers { get; } = new LayersTable();
		public LineTypesTable LineTypes { get; } = new LineTypesTable();
		public StylesTable Styles { get; } = new StylesTable();
		public UCSTable UCSs { get; } = new UCSTable();
		public ViewsTable Views { get; } = new ViewsTable();
		public VPortsTable VPorts { get; } = new VPortsTable();

		public List<Layout> Layouts { get; } = new List<Layout>(); //TODO: replace for a collection or a table

		public object Blocks { get; set; }
		public object Objects { get; set; }
		public object ThumbnailImage { get; set; }

		//TODO: Implement entity collection to store the document's entities
		private Dictionary<ulong, Entity> _entities { get; set; } = new Dictionary<ulong, Entity>();
		//Contains all the objects in the document.
		private readonly Dictionary<ulong, CadObject> _cadObjects = new Dictionary<ulong, CadObject>();

		public CadDocument()
		{
			AppIds = new AppIdsTable();
		}

		[Obsolete]
		internal void AddEntity(Entity entity)
		{
			_entities.Add(entity.Handle, entity);
		}
		public void AddObject(CadObject cadObject)
		{
			//TODO: Assign the objects to each table or have an Add method foreach type?

			if (cadObject.GetType().IsEquivalentTo(typeof(Layer)))
			{
				Layers.Add((Layer)cadObject);
			}

			throw new NotImplementedException();
		}
		private void assignHandle(CadObject cadObject)
		{
			ulong value = _cadObjects.Keys.Max() + 1;
			cadObject.Handle = value;
		}

		internal void AddBlock(Block block, bool addEntities)
		{

		}
	}
}
