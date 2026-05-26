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

		/// <summary>
		/// Initializes a new instance of the ObjectDictionaryCollection class with the specified CAD dictionary.
		/// </summary>
		/// <param name="dictionary">The CAD dictionary that provides the underlying storage for the collection. Cannot be null.</param>
		protected ObjectDictionaryCollection(CadDictionary dictionary)
		{
			if (dictionary == null)
			{
				throw new ArgumentNullException(nameof(dictionary));
			}

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
			if (this.TryGet(item.Name, out T existing))
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
		/// Attempts to retrieve the entry associated with the specified name.
		/// </summary>
		/// <param name="name">The name of the entry to locate. Cannot be null.</param>
		/// <param name="entry">When this method returns, contains the entry associated with the specified name, if the name is found; otherwise,
		/// the default value for the type of the entry parameter. This parameter is passed uninitialized.</param>
		/// <returns>true if an entry with the specified name is found; otherwise, false.</returns>
		public bool TryGet(string name, out T entry)
		{
			return this._dictionary.TryGetEntry(name, out entry);
		}

		public T this[string key] { get { return (T)this._dictionary[key]; } }
	}
}