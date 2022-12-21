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
				//No record id for this sections
				case SummaryInfo:
				case AcDbObjects:
				case FileDepList:
				default:
					return null;
			}
		}
	}
}