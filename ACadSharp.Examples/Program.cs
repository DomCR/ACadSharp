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
			//string file = Path.Combine(PathSamples, "dwg/cad_v2013.dwg");
			//using (DwgReader reader = new DwgReader(file, onNotification))
			//{
			//	CadDocument doc = reader.Read();
			//}

			string[] files = Directory.GetFiles(PathSamples + "/dwg/", "*.dwg");

			foreach (var f in files)
			{
				using (DwgReader reader = new DwgReader(f, onNotification))
				{
					CadDocument doc = reader.Read();
				}

				Console.WriteLine($"file read : {f}");
				Console.ReadLine();
			}
		}

		private static void onNotification(object sender, NotificationEventArgs e)
		{
			Console.WriteLine(e.Message);
		}
	}
}
