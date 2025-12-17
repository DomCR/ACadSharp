using ACadSharp.Blocks;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadBlockEntityTemplate : CadEntityTemplate, ICadOwnerTemplate
	{
		public HashSet<ulong> OwnedObjectsHandlers { get; } = new();

		public CadBlockEntityTemplate(Block entity) : base(entity)
		{
		}
	}
}