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
	/// Dxf class name <see cref="DxfSubclassMarker.TableContent"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectTableContent)]
	[DxfSubClass(DxfSubclassMarker.TableContent)]
	public class TableContent : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectTableContent;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.TableContent;

		[DxfCodeValue(1)]
		public override string Name { get => base.Name; set => base.Name = value; }

		[DxfCodeValue(300)]
		public string Description { get; set; }

		public CellStyle CellStyleOverride { get; set; } = new();

		public List<CellRange> MergedCellRanges { get; set; } = new();
	}
}
