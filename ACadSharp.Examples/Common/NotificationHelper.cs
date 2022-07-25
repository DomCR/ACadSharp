using ACadSharp.IO;
using System;

namespace ACadSharp.Examples.Common
{
	public class NotificationHelper
	{
		public static void LogConsoleNotification(object sender, NotificationEventArgs e)
		{
			//Write in the console all the messages
			Console.WriteLine(e.Message);
		}
	}
}
