using ACadSharp.IO.Templates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.Tables.Collections
{
	public abstract class Table<T> : CadObject, IObservableCollection<T>
		where T : TableEntry
	{
		public event EventHandler<CollectionChangedEventArgs> OnBeforeAdd;
		public event EventHandler<CollectionChangedEventArgs> OnAdd;
		public event EventHandler<CollectionChangedEventArgs> OnBeforeRemove;
		public event EventHandler<CollectionChangedEventArgs> OnRemove;

		public override string ObjectName => DxfFileToken.TableEntry;

		public int Count => this._entries.Count;

		public bool IsReadOnly => false;

		public T this[string name]
		{
			get
			{
				return this._entries.TryGetValue(name, out T item) ? item : null;
			}
		}

		private readonly Dictionary<string, T> _entries = new Dictionary<string, T>();

		public Table(CadDocument document)
		{
			this.Document = document;
			this.Document.RegisterCollection(this);
		}

		internal Table(DxfTableTemplate template)
		{
			this.Handle = template.Handle;
		}

		public void Add(T item)
		{
			OnBeforeAdd?.Invoke(this, new CollectionChangedEventArgs(item));

			this._entries.Add(item.Name, item);
			item.Owner = this;

			OnAdd?.Invoke(this, new CollectionChangedEventArgs(item));
		}

		public void AddRange(IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				this.Add(item);
			}
		}

		public bool Contains(T item)
		{
			return this._entries.Values.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			this._entries.Values.CopyTo(array, arrayIndex);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}

		public bool Remove(T item)
		{
			return this._entries.Remove(item.Name);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}

		internal void addOnBuild(T item)
		{
			OnBeforeAdd?.Invoke(this, new CollectionChangedEventArgs(item));

			this._entries.Add(item.Name, item);
			item.Owner = this;

			OnAdd?.Invoke(this, new CollectionChangedEventArgs(item));
		}
	}
}
