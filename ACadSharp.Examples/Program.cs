using ACadSharp.IO;
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
			//string file = Path.Combine(PathSamples, "dwg/drawing_2000.dwg");
			//string file = Path.Combine(PathSamples, "dwg/drawing_2007.dwg");
			//string file = Path.Combine(PathSamples, "dwg/drawing_2010.dwg");
			
			//string file = Path.Combine(PathSamples, "dwg/single_entities/sample_R14.dwg");
			//string file = Path.Combine(PathSamples, "dwg/single_entities/sample_2000.dwg");
			string file = Path.Combine(PathSamples, "dwg/single_entities/sample_2007.dwg");
			//string file = Path.Combine(PathSamples, "dwg/single_entities/sample_2010.dwg");
			//string file = Path.Combine(PathSamples, "dwg/single_entities/sample_2013.dwg");
			//string file = Path.Combine(PathSamples, "dwg/single_entities/sample_2018.dwg");

			using (DwgReader reader = new DwgReader(file))
			{
				CadDocument doc = reader.Read(onProgress);
			}
		}

		private static void onProgress(object sender, ProgressEventArgs e)
		{
			Console.WriteLine($"Progress: {e.Progress * 100}%");
			Console.WriteLine($"Message: {e.Message}");
		}
	}
}
