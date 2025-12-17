using ACadSharp.Tables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.XData
{
	/// <summary>
	/// Dictionary containing all the extended data for a giving <see cref="CadObject"/>, 
	/// each entry is identified with an <see cref="AppId"/> key that identifies the application that
	/// has added the collection of Extended Data (XData) into the object.
	/// </summary>
	public class ExtendedDataDictionary : IEnumerable<KeyValuePair<AppId, ExtendedData>>
	{
		/// <summary>
		/// Owner of the Extended Data dictionary.
		/// </summary>
		public CadObject Owner { get; }

		private Dictionary<AppId, ExtendedData> _data = new Dictionary<AppId, ExtendedData>();

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="owner"></param>
		public ExtendedDataDictionary(CadObject owner)
		{
			this.Owner = owner;
		}

		/// <summary>
		/// Add an empty entry of ExtendedData for a given <see cref="AppId"/>.
		/// </summary>
		/// <param name="app"></param>
		public void Add(AppId app)
		{
			this.Add(app, new ExtendedData());
		}

		/// <summary>
		/// Add ExtendedData for a specific <see cref="AppId"/> to the Dictionary.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="extendedData"></param>
		public void Add(AppId app, ExtendedData extendedData)
		{
			if (this.Owner.Document != null)
			{
				if (this.Owner.Document.AppIds.TryGetValue(app.Name, out AppId existing))
				{
					this._data.Add(existing, extendedData);
				}
				else
				{
					this.Owner.Document.AppIds.Add(app);
					this._data.Add(app, extendedData);
				}
			}
			else
			{
				this._data.Add(app, extendedData);
			}
		}

		/// <summary>
		/// Add ExtendedData for a specific <see cref="AppId"/> to the Dictionary.
		/// </summary>
		/// <param name="app">The AppId object.</param>
		/// <param name="records">The ExtendedData records.</param>
		public void Add(AppId app, IEnumerable<ExtendedDataRecord> records)
		{
			this._data.Add(app, new ExtendedData(records));
		}

		/// <summary>
		/// Get the different extended data by it's name.
		/// </summary>
		/// <returns></returns>
		public IDictionary<string, ExtendedData> GetExtendedDataByName()
		{
			return this._data.ToDictionary(x => x.Key.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Get the Extended data for a given <see cref="AppId"/> name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public ExtendedData Get(string name)
		{
			return this.GetExtendedDataByName()[name];
		}

		/// <summary>
		/// Get ExtendedData for a specific <see cref="AppId"/> from the Dictionary.
		/// </summary>
		/// <param name="app">The AppId object.</param>
		public ExtendedData Get(AppId app)
		{
			return this._data[app];
		}

		/// <summary>
		/// Try to get ExtendedData for a specific <see cref="AppId"/> from the Dictionary.
		/// </summary>
		/// <param name="app">The AppId object.</param>
		/// <param name="value">ExtendedData object.</param>
		public bool TryGet(AppId app, out ExtendedData value)
		{
			return this._data.TryGetValue(app, out value);
		}

		/// <summary>
		/// Try to get ExtendedData for a specific <see cref="AppId"/> name from the Dictionary.
		/// </summary>
		/// <param name="name">The AppId name.</param>
		/// <param name="value">ExtendedData object.</param>
		/// <returns>true, if found.</returns>
		public bool TryGet(string name, out ExtendedData value)
		{
			return this.GetExtendedDataByName().TryGetValue(name, out value);
		}

		/// <summary>
		/// Check whether a given AppId is in the Dictionary.
		/// </summary>
		/// <param name="app">The AppId object.</param>
		public bool ContainsKey(AppId app)
		{
			return this._data.ContainsKey(app);
		}

		/// <summary>
		/// Check whether a given AppId is in the Dictionary.
		/// </summary>
		/// <param name="name">The AppId name.</param>
		public bool ContainsKeyName(string name)
		{
			return this._data.Keys.Select(k => k.Name).Contains(name);
		}

		/// <summary>
		/// Clear all Dictionary entries.
		/// </summary>
		public void Clear()
		{
			this._data.Clear();
		}

		/// <inheritdoc/>
		public IEnumerator<KeyValuePair<AppId, ExtendedData>> GetEnumerator()
		{
			return this._data.GetEnumerator();
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._data.GetEnumerator();
		}
	}
}
