using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO;

/// <summary>
/// A TableEntity added to a document must survive a DXF write-read round trip.
/// </summary>
public class TableEntityDxfWriteTests : IOTestsBase
{
	public TableEntityDxfWriteTests(ITestOutputHelper output) : base(output) { }

	public static TheoryData<ACadVersion> Versions { get; } = new TheoryData<ACadVersion>
	{
		ACadVersion.AC1027,
		ACadVersion.AC1032,
	};

	[Theory]
	[MemberData(nameof(Versions))]
	public void WriteReadTableEntityAscii(ACadVersion version)
	{
		string path = Path.Combine(TestVariables.OutputSingleCasesFolder, $"out_table_entity_{version}_ascii.dxf");

		this.runRoundTrip(version, path, false);
	}

	[Theory]
	[MemberData(nameof(Versions))]
	public void WriteReadTableEntityBinary(ACadVersion version)
	{
		string path = Path.Combine(TestVariables.OutputSingleCasesFolder, $"out_table_entity_{version}_binary.dxf");

		this.runRoundTrip(version, path, true);
	}

	private void runRoundTrip(ACadVersion version, string path, bool binary)
	{
		CadDocument doc = this.createDocumentWithTable(version);

		using (DxfWriter writer = new DxfWriter(path, doc, binary))
		{
			writer.Write();
		}

		CadDocument readed = new DxfReader(path).Read();

		TableEntity result = Assert.Single(readed.Entities.OfType<TableEntity>());

		Assert.NotNull(result.Block);
		Assert.Equal(2, result.Rows.Count);
		Assert.Equal(3, result.Columns.Count);

		Assert.Equal("R0C0", result.GetCell(0, 0).Content.CadValue.Value);
		Assert.Equal("R1C2", result.GetCell(1, 2).Content.CadValue.Value);
	}

	private CadDocument createDocumentWithTable(ACadVersion version)
	{
		CadDocument doc = new CadDocument();
		doc.Header.Version = version;

		BlockRecord block = new BlockRecord("*T1")
		{
			IsAnonymous = true,
		};
		doc.BlockRecords.Add(block);

		TableEntity table = new TableEntity(block);
		doc.Entities.Add(table);

		table.Style = doc.TableStyles.GetEntry(TableStyle.DefaultName);

		table.Rows.Add(new TableEntity.Row());
		table.Rows.Add(new TableEntity.Row());
		table.Columns.Add(new TableEntity.Column());
		table.Columns.Add(new TableEntity.Column());
		table.Columns.Add(new TableEntity.Column());

		for (int r = 0; r < 2; r++)
		{
			for (int c = 0; c < 3; c++)
			{
				TableEntity.Cell cell = new TableEntity.Cell();
				TableEntity.CellContent content = new TableEntity.CellContent();
				content.CadValue.SetValue($"R{r}C{c}", CadValueType.String);
				cell.Contents.Add(content);
				table.Rows[r].Cells.Add(cell);
			}
		}

		return doc;
	}
}
