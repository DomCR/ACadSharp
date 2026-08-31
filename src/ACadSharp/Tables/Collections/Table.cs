using ACadSharp.Attributes;
using CSUtilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Tables.Collections;

[DxfSubClass(DxfSubclassMarker.Table)]
public abstract class Table<T> : CadObject, ITable, ICadCollection<T>, IObservableCadCollection<T>
	where T : TableEntry
{
	public event EventHandler<CollectionChangedEventArgs> OnAdd;

	public event EventHandler<CollectionChangedEventArgs> OnRemove;

	/// <summary>
	/// Gets the number of entries in this table
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Count, 70)]
	public int Count => this.entries.Count;

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.TableEntry;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.Table;

	protected readonly Dictionary<string, T> entries = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

	private readonly CadObjectReferenceHandler<string, T> _referenceHandler = new();

	protected Table()
	{ }

	protected Table(CadDocument document)
	{
		this.Owner = document;
		document.RegisterCollection(this);
	}

	/// <summary>
	/// Add a <see cref="TableEntry"/> to the collection, this method triggers <see cref="OnAdd"/>
	/// </summary>
	/// <param name="item"></param>
	public virtual void Add(T item)
	{
		if (string.IsNullOrEmpty(item.Name))
		{
			item.Name = this.createName("unnamed");
		}

		this.add(item.Name, item);
	}

	/// <summary>
	/// Add multiple <see cref="TableEntry"/> to the collection, this method triggers <see cref="OnAdd"/>
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
	/// Determines whether the <see cref="Table{T}"/> contains the specified key.
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	public bool Contains(string key)
	{
		return this.entries.ContainsKey(key);
	}

	/// <summary>
	/// Create the default entries for the table if they don't exist
	/// </summary>
	public void CreateDefaultEntries()
	{
		foreach (string entry in this.getDefaultEntries())
		{
			if (this.Contains(entry))
				continue;

			this.Add((T)Activator.CreateInstance(typeof(T), new object[] { entry }));
		}
	}

	/// <summary>
	/// Gets the default entry for the table.
	/// </summary>
	/// <returns>The default <see cref="TableEntry"/>.</returns>
	public abstract T GetDefaultEntry();

	/// <inheritdoc/>
	public IEnumerator<T> GetEnumerator()
	{
		return this.entries.Values.GetEnumerator();
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.entries.Values.GetEnumerator();
	}

	/// <summary>
	/// Gets the references to a <see cref="TableEntry"/> by name.
	/// </summary>
	/// <param name="name">The name of the table entry.</param>
	/// <returns>An enumerable of <see cref="CadObject"/> that reference the specified table entry.</returns>
	public IEnumerable<CadObject> GetReferences(string name)
	{
		return this._referenceHandler.GetReferences(name);
	}

	/// <summary>
	/// Removes a <see cref="TableEntry"/> from the collection, this method triggers <see cref="OnRemove"/>
	/// </summary>
	/// <param name="key">key in the table</param>
	/// <returns>The removed <see cref="TableEntry"/></returns>
	public T Remove(string key)
	{
		if (this.getDefaultEntries().Contains(key))
			return null;

		if (this.entries.Remove(key, out T item))
		{
			item.Owner = null;
			OnRemove?.Invoke(this, new CollectionChangedEventArgs(item));
			item.OnNameChanged -= this.onEntryNameChanged;

			this.assignToDefault(item.Name);

			return item;
		}

		return null;
	}

	/// <summary>
	/// Tries to add the item to the collection, if an item with the same name already exists it returns the existing item.
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	public T TryAdd(T item)
	{
		if (this.TryGetValue(item.Name, out T existing))
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
	/// Gets the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key of the value to get.</param>
	/// <param name="item"></param>
	/// <returns>true if the <see cref="Table{T}"/> contains an element with the specified key; otherwise, false.</returns>
	public bool TryGetValue(string key, out T item)
	{
		return this.entries.TryGetValue(key, out item);
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

	protected void add(string key, T item)
	{
		this.entries.Add(key, item);
		item.Owner = this;

		item.OnNameChanged += this.onEntryNameChanged;

		OnAdd?.Invoke(this, new CollectionChangedEventArgs(item));
	}

	protected void addHandlePrefix(T item)
	{
		item.Owner = this;
		item.OnNameChanged += this.onEntryNameChanged;

		OnAdd?.Invoke(this, new CollectionChangedEventArgs(item));

		string key = $"{item.Handle}:{item.Name}";

		this.entries.Add(key, item);
	}

	protected string createName(string prefix)
	{
		string name = prefix;
		int i = 0;
		while (this.entries.ContainsKey($"{prefix}{i}"))
		{
			i++;
		}

		return $"{prefix}{i}";
	}

	protected abstract string[] getDefaultEntries();

	private void assignToDefault(string name)
	{
		this._referenceHandler.RemoveReference(name, this.GetDefaultEntry());
	}

	private void onEntryNameChanged(object sender, OnNameChangedArgs e)
	{
		if (this.getDefaultEntries().Contains(e.OldName, StringComparer.InvariantCultureIgnoreCase))
		{
			throw new ArgumentException($"The name {e.OldName} belongs to a default entry.");
		}

		var entry = this.entries[e.OldName];
		this.entries.Add(e.NewName, entry);
		this.entries.Remove(e.OldName);
		this._referenceHandler.ChangeKey(e.OldName, e.NewName);
	}

	public T this[string name]
	{
		get
		{
			return this.entries[name];
		}
	}
}