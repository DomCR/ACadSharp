using System;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG
{
	internal static class DwgSectionDefinition
	{
		public const string Header = "AcDb:Header";
		public const string Classes = "AcDb:Classes";
		public const string Handles = "AcDb:Handles";
		public const string ObjFreeSpace = "AcDb:ObjFreeSpace";
		public const string Template = "AcDb:Template";
		public const string SummaryInfo = "AcDb:SummaryInfo";
		public const string AcDbObjects = "AcDb:AcDbObjects";
		public const string FileDepList = "AcDb:FileDepList";

		public static Dictionary<string, byte[]> StartSentinels = new Dictionary<string, byte[]>()
		{
			{
				Header,
				new byte[16]
				{
					0xCF, 0x7B, 0x1F, 0x23, 0xFD, 0xDE, 0x38, 0xA9, 0x5F, 0x7C, 0x68, 0xB8, 0x4E, 0x6D, 0x33, 0x5F
				}
			},
			{
				Classes,
				new byte[16]
				{
					0x8D,0xA1, 0xC4, 0xB8, 0xC4, 0xA9, 0xF8, 0xC5, 0xC0, 0xDC, 0xF4, 0x5F, 0xE7, 0xCF, 0xB6, 0x8A
				}
			},
		};

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