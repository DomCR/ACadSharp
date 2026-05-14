using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal interface ICadOwnerTemplate : ICadObjectTemplate
	{
		public HashSet<ulong> OwnedObjectsHandlers { get; }
	}
}
