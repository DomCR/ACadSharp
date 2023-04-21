using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Tables;
using System.Collections.Generic;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="SortEntitiesTable"/> object
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectSortEntsTable"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.SortentsTable"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectSortEntsTable)]
	[DxfSubClass(DxfSubclassMarker.SortentsTable)]
	public class SortEntitiesTable : CadObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectSortEntsTable;

		/// <summary>
		/// Block owner where the table is applied
		/// </summary>
		[DxfCodeValue(330)]
		public BlockRecord BlockOwner { get; internal set; }

		public List<Sorter> Sorters { get; } = new List<Sorter>();

		public class Sorter
		{
			/// <summary>
			/// Sort handle
			/// </summary>
			[DxfCodeValue(5)]
			public ulong SortHandle { get; set; }

			/// <summary>
			/// Soft-pointer ID/handle to an entity
			/// </summary>
			[DxfCodeValue(331)]
			public Entity Entity { get; set; }

			public Sorter() { }
		}
	}
}
