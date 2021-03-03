using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO
{
	public delegate void ProgressEventHandler(object sender, ProgressEventArgs e);

	public class ProgressEventArgs : EventArgs
	{
		public float Progress { get; }
		public string Message { get; set; }
		public string Error { get; set; }
		public ProgressEventArgs(float progress, string message)
		{
			Progress = progress;
			Message = message;
		}
	}
}
