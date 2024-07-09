using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Objects.Collections
{
	/// <summary>
	/// Object collection linked to a dictionary
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ObjectDictionaryCollection<T> : IHandledCadObject, IEnumerable<T>
		where T : NonGraphicalObject
	{
		public event EventHandler<CollectionChangedEventArgs> OnAdd { add { this._dictionary.OnAdd += value; } remove { this._dictionary.OnAdd -= value; } }
		public event EventHandler<CollectionChangedEventArgs> OnRemove { add { this._dictionary.OnRemove += value; } remove { this._dictionary.OnRemove -= value; } }

		/// <inheritdoc/>
		public ulong Handle { get { return this._dictionary.Handle; } }

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
			this._dictionary.Add(entry);
		}

		/// <summary>
		/// Gets the value associated with the specific key
		/// </summary>
		/// <param name="name"></param>
		/// <param name="entry"></param>
		/// <returns>true if the value is found or false if not found.</returns>
		public bool TryGetValue(string name, out T entry)
		{
			return this._dictionary.TryGetEntry(name, out entry);
		}

		/// <summary>
		/// Remove an entry from the collection.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="entry"></param>
		/// <returns></returns>
		public bool Remove(string name, out T entry)
		{
			bool result = this._dictionary.Remove(name, out NonGraphicalObject n);
			entry = (T)n;
			return result;
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
