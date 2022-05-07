using ACadSharp.Entities;
using ACadSharp.Objects;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ACadSharp
{
	public class LayoutCollection : IObservableCollection<Layout>
	{
		public event EventHandler<ReferenceChangedEventArgs> OnAdd;
		public event EventHandler<ReferenceChangedEventArgs> OnRemove;

		public CadDocument Owner { get; }

		private readonly Dictionary<ulong, Layout> _entries = new Dictionary<ulong, Layout>();

		public LayoutCollection(CadDocument document)
		{
			this.Owner = document;
			document.RegisterCollection(this);
		}

		public void Add(Layout layout)
		{
			this._entries.Add(layout.Handle, layout);

			OnAdd?.Invoke(this, new ReferenceChangedEventArgs(layout));
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
