using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Tables;
using System;
using System.Collections;
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
	public partial class SortEntitiesTable : NonGraphicalObject, IEnumerable<SortEntitiesTable.Sorter>
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
		/// <param name="entity">Entity in the block to be sorted.</param>
		/// <param name="sorterHandle">Sorter handle.</param>
		/// <exception cref="ArgumentException"></exception>
		public void Add(Entity entity, ulong sorterHandle)
		{
			this._sorters.Add(new Sorter(entity, sorterHandle));
		}

		/// <summary>
		/// Removes all elements in the collection.
		/// </summary>
		public void Clear()
		{
			this._sorters.Clear();
		}

		/// <inheritdoc/>
		public IEnumerator<Sorter> GetEnumerator()
		{
			this._sorters.Sort();
			return this._sorters.GetEnumerator();
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
