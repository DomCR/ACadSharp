using ACadSharp.Examples.Common;
using ACadSharp.IO;
using System;

namespace ACadSharp.Examples
{
	public static class WriterExamples
	{
		/// <summary>
		/// Write a ascii dxf file
		/// </summary>
		/// <param name="file"></param>
		/// <param name="doc"></param>
		public static void WriteAsciiDxf(string file, CadDocument doc)
		{
			using (DxfWriter writer = new DxfWriter(file, doc, false))
			{
				writer.OnNotification += NotificationHelper.LogConsoleNotification;
				writer.Write();
			}
		}

		/// <summary>
		/// Write a binary dxf file
		/// </summary>
		/// <param name="file"></param>
		/// <param name="doc"></param>
		public static void WriteBinaryDxf(string file, CadDocument doc)
		{
			using (DxfWriter writer = new DxfWriter(file, doc, true))
			{
				writer.OnNotification += NotificationHelper.LogConsoleNotification;
				writer.Write();
			}
		}
	}
}
