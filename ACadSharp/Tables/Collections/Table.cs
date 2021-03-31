using ACadSharp.IO.Templates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.Tables.Collections
{
	public abstract class Table<T> : CadObject, ICollection<T>	//TODO: Change ICollection interface ??
		where T : TableEntry
	{
		public override string ObjectName => DxfFileToken.TableEntry;

		public int Count => m_entries.Count;
		public bool IsReadOnly => false;
		public T this[string name]
		{
			get
			{
				return this.m_entries.TryGetValue(name, out T item) ? item : null;
			}
		}

		public Dictionary<string, T> m_entries = new Dictionary<string, T>();

		public Table() { }

		internal Table(DxfTableTemplate template) 
		{
			Handle = template.Handle;
			OwnerHandle = template.OwnerHandle;
		}

		public void Add(T item)
		{
			m_entries.Add(item.Name, item);
		}
		public void AddRange(IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				Add(item);
			}
		}
		public void Clear()
		{
			m_entries.Clear();
		}
		public bool Contains(T item)
		{
			return m_entries.Values.Contains(item);
		}
		public void CopyTo(T[] array, int arrayIndex)
		{
			m_entries.Values.CopyTo(array, arrayIndex);
		}
		public IEnumerator<T> GetEnumerator()
		{
			return m_entries.Values.GetEnumerator();
		}
		public bool Remove(T item)
		{
			return m_entries.Remove(item.Name);
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_entries.Values.GetEnumerator();
		}
	}
}
