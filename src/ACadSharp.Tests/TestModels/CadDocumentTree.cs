namespace ACadSharp.Tests.TestModels
{
	public class CadDocumentTree
	{
		public TableNode<TableEntryNode> AppIdsTable { get; set; }

		public TableNode<BlockRecordNode> BlocksTable { get; set; }

		public TableNode<TableEntryNode> DimensionStylesTable { get; set; }

		public TableNode<LayerNode> LayersTable { get; set; }

		public TableNode<TableEntryNode> LineTypesTable { get; set; }

		public TableNode<TableEntryNode> TextStylesTable { get; set; }

		public TableNode<TableEntryNode> UCSsTable { get; set; }

		public TableNode<TableEntryNode> ViewsTable { get; set; }

		public TableNode<TableEntryNode> VPortsTable { get; set; }
	}
}
