using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACadSharp.Entities.TableEntity;

namespace ACadSharp.Objects
{
	public class TableContent : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectTableContent;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.TableContent;

		public string Description { get; set; }

		public CellStyle CellStyleOverride { get; set; } = new();

		public List<CellRange> MergedCellRanges { get; set; } = new();
	}
}
