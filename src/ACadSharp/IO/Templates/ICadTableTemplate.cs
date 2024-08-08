using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal interface ICadTableTemplate : ICadObjectTemplate
	{
		List<ulong> EntryHandles { get; }
	}
}
