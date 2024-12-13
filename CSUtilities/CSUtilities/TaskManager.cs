using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CSUtilities
{
	/// <summary>
	/// Class to control tasks and processes.
	/// </summary>
	internal static class TaskManager
	{
		/// <summary>
		/// Execute a method in a time span if the method don't finish at the time is canceled.
		/// Example:
		///     Action method = () => TaskExample(10);
		///     bool test = TaskManager.ExecuteWithTimeLimit(3000, method);
		///     if the action isn't finish in 3 seconds will stop the execution and return false.
		/// </summary>
		/// <param name="timeSpan">Time in milliseconds.</param>
		/// <param name="codeBlock">Block of code, or method to execute in the timespan.</param>
		/// <returns>bool indicating if the action has finished.</returns>
		public static bool ExecuteWithTimeLimit(int timeSpan, Action codeBlock)
		{
			Thread thread = new Thread(new ThreadStart(codeBlock));
			thread.Start();

			if (!thread.Join(timeSpan))
			{
				thread.Abort();
				return false;
			}

			return true;
		}
	}
}
