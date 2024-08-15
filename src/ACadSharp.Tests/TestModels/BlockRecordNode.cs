using System.Collections.Generic;

namespace ACadSharp.Tests.TestModels
{
	public class BlockRecordNode : TableEntryNode
	{
		public List<EntityNode> Entities { get; set; } = new List<EntityNode>();
	}
}
