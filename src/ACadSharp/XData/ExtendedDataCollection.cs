using ACadSharp.Tables;
using System;
using System.Collections.Generic;

namespace ACadSharp.XData
{
	public class ExtendedDataDictionary
	{
		public IEnumerable<KeyValuePair<AppId, ExtendedData>> Entries { get { return this._data; } }

		public CadObject Owner { get; }

		private Dictionary<AppId, ExtendedData> _data = new Dictionary<AppId, ExtendedData>();

		public ExtendedDataDictionary(CadObject owner)
		{
			this.Owner = owner;
		}

		public void Add(AppId app)
		{
			this._data.Add(app, new ExtendedData(app));
		}
		/// <summary>
		/// Add ExtendedData for a specific AppId to the Dictionary.
		/// </summary>
		/// <param name="app">The AppId object.</param>
		/// <param name="records">The ExtendedData records.</param>
		public void Add(AppId app, IEnumerable<ExtendedDataRecord> records)
		{
			this._data.Add(app, new ExtendedData(app, records));
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

		/// <summary>
		/// Check whether a AppId is given in the Dictionary.
		/// </summary>
		/// <param name="app">The AppId object.</param>
		public bool ContainsKey(AppId app)
		{
			return this._data.ContainsKey(app);
		}

		/// <summary>
		/// Clear all Dictionary entries.
		/// </summary>
		public void Clear()
		{
			this._data.Clear();
		}
	}
}
