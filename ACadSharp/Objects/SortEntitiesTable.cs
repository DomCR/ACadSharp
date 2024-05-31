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
	public partial class SortEntitiesTable : NonGraphicalObject
	{
		/// <summary>
		/// Dictionary entry name for the object <see cref="SortEntitiesTable"/>
		/// </summary>
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
		public List<Sorter> Sorters { get; }

		internal SortEntitiesTable()
		{
			this.Name = DictionaryEntryName;
		}

		internal SortEntitiesTable(BlockRecord owner) : this()
		{
			this.BlockOwner = owner;
			this.BlockOwner.Entities.OnAdd += this.OnAddEntity;
		}

		internal void OnAddEntity(object sender, CollectionChangedEventArgs e)
		{
			this.Sorters.Add(new Sorter
			{
				Entity = (Entity)e.Item
			});
		}
	}
}
