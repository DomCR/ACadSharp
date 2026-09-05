using ACadSharp.Attributes;
using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.Tables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Objects;

/// <summary>
/// Represents a <see cref="SortEntitiesTable"/> object
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectSortEntsTable"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.SortentsTable"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectSortEntsTable)]
[DxfSubClass(DxfSubclassMarker.SortentsTable)]
public partial class SortEntitiesTable : NonGraphicalObject, IDxfClassDefined, IEnumerable<SortEntitiesTable.Sorter>
{
	/// <summary>
	/// Block owner where the table is applied
	/// </summary>
	[DxfCodeValue(330)]
	public BlockRecord BlockOwner { get; internal set; }

	/// <summary>
	/// The owner exactly as the file stores it, which is not always a block record.
	/// </summary>
	/// <remarks>
	/// A drawing can hold sort tables whose owner is a dictionary: AutoCAD writes them, keeps them
	/// and audits such a file without a single error. <see cref="BlockOwner"/> can only hold a block
	/// record, so writing that back gave a null handle and AutoCAD then reported
	/// "AcDbSortEntsTable Block Id not valid" once per object. The writers use this reference so the
	/// file keeps what it came with.
	/// </remarks>
	internal CadObject BlockOwnerReference { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectSortEntsTable;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.UNLISTED;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.SortentsTable;

	/// <summary>
	/// Dictionary entry name for the object <see cref="SortEntitiesTable"/>
	/// </summary>
	public const string DictionaryEntryName = "ACAD_SORTENTS";

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
	public override CadObject Clone()
	{
		SortEntitiesTable clone = (SortEntitiesTable)base.Clone();

		clone._sorters = new List<Sorter>();

		return clone;
	}

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.SortentsTable,
			DwgVersion = ACadVersion.AC1014,
			DxfName = DxfFileToken.ObjectSortEntsTable,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		};
	}

	//The order the file gives this table is not the order it enumerates in, and it is worth keeping:
	//a table whose keys collide - AutoCAD reports 15 such entries in one production drawing - is read
	//by whichever entry comes first, so rewriting it in another order changes what the drawing means.
	//Sorting inside GetEnumerator did exactly that: reading the table reordered it, and the writer
	//then wrote that order back out. It cost that drawing an audit error it did not arrive with.
	internal IReadOnlyList<Sorter> StoredOrder
	{
		get { return this._sorters; }
	}

	/// <inheritdoc/>
	public IEnumerator<Sorter> GetEnumerator()
	{
		//Sorted, as before - but without reordering what is stored.
		return this._sorters.OrderBy(s => s.SortHandle).GetEnumerator();
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	/// <summary>
	/// Get the sorter handle of an entity, if is not in the sorter table it will return the entity's handle.
	/// </summary>
	/// <param name="entity"></param>
	/// <returns></returns>
	public ulong GetSorterHandle(Entity entity)
	{
		Sorter sorter = this._sorters.FirstOrDefault(s => s.Entity.Equals(entity));

		if (sorter is not null)
		{
			return sorter.SortHandle;
		}
		else
		{
			return entity.Handle;
		}
	}

	/// <summary>
	/// Moves the specified entity to the bottom of the draw order.
	/// </summary>
	/// <param name="entity">Entity to move to the bottom.</param>
	public void MoveToBottom(Entity entity)
	{
		ulong maxHandle = this._sorters.Count > 0
			? this._sorters.Max(s => s.SortHandle)
			: entity.Handle;

		ulong newHandle = maxHandle < ulong.MaxValue ? maxHandle + 1 : ulong.MaxValue;

		Sorter existing = this._sorters.FirstOrDefault(s => s.Entity.Equals(entity));
		if (existing is not null)
		{
			existing.SortHandle = newHandle;
		}
		else
		{
			this._sorters.Add(new Sorter(entity, newHandle));
		}
	}

	/// <summary>
	/// Moves the specified entity to the top of the draw order.
	/// </summary>
	/// <param name="entity">Entity to move to the top.</param>
	public void MoveToTop(Entity entity)
	{
		ulong minHandle = this._sorters.Count > 0
			? this._sorters.Min(s => s.SortHandle)
			: entity.Handle;

		ulong sorter = minHandle > 0 ? minHandle - 1 : 0;

		Sorter existing = this._sorters.FirstOrDefault(s => s.Entity.Equals(entity));
		if (existing is not null)
		{
			existing.SortHandle = sorter;
		}
		else
		{
			this._sorters.Add(new Sorter(entity, sorter));
		}
	}

	/// <summary>
	/// Moves the specified entity one step down in the draw order.
	/// If the entity is not in the table or is already at the bottom, no action is taken.
	/// </summary>
	/// <param name="entity">Entity to move one step down.</param>
	public void OneStepDown(Entity entity)
	{
		Sorter existing = this._sorters.FirstOrDefault(s => s.Entity.Equals(entity));
		if (existing is null)
		{
			return;
		}

		//A sorted view, not a sort in place: the stored order is what the file gave and is kept.
		List<Sorter> sorted = this._sorters.OrderBy(s => s.SortHandle).ToList();
		int index = sorted.IndexOf(existing);
		if (index < 0 || index >= sorted.Count - 1)
		{
			return;
		}

		Sorter next = sorted[index + 1];
		(existing.SortHandle, next.SortHandle) = (next.SortHandle, existing.SortHandle);
	}

	/// <summary>
	/// Moves the specified entity one step up in the draw order.
	/// If the entity is not in the table or is already at the top, no action is taken.
	/// </summary>
	/// <param name="entity">Entity to move one step up.</param>
	public void OneStepUp(Entity entity)
	{
		Sorter existing = this._sorters.FirstOrDefault(s => s.Entity.Equals(entity));
		if (existing is null)
		{
			return;
		}

		//The neighbour has to come from the same sorted view the index came from. Taking it from
		//the stored list swapped with whatever happened to sit there, which was only right while
		//enumeration sorted the list in place as a side effect.
		List<Sorter> sorted = this._sorters.OrderBy(s => s.SortHandle).ToList();
		int index = sorted.IndexOf(existing);
		if (index <= 0)
		{
			return;
		}

		Sorter previous = sorted[index - 1];
		(existing.SortHandle, previous.SortHandle) = (previous.SortHandle, existing.SortHandle);
	}

	/// <summary>
	/// Removes the first occurrence of a specific object from the sorters table.
	/// </summary>
	/// <param name="entity"></param>
	/// <returns></returns>
	public bool Remove(Entity entity)
	{
		var sorter = this._sorters.FirstOrDefault(s => s.Entity.Equals(entity));
		if (sorter is null)
		{
			return false;
		}

		return this._sorters.Remove(sorter);
	}
}