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

		public static Dictionary<string, byte[]> StartSentinels = new Dictionary<string, byte[]>()
		{
			{Header,  new byte[16]{ 0xCF, 0x7B, 0x1F, 0x23, 0xFD, 0xDE, 0x38, 0xA9, 0x5F, 0x7C, 0x68, 0xB8, 0x4E, 0x6D, 0x33, 0x5F } },
			{Classes, new byte[16]{ 0x8D, 0xA1, 0xC4, 0xB8, 0xC4, 0xA9, 0xF8, 0xC5, 0xC0, 0xDC, 0xF4, 0x5F, 0xE7, 0xCF, 0xB6, 0x8A } },
			{Preview, new byte[16]{ 0x1F, 0x25, 0x6D, 0x07, 0xD4, 0x36, 0x28, 0x28, 0x9D, 0x57, 0xCA, 0x3F, 0x9D, 0x44, 0x10, 0x2B } },
		};

		public static Dictionary<string, byte[]> EndSentinels = new Dictionary<string, byte[]>()
		{
			{Header, new byte[16]{ 0x30, 0x84, 0xE0, 0xDC, 0x02, 0x21, 0xC7, 0x56, 0xA0, 0x83, 0x97, 0x47, 0xB1, 0x92, 0xCC, 0xA0 } },
			{Classes, new byte[16]{ 0x72, 0x5E, 0x3B, 0x47, 0x3B, 0x56, 0x07, 0x3A, 0x3F, 0x23, 0x0B, 0xA0, 0x18, 0x30, 0x49, 0x75 } },
			{Preview, new byte[16]{ 0xE0, 0xDA, 0x92, 0xF8, 0x2B, 0xc9, 0xD7, 0xD7, 0x62, 0xA8, 0x35, 0xC0, 0x62, 0xBB, 0xEF, 0xD4 } },
		};

		public static int? GetSectionLocatorByName(string name)
		{
			switch (name)
			{
				case Header:
					return 0;
				case Classes:
					return 1;
				case Handles:
					return 2;
				case ObjFreeSpace:
					return 3;
				case Template:
					return 4;
				case AuxHeader:
					return 5;
				default:
					return null;
			}
		}
	}
}