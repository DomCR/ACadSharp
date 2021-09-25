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
	/// <summary>
	/// Drawing format version codes for AutoCAD.
	/// </summary>
	public enum ACadVersion : short
	{
		/// <summary>
		/// Version not identified.
		/// </summary>
		Unknown = -1,
		/// <summary>
		/// DWG Release 1.1
		/// </summary>
		MC0_0,
		/// <summary>
		/// DWG Release 1.2
		/// </summary>
		AC1_2,
		/// <summary>
		/// DWG Release 1.4
		/// </summary>
		AC1_4,
		/// <summary>
		/// DWG Release 2.0
		/// </summary>
		AC1_50,
		/// <summary>
		/// DWG Release 2.10
		/// </summary>
		AC2_10,
		/// <summary>
		/// DWG Release 2.5
		/// </summary>
		AC1002,
		/// <summary>
		/// DWG Release 2.6
		/// </summary>
		AC1003,
		/// <summary>
		/// DWG Release 9
		/// </summary>
		AC1004,
		/// <summary>
		/// DWG Release 10
		/// </summary>
		AC1006,
		/// <summary>
		/// DWG Release 11/12 (LT R1/R2)
		/// </summary>
		AC1009,
		/// <summary>
		/// DWG Release 13 (LT95)
		/// </summary>
		AC1012,
		/// <summary>
		/// DWG Release 14, 14.01 (LT97/LT98)
		/// </summary>
		AC1014,
		/// <summary>
		/// DWG AutoCAD 2000/2000i/2002
		/// </summary>
		AC1015,
		/// <summary>
		/// DWG AutoCAD 2004/2005/2006
		/// </summary>
		AC1018,
		/// <summary>
		/// DWG AutoCAD 2007/2008/2009
		/// </summary>
		AC1021,
		/// <summary>
		/// DWG AutoCAD 2010/2011/2012
		/// </summary>
		AC1024, 
		/// <summary>
		/// DWG AutoCAD 2013/2014/2015/2016/2017
		/// </summary>
		AC1027, 
		/// <summary>
		/// DWG AutoCAD 2018/2019/2020
		/// </summary>
		AC1032, 
	}
}
