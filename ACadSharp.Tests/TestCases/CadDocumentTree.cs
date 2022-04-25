using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.Tests.TestCases
{
	public class CadDocumentTree
	{
		public Node BlocksTable { get; set; }
	}

	public class Node
	{
		public ObjectType ObjectType { get; set; }

		public ulong Handle { get; set; }

		public ulong OwnerHandle { get; set; }

		public ulong DictionaryHandle { get; set; }

		public List<Node> Children { get; set; } = new List<Node>();

		public Node GetByHandle(ulong handle)
		{
			return this.Children.FirstOrDefault(x => x.Handle == handle);
		}
	}
}
