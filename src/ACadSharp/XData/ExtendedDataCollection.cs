using ACadSharp.Tables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace ACadSharp.XData
{
	public class ExtendedDataDictionary : IEnumerable<KeyValuePair<AppId, ExtendedData>>
	{
		public CadObject Owner { get; }

		private Dictionary<AppId, ExtendedData> _data = new Dictionary<AppId, ExtendedData>();

		public ExtendedDataDictionary(CadObject owner)
		{
			this.Owner = owner;
		}

		public void Add(AppId app)
		{
			this.Add(app, new ExtendedData());
		}

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
		/// Add ExtendedData for a specific AppId to the Dictionary.
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
			return this._data.ToDictionary(x => x.Key.Name, x => x.Value);
		}

		public ExtendedData Get(string name)
		{
			return this.GetExtendedDataByName()[name];
		}

		/// <summary>
		/// Get ExtendedData for a specific AppId from the Dictionary.
		/// </summary>
		/// <param name="app">The AppId object.</param>
		public ExtendedData Get(AppId app)
		{
			return this._data[app];
		}

		/// <summary>
		/// Try to get ExtendedData for a specific AppId from the Dictionary.
		/// </summary>
		/// <param name="app">The AppId object.</param>
		/// <param name="value">ExtendedData object.</param>
		public bool TryGet(AppId app, out ExtendedData value)
		{
			return this._data.TryGetValue(app, out value);
		}

		public bool TryGet(string name, out ExtendedData value)
		{
			AppId app = this._data.Keys.FirstOrDefault(k => k.Name == name);
			if (app == null)
			{
				value = null;
				return false;
			}

			value = this._data[app];
			return true;
		}

		/// <summary>
		/// Check whether a AppId is given in the Dictionary.
		/// </summary>
		/// <param name="app">The AppId object.</param>
		public bool ContainsKey(AppId app)
		{
			return this._data.ContainsKey(app);
		}

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

		public IEnumerator<KeyValuePair<AppId, ExtendedData>> GetEnumerator()
		{
			return this._data.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._data.GetEnumerator();
		}
	}
}
