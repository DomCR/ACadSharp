using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Tests.TestModels
{
	public class Node
	{
		public string ACadName { get; set; }

		public ulong Handle { get; set; }

		public ulong OwnerHandle { get; set; }

		public ulong DictionaryHandle { get; set; }

		public List<Node> Children { get; set; } = new List<Node>();

		public Node GetChild(ulong handle)
		{
			return this.Children.FirstOrDefault(x => x.Handle == handle);
		}

		public override string ToString()
		{
			return $"{this.ACadName}:{this.Handle}";
		}
	}
}
