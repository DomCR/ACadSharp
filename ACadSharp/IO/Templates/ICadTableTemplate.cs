using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal interface ICadTableTemplate : ICadObjectTemplate
	{
		public CadObject CadObject { get; set; }

		List<ulong> EntryHandles { get; }
	}
}
