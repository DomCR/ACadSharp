using ACadSharp.Attributes;
using CSUtilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Tables.Collections
{
	[DxfSubClass(DxfSubclassMarker.Table)]
	public abstract class Table<T> : CadObject, IObservableCollection<T>
		where T : TableEntry
	{
		public event EventHandler<CollectionChangedEventArgs> OnAdd;
		public event EventHandler<CollectionChangedEventArgs> OnRemove;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableEntry;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Table;

		/// <summary>
		/// Gets the number of entries in this table
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 70)]
		public int Count => this._entries.Count;

		public T this[string name]
		{
			get
			{
				return this._entries.Values.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
			}
		}

		protected abstract string[] _defaultEntries { get; }

		protected readonly Dictionary<string, T> _entries = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

		protected Table() { }

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
				item.Name = this.createName();
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

		public bool TryGetValue(string key, out T item)
		{
			return this._entries.TryGetValue(key, out item);
		}

		public bool Contains(string key)
		{
			return this._entries.ContainsKey(key);
		}

		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}

		/// <summary>
		/// Removes a <see cref="TableEntry"/> from the collection, this method triggers <see cref="OnRemove"/>
		/// </summary>
		/// <param name="key">key in the table</param>
		/// <returns>The removed <see cref="TableEntry"/></returns>
		public T Remove(string key)
		{
			if (this._defaultEntries.Contains(key))
				return null;

			if (this._entries.Remove(key, out T item))
			{
				item.Owner = null;
				OnRemove?.Invoke(this, new CollectionChangedEventArgs(item));
				return item;
			}

			return null;
		}

		/// <summary>
		/// Create the default entries for the table if they don't exist
		/// </summary>
		public void CreateDefaultEntries()
		{
			foreach (string entry in this._defaultEntries)
			{
				if (this.Contains(entry))
					continue;

				this.Add((T)Activator.CreateInstance(typeof(T), new object[] { entry }));
			}
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}

		protected void add(string key, T item)
		{
			this._entries.Add(key, item);
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

			this._entries.Add(key, item);
		}

		private void onEntryNameChanged(object sender, OnNameChangedArgs e)
		{
			if (this._defaultEntries.Contains(e.OldName, StringComparer.InvariantCultureIgnoreCase))
			{
				throw new ArgumentException($"The name {e.OldName} belongs to a default entry.");
			}

			var entry = this._entries[e.OldName];
			this._entries.Add(e.NewName, entry);
			this._entries.Remove(e.OldName);
		}

		private string createName()
		{
			string name = "generated_name";
			int i = 0;
			while (this._entries.ContainsKey($"{name}_{i}"))
			{
				i++;
			}

			return $"{name}_{i}";
		}
	}
}
