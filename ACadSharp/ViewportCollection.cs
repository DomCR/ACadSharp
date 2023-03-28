using ACadSharp.Entities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ACadSharp
{
	public class ViewportCollection : CadObject, IObservableCollection<Viewport>
	{
		public event EventHandler<CollectionChangedEventArgs> OnAdd;
		public event EventHandler<CollectionChangedEventArgs> OnRemove;

		public override ObjectType ObjectType => ObjectType.VP_ENT_HDR_CTRL_OBJ;

		private readonly Dictionary<ulong, Viewport> _entries = new Dictionary<ulong, Viewport>();

		public ViewportCollection(CadDocument document)
		{
			document.RegisterCollection(this);
		}

		public void Add(Viewport item)
		{
			this._entries.Add(item.Handle, item);
			item.Owner = this.Owner;

			OnAdd?.Invoke(this, new CollectionChangedEventArgs(item));
		}

		public void AddRange(IEnumerable<Viewport> items)
		{
			foreach (var item in items)
			{
				this.Add(item);
			}
		}

		public bool Remove(Viewport item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<Viewport> GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}
	}
}
