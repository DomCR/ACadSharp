using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Tables;
using System;
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
		public IEnumerable<Sorter> Sorters { get { return this._sorters; } }

		private List<Sorter> _sorters = new();

		internal SortEntitiesTable()
		{
			this.Name = DictionaryEntryName;
		}

		internal SortEntitiesTable(BlockRecord owner) : this()
		{
			this.BlockOwner = owner;
		}

		/// <summary>
		/// Sorter attached to an entity.
		/// </summary>
		/// <param name="entity">Enity in the block to be sorted.</param>
		/// <param name="sorterHandle">Sorter handle, will use the entity handle if null.</param>
		/// <exception cref="ArgumentException"></exception>
		public void AddEntity(Entity entity, ulong? sorterHandle = null)
		{
			if (entity.Owner != this.BlockOwner)
			{
				throw new ArgumentException($"Entity is not owned by the block {this.BlockOwner.Name}", nameof(entity));
			}

			this._sorters.Add(new Sorter(entity, sorterHandle));
		}

		internal void OnAddEntity(object sender, CollectionChangedEventArgs e)
		{
			this._sorters.Add(new Sorter((Entity)e.Item));
		}
	}
}
