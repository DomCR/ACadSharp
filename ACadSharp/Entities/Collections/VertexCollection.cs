using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Entities.Collections
{
	public class VertexCollection : IEntityCollection<Vertex>
	{
		public int Count => m_vertices.Count;
		public bool IsReadOnly => false;

		public Dictionary<ulong, Vertex> m_vertices = new Dictionary<ulong, Vertex>();

		public VertexCollection() : base() { }

		public void Add(Entity item)
		{
			if (item.GetType() != typeof(Vertex))
				throw new ArgumentException();

			Add(item as Vertex);
		}
		public void Add(Vertex item)
		{
			if (item == null)
				throw new ArgumentNullException();

			//if (m_vertices.ContainsKey(item.Handle))
			//	throw new ArgumentException();

			m_vertices.Add(item.Handle, item);
		}
		public void Clear()
		{
			m_vertices.Clear();
		}

		public bool Contains(Entity item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(Entity[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(Entity item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<Entity> GetEnumerator()
		{
			return m_vertices.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_vertices.Values.GetEnumerator();
		}
	}
}
