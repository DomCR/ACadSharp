using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.Tests.TestCases
{
	public class CadDocumentTree
	{
		public TableNode AppIdsTable { get; set; }

		public TableNode BlocksTable { get; set; }

		public TableNode DimensionStylesTable { get; set; }

		public TableNode LayersTable { get; set; }

		public TableNode LineTypesTable { get; set; }

		public TableNode TextStylesTable { get; set; }

		public TableNode UCSsTable { get; set; }

		public TableNode ViewsTable { get; set; }

		public TableNode VPortsTable { get; set; }
	}

	public class Node
	{
		public string ACadName { get; set ; }

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
			return $"{this.ACadName}";
		}
	}

	public class TableNode : Node
	{
		public List<TableEntryNode> Entries { get; set; } = new List<TableEntryNode>();

		public TableEntryNode GetEntry(ulong handle)
		{
			return this.Entries.FirstOrDefault(x => x.Handle == handle);
		}

		public TableEntryNode GetEntry(string name)
		{
			return this.Entries.FirstOrDefault(x => x.Name == name);
		}
	}

	public class TableEntryNode : Node
	{
		public string Name { get; set; }

		public override string ToString()
		{
			return $"{this.ACadName}:{this.Name}";
		}
	}
}
