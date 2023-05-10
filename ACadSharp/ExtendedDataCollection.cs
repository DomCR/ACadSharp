using ACadSharp.Tables;
using System;
using System.Collections.Generic;

namespace ACadSharp
{
	public class ExtendedDataDictionary
	{
		private Dictionary<AppId, ExtendedData> _data = new Dictionary<AppId, ExtendedData>();

		/// <summary>Add ExtendedData for a specific AppId to the Dictionary.</summary>
		/// <param name="app">The AppId object.</param>
		/// <param name="edata">The ExtendedData object.</param>
		public void Add(AppId app, ExtendedData edata)
		{
			this._data.Add(app, edata);
		}

		/// <summary>Get ExtendedData for a specific AppId from the Dictionary.</summary>
		/// <param name="app">The AppId object.</param>
		/// <param name="edata">The ExtendedData object.</param>
		public ExtendedData Get(AppId app)
		{
			return this._data[app];
		}

		/// <summary>Try to get ExtendedData for a specific AppId from the Dictionary.</summary>
		/// <param name="app">The AppId object.</param>
		/// <param name="value">ExtendedData object.</param>
		public bool TryGet(AppId app, out ExtendedData value)
		{
			return this._data.TryGetValue(app, out value);
		}

		/// <summary>Check whether a AppId is given in the Dictionary.</summary>
		/// <param name="app">The AppId object.</param>
		public bool ContainsKey(AppId app)
		{
			return this._data.ContainsKey(app);
		}

		/// <summary>Clear all Dictionary entries.</summary>
		public void Clear()
		{
			this._data.Clear();
		}
	}

	public class ExtendedData
	{
		public List<ExtendedDataRecord> Data { get; set; } = new List<ExtendedDataRecord>();
	}

	public class ExtendedDataRecord
	{
		public DxfCode Code
		{
			get { return this._code; }
			set
			{
				if (value < DxfCode.ExtendedDataAsciiString)
				{
					throw new ArgumentException($"Dxf code for ExtendedDataRecord is not valid: {value}", nameof(value));
				}

				this._code = value;
			}
		}

		public object Value { get; set; }

		private DxfCode _code;

		public ExtendedDataRecord(DxfCode dxfCode, object value)
		{
			this.Code = dxfCode;
			this.Value = value;
		}
	}
}
