using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Entities.Collections
{
	[Obsolete]
	public class VertexCollection : IEntityCollection<Vertex>
	{
		public int Count => _vertices.Count;
		
		public bool IsReadOnly => false;

		public Dictionary<ulong, Vertex> _vertices = new Dictionary<ulong, Vertex>();

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

			_vertices.Add(item.Handle, item);
		}

		public void AddRange(IEnumerable<Vertex> vertices)
		{
			foreach (var v in vertices)
			{
				this.Add(v);
			}
		}

		public void Clear()
		{
			_vertices.Clear();
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
			return _vertices.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _vertices.Values.GetEnumerator();
		}
	}
}
