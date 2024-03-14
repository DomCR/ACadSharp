using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Objects.Collections
{
	public abstract class ObjectDictionaryCollection<T> : IEnumerable<T>
		where T : CadObject, IDictionaryEntry
	{
		public T this[string key] { get { return (T)this._dictionary[key]; } }

		protected CadDictionary _dictionary;

		protected ObjectDictionaryCollection(CadDictionary dictionary)
		{
			this._dictionary = dictionary;
		}

		public void Add(T entry)
		{
			this._dictionary.Add(entry.Name, entry);
		}

		public T Remove(T entry)
		{
			return (T)this._dictionary.Remove(entry.Name);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this._dictionary.OfType<T>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._dictionary.OfType<T>().GetEnumerator();
		}
	}
}
