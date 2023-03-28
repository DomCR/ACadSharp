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
			Assert.True(document.BlockRecords.Contains(_blockName));
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

		[Fact(Skip = "Feature to be implemented")]
		public void ChangeBlock()
		{
			BlockRecord record = new BlockRecord(_blockName);
			BlockRecord record2 = new BlockRecord(_blockName);
			Insert insert = new Insert(record);
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
