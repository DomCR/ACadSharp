using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal interface ICadTableTemplate : ICadObjectTemplate
	{
		HashSet<ulong> EntryHandles { get; }
	}
}
