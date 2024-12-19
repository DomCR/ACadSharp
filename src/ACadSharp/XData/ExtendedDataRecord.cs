using System;

namespace ACadSharp.XData
{
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
