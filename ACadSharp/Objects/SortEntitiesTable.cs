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
	public class SortEntitiesTable : NonGraphicalObject
	{
		public const string DictionaryEntryName = "ACAD_SORTENTS";

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectSortEntsTable;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.SortentsTable;

		/// <summary>
		/// Block owner where the table is applied
		/// </summary>
		[DxfCodeValue(330)]
		public BlockRecord BlockOwner { get; internal set; }

		/// <summary>
		/// List of the <see cref="BlockOwner"/> entities sorted.
		/// </summary>
		public List<Sorter> Sorters { get; } = new List<Sorter>();

		/// <summary>
		/// Default constructor.
		/// </summary>
		public SortEntitiesTable()
		{
			this.Name = DictionaryEntryName;
		}

		/// <summary>
		/// Entity sorter based in their position in the collection.
		/// </summary>
		public class Sorter : IHandledCadObject
		{
			/// <inheritdoc/>
			[DxfCodeValue(5)]
			public ulong Handle { get; internal set; }

			/// <summary>
			/// Soft-pointer ID/handle to an entity
			/// </summary>
			[DxfCodeValue(331)]
			public Entity Entity { get; set; }
		}
	}
}
