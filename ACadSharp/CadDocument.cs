using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.Header;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;
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

		//TODO: Implement entity collection to store the document's entities
		private Dictionary<ulong, Entity> m_entities { get; set; } = new Dictionary<ulong, Entity>();

		public object Objects { get; set; }

		public object ThumbnailImage { get; set; }

		internal void AddEntity(Entity entity)
		{
			m_entities.Add(entity.Handle, entity);
		}
	}
}
