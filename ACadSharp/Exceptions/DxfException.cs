using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Exceptions
{

	[Serializable]
	public class DxfException : Exception
	{
		public DxfException() { }

		/// <summary>
		/// Invalid Dxf code.
		/// </summary>
		/// <param name="code"></param>
		/// <param name="line"></param>
		public DxfException(int code, long line) : base($"Invalid dxf code with value {code}, at line {line}.") { }
		public DxfException(string message) : base(message) { }
		public DxfException(string message, Exception inner) : base(message, inner) { }
		protected DxfException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
