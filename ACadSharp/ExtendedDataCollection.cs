using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp
{
	public class ExtendedDataDictionary
	{
		private Dictionary<AppId, ExtendedData> _data = new Dictionary<AppId, ExtendedData>();

		public void Add(AppId app, ExtendedData edata)
		{
			this._data.Add(app, edata);
		}

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
