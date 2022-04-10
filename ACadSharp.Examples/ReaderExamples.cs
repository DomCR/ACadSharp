using ACadSharp.IO;
using ACadSharp.IO.DWG;
using ACadSharp.IO.DXF;
using System;

namespace ACadSharp.Examples
{
	public static class ReaderExamples
	{
		public static void ReadDxf(string file)
		{
			using (DxfReader reader = new DxfReader(file, onNotification))
			{
				CadDocument doc = reader.Read();
			}
		}

		public static void ReadDwg(string file)
		{
			using (DwgReader reader = new DwgReader(file, onNotification))
			{
				CadDocument doc = reader.Read();
			}
		}

		private static void onNotification(object sender, NotificationEventArgs e)
		{
			Console.WriteLine(e.Message);
		}
	}
}
