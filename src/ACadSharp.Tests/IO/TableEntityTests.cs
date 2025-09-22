using Xunit;
using Xunit.Abstractions;
using ACadSharp.Tests.TestModels;
using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.IO;

namespace ACadSharp.Tests.IO
{
	public class TableEntityTests : IOTestsBase
	{
		public static TheoryData<FileModel> TableSamplesFilePaths { get; } = new();

		static TableEntityTests()
		{
			loadSamples("./", "dxf", TableSamplesFilePaths);
			loadSamples("./", "dwg", TableSamplesFilePaths);
		}

		public TableEntityTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(TableSamplesFilePaths))]
		public void ReadTableEntity(FileModel test)
		{
			CadReaderConfiguration configuration;

			if (test.IsDxf)
			{
				configuration = new DxfReaderConfiguration();
			}
			else
			{
				configuration = new DwgReaderConfiguration();
			}

			configuration.KeepUnknownNonGraphicalObjects = true;

			CadDocument doc = this.readDocument(test, configuration);

			if(doc.Header.Version <= ACadVersion.AC1021)
			{
				return;
			}

			TableEntity table = doc.GetCadObject<TableEntity>(0xA35);

			BlockRecord record = table.Block;

			Assert.NotNull(record);
			Assert.Equal("*T16", record.Name);

			Assert.Equal(5, table.Columns.Count);
			foreach (var column in table.Columns)
			{
				Assert.Equal(2.5, column.Width);
			}

			Assert.Equal(4, table.Rows.Count);
			//First row
			TableEntity.Cell titleCell = table.GetCell(0, 0);
			Assert.False(titleCell.HasMultipleContent);
			Assert.Equal("Hello this is a title", titleCell.Content.Value.Value);

			TableEntity.Cell next = table.GetCell(0, 1);
		}
	}
}
