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
			//string file = Path.Combine(PathSamples, "dwg/cad_v2000.dwg");
			string file = Path.Combine(PathSamples, "dwg/cad_v2018.dwg");

			using (DwgReader reader = new DwgReader(file))
			{
				CadDocument doc = reader.Read(onNotification);
			}
		}

		private static void onProgress(object sender, ProgressEventArgs e)
		{
			Console.WriteLine($"Progress: {e.Progress * 100}%");
			Console.WriteLine($"Message: {e.Message}");
		}

		private static void onNotification(object sender, NotificationEventArgs e)
		{
			Console.WriteLine(e.Message);
		}
	}
}
