using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.Header;
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
		public CadHeader Header { get; set; }

		public CadSummaryInfo SummaryInfo { get; set; }

		public DxfClassCollection Classes { get; set; }

		public AppIdsTable AppIds { get; set; }
		public BlockRecordsTable BlockRecords { get; set; }
		public DimensionStylesTable DimensionStyles { get; set; }
		public LayersTable Layers { get; set; } = new LayersTable();
		public LineTypesTable LineTypes { get; set; }
		public StylesTable Styles { get; set; }
		public UCSTable UCSs { get; set; }
		public ViewsTable Views { get; set; }
		public ViewPortsTable ViewPorts { get; set; }

		public object Blocks { get; set; }
		public object Objects { get; set; }
		public object ThumbnailImage { get; set; }

		//TODO: Implement entity collection to store the document's entities
		private Dictionary<ulong, Entity> _entities { get; set; } = new Dictionary<ulong, Entity>();
		private readonly Dictionary<ulong, CadObject> _cadObjects = new Dictionary<ulong, CadObject>();

		internal void AddEntity(Entity entity)
		{
			_entities.Add(entity.Handle, entity);
		}
		public void AddObject(CadObject cadObject)
		{
			throw new NotImplementedException();
		}
		private void assignHandle(CadObject cadObject)
		{
			ulong value = _cadObjects.Keys.Max() + 1;
			cadObject.Handle = value;
		}
	}
}
