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
	public class ExtendedDataCollection //: IDictionary<string, ExtendedData>
	{
		private Dictionary<string, ExtendedData> m_data = new Dictionary<string, ExtendedData>();	//AppId, Data
	}

	public class ExtendedData
	{

	}
}
