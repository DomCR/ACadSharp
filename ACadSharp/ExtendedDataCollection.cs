#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp
{
	public class ExtendedDataDictionary //: IDictionary<string, ExtendedData>
	{
		private Dictionary<string, ExtendedData> _data = new Dictionary<string, ExtendedData>();    //AppId, Data
	}

	public class ExtendedData
	{
		public List<ExtendedDataRecord> Data { get; set; } = new List<ExtendedDataRecord>();
	}

	public class ExtendedDataRecord
	{
		public DxfCode Code { get; set; }

		public object Value { get; set; }

		public ExtendedDataRecord(DxfCode dxfCode, object value)
		{
			//TODO: finish the dxf code throw exception for wrong values
		}
	}
}
