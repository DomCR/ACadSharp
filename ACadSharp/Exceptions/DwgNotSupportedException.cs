using System;

namespace ACadSharp.Exceptions
{
	[Serializable]
	public class DwgNotSupportedException : NotSupportedException
	{
		public DwgNotSupportedException() : base($"Dwg version not recognised") { }

		public DwgNotSupportedException(ACadVersion version) : base($"Dwg version not supported: {version}") { }
	}

	[Serializable]
	public class DxfNotSupportedException : NotSupportedException
	{
		public DxfNotSupportedException() : base($"Dxf version not recognised") { }

		public DxfNotSupportedException(ACadVersion version) : base($"Dxf version not supported: {version}") { }
	}
}
