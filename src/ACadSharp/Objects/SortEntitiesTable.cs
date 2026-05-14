using ACadSharp.Attributes;
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
public partial class SortEntitiesTable : NonGraphicalObject, IEnumerable<SortEntitiesTable.Sorter>
{
	/// <summary>
	/// Block owner where the table is applied
	/// </summary>
	[DxfCodeValue(330)]
	public BlockRecord BlockOwner { get; internal set; }

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

		int index = this._sorters.OrderBy(s => s.SortHandle).ToList().IndexOf(existing);
		if (index <= 0 || _sorters.Count < 2)
		{
			return;
		}

		Sorter previous = this._sorters[index - 1];
		(existing.SortHandle, previous.SortHandle) = (previous.SortHandle, existing.SortHandle);
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

		this._sorters.Sort();

		int index = this._sorters.IndexOf(existing);
		if (index < 0 || index >= this._sorters.Count - 1)
		{
			return;
		}

		Sorter next = this._sorters[index + 1];
		(existing.SortHandle, next.SortHandle) = (next.SortHandle, existing.SortHandle);
	}
}