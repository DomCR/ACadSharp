using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using ACadSharp.IO.DXF;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ACadSharp.Examples
{
	class Program
	{
		static string PathSamples = "../../../../samples/local";

		static void Main(string[] args)
		{
			//ReadDxf();
			//ReadDwg();

			getInsertEntities("", "MyBlock");
		}

		static void ReadDxf()
		{
			string file = Path.Combine(PathSamples, "bin.dxf");
			DxfReader reader = new DxfReader(file, onNotification);
			reader.Read();
		}

		static void ReadDwg()
		{
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

		static IEnumerable<Insert> getInsertEntities(string file, string blockname)
		{
			CadDocument doc = DwgReader.Read(file);

			// Get the model space where all the drawing entities are
			BlockRecord modelSpace = doc.BlockRecords["*Model_Space"];

			// Get the insert instance that is using the block that you are looking for
			return modelSpace.Entities.OfType<Insert>().Where(e => e.Block.Name == blockname);
		}

		private static void onNotificationFail(object sender, NotificationEventArgs e)
		{
			Debug.Fail(e.Message);
		}

		private static void onNotification(object sender, NotificationEventArgs e)
		{
			Console.WriteLine(e.Message);
		}
	}
}
