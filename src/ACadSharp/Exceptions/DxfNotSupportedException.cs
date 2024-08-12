using System;

namespace ACadSharp.Exceptions
{
	[Serializable]
	public class DxfNotSupportedException : NotSupportedException
	{
		public DxfNotSupportedException() : base($"Dxf version not recognised") { }

		public DxfNotSupportedException(ACadVersion version) : base($"Dxf version not supported: {version}") { }
	}
}
