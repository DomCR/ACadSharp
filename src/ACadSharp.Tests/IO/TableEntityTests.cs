using Xunit;
using Xunit.Abstractions;
using ACadSharp.Tests.TestModels;
using ACadSharp.Entities;
using System.Linq;
using ACadSharp.Tables;

namespace ACadSharp.Tests.IO
{
	public class TableEntityTests : IOTestsBase
	{
		public static TheoryData<FileModel> TableSamplesFilePaths { get; } = new();

		static TableEntityTests()
		{
			loadSamples("table_samples", "*", TableSamplesFilePaths);
		}

		public TableEntityTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(TableSamplesFilePaths))]
		public void ReadTableEntity(FileModel test)
		{
			CadDocument doc = this.readDocument(test);

			TableEntity table = (TableEntity)doc.Entities.First();

			BlockRecord record = table.Block;

			Assert.NotNull(record);
			Assert.Equal("*T1", record.Name);

			Assert.Equal(5, table.Columns.Count);
			foreach (var column in table.Columns)
			{
				Assert.Equal(2.5, column.Width);
			}

			Assert.Equal(4, table.Rows.Count);
			//First row
			TableEntity.Cell titleCell = table.GetCell(0, 0);
			Assert.False(titleCell.HasMultipleContent);
			Assert.Equal("Hello this is a title", titleCell.Content.Value.FormatedValue);

			TableEntity.Cell next = table.GetCell(0, 1);
		}
	}
}
