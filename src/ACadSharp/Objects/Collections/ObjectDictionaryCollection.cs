using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Objects.Collections;

/// <summary>
/// Object collection linked to a dictionary.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ObjectDictionaryCollection<T> : ICadCollection<T>, IObservableCadCollection<T>, IHandledCadObject, IEnumerable<T>
	where T : NonGraphicalObject
{
	public event EventHandler<CollectionChangedEventArgs> OnAdd
	{ add { this._dictionary.OnAdd += value; } remove { this._dictionary.OnAdd -= value; } }

	public event EventHandler<CollectionChangedEventArgs> OnRemove
	{ add { this._dictionary.OnRemove += value; } remove { this._dictionary.OnRemove -= value; } }

	public CadDocument Document { get { return this._dictionary.Document; } }

	/// <inheritdoc/>
	public ulong Handle { get { return this._dictionary.Handle; } }

	protected CadDictionary _dictionary;

	private readonly CadObjectReferenceHandler<string, T> _referenceHandler = new();

	/// <summary>
	/// Initializes a new instance of the ObjectDictionaryCollection class with the specified CAD dictionary.
	/// </summary>
	/// <param name="dictionary">The CAD dictionary that provides the underlying storage for the collection. Cannot be null.</param>
	protected ObjectDictionaryCollection(CadDictionary dictionary)
	{
		if (dictionary == null)
		{
			throw new ArgumentNullException(nameof(dictionary));
		}

		this._dictionary = dictionary;
	}

	/// <summary>
	/// Add an entry to the collection
	/// </summary>
	/// <param name="entry"></param>
	public virtual void Add(T entry)
	{
		this._dictionary.Add(entry);
	}

	/// <summary>
	/// Removes all keys and values from the <see cref="ObjectDictionaryCollection{T}"/>.
	/// </summary>
	public void Clear()
	{
		this._dictionary.Clear();
	}

	/// <summary>
	/// Determines whether the <see cref="ObjectDictionaryCollection{T}"/> contains the specified key.
	/// </summary>
	/// <param name="key">The key to locate in the <see cref="ObjectDictionaryCollection{T}"/></param>
	/// <returns></returns>
	public bool ContainsKey(string key)
	{
		return this._dictionary.ContainsKey(key);
	}

	/// <summary>
	/// Retrieves the entry associated with the specified name.
	/// </summary>
	/// <param name="name">The name of the entry to locate. Cannot be null.</param>
	/// <returns>The entry associated with the specified name.</returns>
	public T GetEntry(string name)
	{
		return this._dictionary.GetEntry<T>(name);
	}

	/// <inheritdoc/>
	public IEnumerator<T> GetEnumerator()
	{
		return this._dictionary.OfType<T>().GetEnumerator();
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this._dictionary.OfType<T>().GetEnumerator();
	}

	/// <summary>
	/// Retrieves the references associated with the specified name.
	/// </summary>
	/// <param name="name">The name of the entry whose references are to be retrieved.</param>
	/// <returns>An enumerable collection of CAD objects that reference the specified entry.</returns>
	public IEnumerable<CadObject> GetReferences(string name)
	{
		return this._referenceHandler.GetReferences(name);
	}

	/// <summary>
	/// Remove an entry from the collection.
	/// </summary>
	/// <param name="name">The name of the entry to remove.</param>
	/// <returns>true if the entry was successfully removed; otherwise, false.</returns>
	public bool Remove(string name)
	{
		return this.Remove(name, out _);
	}

	/// <summary>
	/// Remove an entry from the collection.
	/// </summary>
	/// <param name="name">The name of the entry to remove.</param>
	/// <param name="entry">When this method returns, contains the entry that was removed, if the removal was successful; otherwise, the default value for the type of the entry parameter.</param>
	/// <returns>true if the entry was successfully removed; otherwise, false.</returns>
	public virtual bool Remove(string name, out T entry)
	{
		if (this._dictionary.Remove(name, out NonGraphicalObject n))
		{
			entry = (T)n;
			this.assignToDefault(entry.Name);
			return true;
		}
		else
		{
			entry = null;
			return false;
		}
	}

	/// <inheritdoc/>
	public T TryAdd(T item)
	{
		if (this.TryGet(item.Name, out T existing))
		{
			return existing;
		}
		else
		{
			this.Add(item);
			return item;
		}
	}

	/// <summary>
	/// Attempts to retrieve the entry associated with the specified name.
	/// </summary>
	/// <param name="name">The name of the entry to locate. Cannot be null.</param>
	/// <param name="entry">When this method returns, contains the entry associated with the specified name, if the name is found; otherwise,
	/// the default value for the type of the entry parameter. This parameter is passed uninitialized.</param>
	/// <returns>true if an entry with the specified name is found; otherwise, false.</returns>
	public bool TryGet(string name, out T entry)
	{
		return this._dictionary.TryGetEntry(name, out entry);
	}

	internal void RemoveReference(string name, CadObject owner)
	{
		if (string.IsNullOrEmpty(name))
		{
			return;
		}

		this._referenceHandler.RemoveReference(name, owner);
	}

	internal T UpdateReference(CadObject owner, T entry, Action<T> assignValue)
	{
		if (owner == null)
		{
			throw new ArgumentNullException(nameof(owner), "The reference cannot be null.");
		}

		if (entry == null)
		{
			throw new ArgumentNullException(nameof(entry));
		}

		if (assignValue == null)
		{
			throw new ArgumentNullException(nameof(assignValue));
		}

		if (owner.Document != this.Document)
		{
			throw new ArgumentException("The reference must belong to the same document as the table.", nameof(owner));
		}

		var existing = this.TryAdd(entry);

		this.RemoveReference(existing.Name, owner);
		this._referenceHandler.AddReference(existing.Name, owner, assignValue);
		assignValue(existing);

		return existing;
	}

	protected virtual T getDefaultEntry()
	{
		return null;
	}

	private void assignToDefault(string name)
	{
		this._referenceHandler.RemoveReference(name, this.getDefaultEntry());
	}

	public T this[string key] { get { return (T)this._dictionary[key]; } }
}