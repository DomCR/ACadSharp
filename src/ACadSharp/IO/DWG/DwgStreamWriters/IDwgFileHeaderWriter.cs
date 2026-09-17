namespace ACadSharp.IO.DWG
{
	internal interface IDwgFileHeaderWriter
	{
		int HandleSectionOffset { get; }

		// [PATCH] MemoryStream -> Stream (supports a temp file for the AcDbObjects section, low memory)
		void AddSection(string name, System.IO.Stream stream, bool isCompressed, int decompsize = 0x7400);

		void WriteFile();
	}
}
