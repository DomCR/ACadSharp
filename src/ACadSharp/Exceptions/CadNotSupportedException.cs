using System;

namespace ACadSharp.Exceptions
{
	[Serializable]
	public class CadNotSupportedException : NotSupportedException
	{
		public CadNotSupportedException() : base($"File version not recognized") { }

		public CadNotSupportedException(ACadVersion version) : base($"File version not supported: {version}") { }
	}
}
