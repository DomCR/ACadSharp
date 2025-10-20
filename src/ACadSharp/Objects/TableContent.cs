using ACadSharp.Attributes;
using System.Collections.Generic;
using static ACadSharp.Entities.TableEntity;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="TableContent"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectTableContent"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.TableLinkedTableData"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectTableContent)]
	[DxfSubClass(DxfSubclassMarker.TableLinkedTableData)]
	public class TableContent : LinkedData
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectTableContent;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.TableLinkedTableData;

		public CellStyle CellStyleOverride { get; set; } = new();

		public List<CellRange> MergedCellRanges { get; set; } = new();
	}
}
