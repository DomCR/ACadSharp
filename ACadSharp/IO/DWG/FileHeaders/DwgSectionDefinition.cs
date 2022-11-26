using System;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG
{
	internal static class DwgSectionDefinition
	{
		public const string AcDbObjects = "AcDb:AcDbObjects";
		public const string AppInfo = "AcDb:AppInfo";
		public const string AuxHeader = "AcDb:AuxHeader";
		public const string Header = "AcDb:Header";
		public const string Classes = "AcDb:Classes";
		public const string Handles = "AcDb:Handles";
		public const string ObjFreeSpace = "AcDb:ObjFreeSpace";
		public const string Template = "AcDb:Template";
		public const string SummaryInfo = "AcDb:SummaryInfo";
		public const string FileDepList = "AcDb:FileDepList";
		public const string Preview = "AcDb:Preview";
		public const string RevHistory = "AcDb:RevHistory";

		public static int GetSectionLocatorByName(string name)
		{
			switch (name)
			{
				case "AcDb:Header":
					return 0;
				case "AcDb:Classes":
					return 1;
				case "AcDb:Handles":
					return 2;
				case "AcDb:ObjFreeSpace":
					return 3;
				case "AcDb:Template":
					return 4;
				//No record id for this sections
				case "AcDb:SummaryInfo":
				case "AcDb:AcDbObjects":
				case "AcDb:FileDepList":
				default:
					return -1;
			}
		}
	}
}