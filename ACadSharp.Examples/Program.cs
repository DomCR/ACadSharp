using ACadSharp.Examples.Common;
using ACadSharp.IO;
using System;

namespace ACadSharp.Examples
{
	class Program
	{
		const string file = "../../../../samples/sample_AC1021.dwg";

		static void Main(string[] args)
		{
			CadDocument doc;
			using (DwgReader reader = new DwgReader(file, NotificationHelper.LogConsoleNotification))
			{
				doc = reader.Read();
			}
		}

		/// <summary>
		/// Logs in the console the document information
		/// </summary>
		/// <param name="doc"></param>
		static void ExploreDocument(CadDocument doc)
		{
			Console.WriteLine("SUMMARY INFO:");


			foreach (var item in doc.AppIds)
			{

			}
		}
	}
}
