using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Exceptions
{
	[Serializable]
	public class DxfException : Exception
	{
		/// <summary>
		/// Invalid Dxf code.
		/// </summary>
		/// <param name="code"></param>
		/// <param name="line"></param>
		public DxfException(int code, long line) : base($"Invalid dxf code with value {code}, at line {line}.") { }

		/// <summary>
		/// Generic exception.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="line"></param>
		public DxfException(string message, long line) : base($"{message}, at line {line}.") { }

		public DxfException(string message) : base(message) { }
	}
}
