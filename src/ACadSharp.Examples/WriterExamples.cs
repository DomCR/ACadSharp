using ACadSharp.Examples.Common;
using ACadSharp.IO;

namespace ACadSharp.Examples
{
	public static class WriterExamples
	{
		/// <summary>
		/// Write a binary dxf file
		/// </summary>
		/// <param name="file">file path</param>
		/// <param name="doc"></param>
		/// <param name="binary">if the file has to be in binary</param>
		public static void WriteDxf(string file, CadDocument doc, bool binary)
		{
			using (DxfWriter writer = new DxfWriter(file, doc, binary))
			{
				writer.OnNotification += NotificationHelper.LogConsoleNotification;
				writer.Write();
			}
		}

		/// <summary>
		/// Write a dwg file
		/// </summary>
		/// <param name="file">file path</param>
		/// <param name="doc"></param>
		public static void WriteDwg(string file, CadDocument doc)
		{
			using (DwgWriter writer = new DwgWriter(file, doc))
			{
				writer.OnNotification += NotificationHelper.LogConsoleNotification;
				writer.Write();
			}
		}
	}
}
