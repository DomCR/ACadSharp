using ACadSharp.Examples.Common;
using ACadSharp.IO;

namespace ACadSharp.Examples
{
	public static class ReaderExamples
	{
		/// <summary>
		/// Read a dxf file
		/// </summary>
		/// <param name="file">dxf file path</param>
		public static void ReadDxf(string file)
		{
			using (DxfReader reader = new DxfReader(file))
			{
				reader.OnNotification += NotificationHelper.LogConsoleNotification;
				CadDocument doc = reader.Read();
			}
		}

		/// <summary>
		/// Read a dwg file
		/// </summary>
		/// <param name="file">dwg file path</param>
		public static void ReadDwg(string file)
		{
			using (DwgReader reader = new DwgReader(file))
			{
				reader.OnNotification += NotificationHelper.LogConsoleNotification;
				CadDocument doc = reader.Read();
			}
		}
	}
}
