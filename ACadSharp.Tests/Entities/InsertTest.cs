using ACadSharp.Entities;
using ACadSharp.Tables;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class InsertTest
	{
		private readonly string _blockName = "mock_record";

		[Fact]
		public void CreateInsert()
		{
			BlockRecord record = new BlockRecord(_blockName);
			Insert insert = new Insert(record);

			Assert.NotNull(insert);
			Assert.NotNull(insert.Block);
			Assert.Empty(insert.Attributes);
		}

		[Fact]
		public void AddInsertToDocument()
		{
			BlockRecord record = new BlockRecord(_blockName);
			Insert insert = new Insert(record);

			CadDocument document = new CadDocument();

			document.Entities.Add(insert);

			Assert.Equal(document, insert.Document);
			Assert.Equal(document, insert.Block.Document);
			Assert.Equal(record.Name, insert.Block.Name);
			Assert.Equal(record, insert.Block);
			Assert.True(insert.Handle != 0);
			Assert.True(document.BlockRecords.Contains(_blockName));
		}

		[Fact]
		public void CreateUnlinkedInsert()
		{
			CadDocument document = new CadDocument();
			BlockRecord record = new BlockRecord(_blockName);

			document.BlockRecords.Add(record);

			Insert insert = new Insert(record);

			Assert.Null(insert.Document);
			Assert.Null(insert.Block.Document);
			Assert.NotEqual(record, insert.Block);
		}

		[Fact]
		public void AddInsertToDocumentWithExistingBlock()
		{
			CadDocument document = new CadDocument();
			BlockRecord record = new BlockRecord(_blockName);

			document.BlockRecords.Add(record);

			Insert insert = new Insert(record);

			document.Entities.Add(insert);

			Assert.Equal(document, insert.Document);
			Assert.Equal(document, insert.Block.Document);
			Assert.True(document.BlockRecords.Contains(_blockName));
		}

		[Fact]
		public void LinkedAttributes()
		{
			BlockRecord record = new BlockRecord(_blockName);
			record.Entities.Add(new AttributeDefinition());

			Insert insert = new Insert(record);

			Assert.True(record.AttributeDefinitions.Count() == insert.Attributes.Count);
		}
	}
}
