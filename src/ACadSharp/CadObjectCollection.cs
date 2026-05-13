using CSUtilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp;

/// <summary>
/// Collection formed by <see cref="CadObject"/> managed by an owner.
/// </summary>
/// <typeparam name="T"></typeparam>
public class CadObjectCollection<T> : IObservableCadCollection<T>
	where T : CadObject
{
	/// <inheritdoc/>
	public event EventHandler<CollectionChangedEventArgs> OnAdd;

	/// <summary>
	/// Occurs before an item is removed from the collection.
	/// </summary>
	/// <remarks>Subscribers can use this event to perform actions or validation before the removal operation
	/// completes. To cancel the removal, handle the event and set the appropriate property on the event arguments if
	/// supported.</remarks>
	public event EventHandler<CollectionChangedEventArgs> OnBeforeRemove;

	/// <inheritdoc/>
	public event EventHandler<CollectionChangedEventArgs> OnRemove;

	/// <summary>
	/// Gets the number of elements that are contained in the collection.
	/// </summary>
	public int Count { get { return this._entries.Count; } }

	/// <summary>
	/// Owner of the collection.
	/// </summary>
	public CadObject Owner { get; }

	protected readonly HashSet<T> _entries = new HashSet<T>();

	/// <summary>
	/// Default constructor for a <see cref="CadObjectCollection{T}"/> with it's owner assigned.
	/// </summary>
	/// <param name="owner">Owner of the collection.</param>
	public CadObjectCollection(CadObject owner)
	{
		this.Owner = owner;
	}

	/// <summary>
	/// Add a <see cref="CadObject"/> to the collection, this method triggers <see cref="OnAdd"/>.
	/// </summary>
	/// <param name="item"></param>
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="ArgumentNullException"></exception>
	public virtual void Add(T item)
	{
		if (item is null) throw new ArgumentNullException(nameof(item));

		if (item.Owner != null)
			throw new ArgumentException($"Item {item.GetType().FullName} already has an owner", nameof(item));

		if (this._entries.Contains(item))
			throw new ArgumentException($"Item {item.GetType().FullName} is already in the collection", nameof(item));

		this._entries.Add(item);
		item.Owner = this.Owner;

		OnAdd?.Invoke(this, new CollectionChangedEventArgs(item));
	}

	/// <summary>
	/// Add multiple <see cref="CadObject"/> to the collection, this method triggers <see cref="OnAdd"/>.
	/// </summary>
	/// <param name="items"></param>
	public void AddRange(IEnumerable<T> items)
	{
		foreach (var item in items)
		{
			this.Add(item);
		}
	}

	/// <summary>
	/// Removes all elements from the Collection.
	/// </summary>
	public void Clear()
	{
		Queue<T> q = new(this._entries.ToList());
		while (q.TryDequeue(out T entry))
		{
			this.Remove(entry);
		}
	}

	/// <inheritdoc/>
	public IEnumerator<T> GetEnumerator()
	{
		return this._entries.GetEnumerator();
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this._entries.GetEnumerator();
	}

	/// <summary>
	/// Removes the specified item from the collection.
	/// </summary>
	/// <remarks>If an event handler for the removal is registered and cancels the operation, the item will not be
	/// removed. After successful removal, the item's owner is set to null and a removal event is raised.</remarks>
	/// <param name="item">The item to remove from the collection.</param>
	/// <returns>true if the item was successfully removed; otherwise, false.</returns>
	public virtual bool Remove(T item)
	{
		if (this.OnBeforeRemove != null)
		{
			CollectionChangedEventArgs args = new(item);
			this.OnBeforeRemove.Invoke(this, args);
			if (args.Cancel)
			{
				return false;
			}
		}

		if (!this._entries.Remove(item))
		{
			return false;
		}

		item.Owner = null;

		OnRemove?.Invoke(this, new CollectionChangedEventArgs(item));

		return true;
	}

	public T this[int index]
	{
		get
		{
			return this._entries.ElementAtOrDefault(index);
		}
	}
}