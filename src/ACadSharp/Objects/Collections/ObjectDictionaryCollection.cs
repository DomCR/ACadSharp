using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Objects.Collections
{
	/// <summary>
	/// Object collection linked to a dictionary.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ObjectDictionaryCollection<T> : ICadCollection<T>, IObservableCadCollection<T>, IHandledCadObject, IEnumerable<T>
		where T : NonGraphicalObject
	{
		public event EventHandler<CollectionChangedEventArgs> OnAdd
		{ add { this._dictionary.OnAdd += value; } remove { this._dictionary.OnAdd -= value; } }

		public event EventHandler<CollectionChangedEventArgs> OnRemove
		{ add { this._dictionary.OnRemove += value; } remove { this._dictionary.OnRemove -= value; } }

		/// <inheritdoc/>
		public ulong Handle { get { return this._dictionary.Handle; } }

		protected CadDictionary _dictionary;

		protected ObjectDictionaryCollection(CadDictionary dictionary)
		{
			this._dictionary = dictionary;
		}

		/// <summary>
		/// Add an entry to the collection
		/// </summary>
		/// <param name="entry"></param>
		public virtual void Add(T entry)
		{
			this._dictionary.Add(entry);
		}

		/// <summary>
		/// Removes all keys and values from the <see cref="ObjectDictionaryCollection{T}"/>.
		/// </summary>
		public void Clear()
		{
			this._dictionary.Clear();
		}

		/// <summary>
		/// Determines whether the <see cref="ObjectDictionaryCollection{T}"/> contains the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="ObjectDictionaryCollection{T}"/></param>
		/// <returns></returns>
		public bool ContainsKey(string key)
		{
			return this._dictionary.ContainsKey(key);
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

		/// <summary>
		/// Remove an entry from the collection.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool Remove(string name)
		{
			return this.Remove(name, out _);
		}

		/// <summary>
		/// Remove an entry from the collection.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="entry"></param>
		/// <returns></returns>
		public virtual bool Remove(string name, out T entry)
		{
			bool result = this._dictionary.Remove(name, out NonGraphicalObject n);
			entry = (T)n;
			return result;
		}

		/// <inheritdoc/>
		public T TryAdd(T item)
		{
			if (this.TryGetValue(item.Name, out T existing))
			{
				return existing;
			}
			else
			{
				this.Add(item);
				return item;
			}
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

		public T this[string key] { get { return (T)this._dictionary[key]; } }
	}
}