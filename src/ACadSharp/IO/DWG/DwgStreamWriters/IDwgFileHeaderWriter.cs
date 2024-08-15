namespace ACadSharp.IO.DWG
{
	internal interface IDwgFileHeaderWriter
	{
		int HandleSectionOffset { get; }

		void AddSection(string name, System.IO.MemoryStream stream, bool isCompressed, int decompsize = 0x7400);

		void WriteFile();
	}
}
