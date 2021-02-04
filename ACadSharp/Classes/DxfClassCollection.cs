using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.Classes
{
	public class DxfClassCollection : ICollection<DxfClass>
	{
		public int Count => m_list.Count;

		public bool IsReadOnly => false;

		public Dictionary<string, DxfClass> m_list = new Dictionary<string, DxfClass>();

		public void Add(DxfClass item)
		{
			m_list.Add(item.DxfName, item);
		}

		public void Clear()
		{
			m_list.Clear();
		}

		public bool Contains(DxfClass item)
		{
			return m_list.Values.Contains(item);
		}

		public void CopyTo(DxfClass[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<DxfClass> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public bool Remove(DxfClass item)
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}
