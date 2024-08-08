using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Tests.TestModels
{
	public class TableNode<T> : Node
		where T : TableEntryNode
	{
		public List<T> Entries { get; set; } = new List<T>();

		public TableEntryNode GetEntry(ulong handle)
		{
			return this.Entries.FirstOrDefault(x => x.Handle == handle);
		}

		public TableEntryNode GetEntry(string name)
		{
			return this.Entries.FirstOrDefault(x => x.Name == name);
		}
	}
}
