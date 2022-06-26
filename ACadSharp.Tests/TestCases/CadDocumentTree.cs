using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.Tests.TestCases
{
	public class CadDocumentTree
	{
		public Node AppIdsTable { get; set; }

		public Node BlocksTable { get; set; }

		public Node DimensionStylesTable { get; set; }

		public Node LayersTable { get; set; }

		public Node LineTypesTable { get; set; }

		public Node TextStylesTable { get; set; }

		public Node UCSsTable { get; set; }

		public Node ViewsTable { get; set; }

		public Node VPortsTable { get; set; }
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

	public class TableEntryNode : Node
	{
		public string Name { get; set; }
	}
}
