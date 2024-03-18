using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Objects.Collections
{
	/// <summary>
	/// Object collection linked to a dictionary
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ObjectDictionaryCollection<T> : IEnumerable<T>
		where T : CadObject, IDictionaryEntry
	{
		public T this[string key] { get { return (T)this._dictionary[key]; } }

		protected CadDictionary _dictionary;

		protected ObjectDictionaryCollection(CadDictionary dictionary)
		{
			this._dictionary = dictionary;
		}

		/// <summary>
		/// Add an entry to the collection
		/// </summary>
		/// <param name="entry"></param>
		public void Add(T entry)
		{
			this._dictionary.Add(entry.Name, entry);
		}

		/// <summary>
		/// Remove an entry from the collection
		/// </summary>
		/// <param name="entry"></param>
		/// <returns></returns>
		public T Remove(T entry)
		{
			return (T)this._dictionary.Remove(entry.Name);
		}

		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator()
		{
			return this._dictionary.OfType<T>().GetEnumerator();
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._dictionary.OfType<T>().GetEnumerator();
		}
	}
}
