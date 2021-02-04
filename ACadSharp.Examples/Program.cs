using ACadSharp.IO.DWG;
using ACadSharp.IO.DXF;
using System;
using System.IO;

namespace ACadSharp.Examples
{
	class Program
	{
		static string PathSamples = "../../../../samples";
		static void Main(string[] args)
		{
			//ReadDxf();
			ReadDwg();
		}

		static void ReadDxf()
		{
			string file = Path.Combine(PathSamples, "dxf/ascii.dxf");
			DxfReader reader = new DxfReader(file);
			reader.Read();
		}

		static void ReadDwg()
		{
			string file = Path.Combine(PathSamples, "dwg/drawing_2007.dwg");
			DwgReader reader = new DwgReader(file);

			//TEXT ENTITIES OFFSETS:
			//	250511
			//	250939

			//reader.ReadObjects(ObjectType.TEXT);
			reader.ReadObjects(250511);
		}
	}
}
