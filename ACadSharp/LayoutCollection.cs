using ACadSharp.Entities;
using ACadSharp.Objects;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ACadSharp
{
	public class LayoutCollection : CadObject, IObservableCollection<Layout>
	{
		public event EventHandler<CollectionChangedEventArgs> OnBeforeAdd;
		public event EventHandler<CollectionChangedEventArgs> OnAdd;
		public event EventHandler<CollectionChangedEventArgs> OnBeforeRemove;
		public event EventHandler<CollectionChangedEventArgs> OnRemove;

		public override ObjectType ObjectType => ObjectType.UNLISTED;

		private readonly Dictionary<ulong, Layout> _entries = new Dictionary<ulong, Layout>();

		public LayoutCollection(CadDocument document)
		{
			this.Document = document;
			this.Document.RegisterCollection(this);
		}

		public void Add(Layout item)
		{
			OnBeforeAdd?.Invoke(this, new CollectionChangedEventArgs(item));

			this._entries.Add(item.Handle, item);
			item.Owner = this.Owner;

			OnAdd?.Invoke(this, new CollectionChangedEventArgs(item));
		}

		public void AddRange(IEnumerable<Layout> items)
		{
			foreach (var item in items)
			{
				this.Add(item);
			}
		}

		public bool Remove(Layout item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<Layout> GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}
	}
}
