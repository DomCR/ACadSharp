using System;

namespace ACadSharp.Exceptions
{
	[Serializable]
	public class DwgNotSupportedException : Exception
	{
		public DwgNotSupportedException() : base($"Dwg version not recognised") { }

		public DwgNotSupportedException(ACadVersion version) : base($"Dwg version not supported: {version}") { }
	}
}
