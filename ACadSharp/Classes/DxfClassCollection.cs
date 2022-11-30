using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Classes
{
	public class DxfClassCollection : ICollection<DxfClass>
	{
		public int Count => _list.Count;

		public bool IsReadOnly => false;

		public Dictionary<string, DxfClass> _list = new Dictionary<string, DxfClass>();

		public void Add(DxfClass item)
		{
			if (_list.ContainsKey(item.DxfName))
				return;

			_list.Add(item.DxfName, item);
		}

		public void Clear()
		{
			_list.Clear();
		}

		public bool Contains(DxfClass item)
		{
			return _list.Values.Contains(item);
		}

		public void CopyTo(DxfClass[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<DxfClass> GetEnumerator()
		{
			return _list.Values.GetEnumerator();
		}

		public bool Remove(DxfClass item)
		{
			return this._list.Remove(item.DxfName);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._list.Values.GetEnumerator();
		}
	}
}
