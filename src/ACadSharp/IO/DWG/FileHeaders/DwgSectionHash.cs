namespace ACadSharp.IO.DWG
{
	internal enum DwgSectionHash
	{
		AcDb_Unknown = 0x00000000,
		AcDb_Security = 0x4a0204ea,
		AcDb_FileDepList = 0x6c4205ca,
		AcDb_VBAProject = 0x586e0544,
		AcDb_AppInfo = 0x3fa0043e,
		AcDb_Preview = 0x40aa0473,
		AcDb_SummaryInfo = 0x717a060f,
		AcDb_RevHistory = 0x60a205b3,
		AcDb_AcDbObjects = 0x674c05a9,
		AcDb_ObjFreeSpace = 0x77e2061f,
		AcDb_Template = 0x4a1404ce,
		AcDb_Handles = 0x3f6e0450,
		AcDb_Classes = 0x3f54045f,
		AcDb_AuxHeader = 0x54f0050a,
		AcDb_Header = 0x32b803d9,
		AcDb_Signature = -0x00000001,
	}
}