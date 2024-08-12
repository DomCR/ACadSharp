using System;

namespace ACadSharp.Exceptions
{
	[Serializable]
	public class DwgException : Exception
	{
		public DwgException(string message) : base(message) { }

		public DwgException(string message, Exception inner) : base(message, inner) { }

		protected DwgException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
