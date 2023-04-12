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
		/// Release 1.1
		/// </summary>
		MC0_0,
		/// <summary>
		/// Release 1.2
		/// </summary>
		AC1_2,
		/// <summary>
		/// Release 1.4
		/// </summary>
		AC1_4,
		/// <summary>
		/// Release 2.0
		/// </summary>
		AC1_50,
		/// <summary>
		/// Release 2.10
		/// </summary>
		AC2_10,
		/// <summary>
		/// Release 2.5
		/// </summary>
		AC1002,
		/// <summary>
		/// Release 2.6
		/// </summary>
		AC1003,
		/// <summary>
		/// Release 9
		/// </summary>
		AC1004,
		/// <summary>
		/// Release 10
		/// </summary>
		AC1006,
		/// <summary>
		/// Release 11/12 (LT R1/R2)
		/// </summary>
		AC1009,
		/// <summary>
		/// Release 13 (LT95)
		/// </summary>
		AC1012 = 19,
		/// <summary>
		/// Release 14, 14.01 (LT97/LT98)
		/// </summary>
		AC1014 = 21,
		/// <summary>
		/// AutoCAD 2000/2000i/2002
		/// </summary>
		AC1015 = 23,
		/// <summary>
		/// AutoCAD 2004/2005/2006
		/// </summary>
		AC1018 = 25,
		/// <summary>
		/// AutoCAD 2007/2008/2009
		/// </summary>
		AC1021 = 27,
		/// <summary>
		/// AutoCAD 2010/2011/2012
		/// </summary>
		AC1024 = 29, 
		/// <summary>
		/// AutoCAD 2013/2014/2015/2016/2017
		/// </summary>
		AC1027 = 31, 
		/// <summary>
		/// AutoCAD 2018/2019/2020
		/// </summary>
		AC1032 = 33 
	}
}
