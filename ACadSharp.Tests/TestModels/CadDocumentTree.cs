using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.Tests.TestModels
{
	public class CadDocumentTree
	{
		public TableNode<TableEntryNode> AppIdsTable { get; set; }

		public TableNode<BlockRecordNode> BlocksTable { get; set; }

		public TableNode<TableEntryNode> DimensionStylesTable { get; set; }

		public TableNode<TableEntryNode> LayersTable { get; set; }

		public TableNode<TableEntryNode> LineTypesTable { get; set; }

		public TableNode<TableEntryNode> TextStylesTable { get; set; }

		public TableNode<TableEntryNode> UCSsTable { get; set; }

		public TableNode<TableEntryNode> ViewsTable { get; set; }

		public TableNode<TableEntryNode> VPortsTable { get; set; }
	}

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
			return $"{this.ACadName}";
		}
	}

	public class EntityNode : Node
	{
		public string LayerName { get; set; }

		public bool IsInvisible { get; set; }

		public Transparency Transparency { get; set; }

		public string LinetypeName { get; set; }

		public double LinetypeScale { get; set; }

		public LineweightType LineWeight { get; set; }
	}

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

	public class TableEntryNode : Node
	{
		public string Name { get; set; }

		public override string ToString()
		{
			return $"{this.ACadName}:{this.Name}";
		}
	}

	public class BlockRecordNode : TableEntryNode
	{
		public List<EntityNode> Entities { get; set; } = new List<EntityNode>();
	}
}
