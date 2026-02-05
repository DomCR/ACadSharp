using System.Collections.Generic;

namespace ACadSharp.Tests.TestModels
{
	public class BlockRecordNode : TableEntryNode
	{
		public bool IsAnonymous { get; set; }

		public bool IsDynamic { get; set; }

		public List<EntityNode> Entities { get; set; } = new List<EntityNode>();
	}
}
