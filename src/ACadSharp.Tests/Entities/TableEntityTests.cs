using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using Xunit;

namespace ACadSharp.Tests.Entities;

public class TableEntityTests : CommonEntityTests<TableEntity>
{
	public override void CloneTest()
	{
		TableEntity table = new TableEntity();
		table.Version = 4;
		table.Style = new TableStyle("my_style");

		for (int c = 0; c < 2; c++)
		{
			table.Columns.Add(new TableEntity.Column { Name = $"col_{c}", Width = 10 + c });
		}

		for (int r = 0; r < 3; r++)
		{
			TableEntity.Row row = new TableEntity.Row { Height = 5 + r };
			for (int c = 0; c < 2; c++)
			{
				row.Cells.Add(new TableEntity.Cell { ToolTip = $"cell_{r}_{c}" });
			}

			table.Rows.Add(row);
		}

		TableEntity clone = (TableEntity)table.Clone();

		Assert.NotNull(clone);
		Assert.NotSame(table, clone);
		Assert.Equal(table.Version, clone.Version);

		//Style
		Assert.NotNull(clone.Style);
		Assert.NotSame(table.Style, clone.Style);
		Assert.Equal(table.Style.Name, clone.Style.Name);

		//Break data
		Assert.NotSame(table.BreakData, clone.BreakData);

		//Columns
		Assert.NotSame(table.Columns, clone.Columns);
		Assert.Equal(table.Columns.Count, clone.Columns.Count);
		for (int c = 0; c < table.Columns.Count; c++)
		{
			TableEntity.Column original = table.Columns[c];
			TableEntity.Column cloned = clone.Columns[c];

			Assert.NotSame(original, cloned);
			Assert.Equal(original.Name, cloned.Name);
			Assert.Equal(original.Width, cloned.Width);
			Assert.NotNull(cloned.Owner);
			Assert.NotSame(original.Owner, cloned.Owner);
		}

		//Rows and cells
		Assert.NotSame(table.Rows, clone.Rows);
		Assert.Equal(table.Rows.Count, clone.Rows.Count);
		for (int r = 0; r < table.Rows.Count; r++)
		{
			TableEntity.Row original = table.Rows[r];
			TableEntity.Row cloned = clone.Rows[r];

			Assert.NotSame(original, cloned);
			Assert.Equal(original.Height, cloned.Height);
			Assert.NotNull(cloned.Owner);
			Assert.NotSame(original.Owner, cloned.Owner);
			Assert.Same(clone.Rows[0].Owner, cloned.Owner);

			Assert.NotSame(original.Cells, cloned.Cells);
			Assert.Equal(original.Cells.Count, cloned.Cells.Count);
			for (int c = 0; c < original.Cells.Count; c++)
			{
				TableEntity.Cell originalCell = table.GetCell(r, c);
				TableEntity.Cell clonedCell = clone.GetCell(r, c);

				Assert.NotSame(originalCell, clonedCell);
				Assert.Equal(originalCell.ToolTip, clonedCell.ToolTip);
				Assert.Same(cloned.Owner, clonedCell.Owner);
			}
		}

		//Modifying the clone must not affect the original
		clone.Rows.Add(new TableEntity.Row());
		clone.GetCell(0, 0).ToolTip = "modified";

		Assert.Equal(3, table.Rows.Count);
		Assert.Equal("cell_0_0", table.GetCell(0, 0).ToolTip);
	}

	[Fact(Skip = "Not implemented")]
	public override void GetBoundingBoxTest()
	{
	}

	[Theory(Skip = "Not implemented")]
	[InlineData(CadFileFormat.DXF, ACadVersion.AC1014)]
	[InlineData(CadFileFormat.DXF, ACadVersion.AC1032)]
	[InlineData(CadFileFormat.DWG, ACadVersion.AC1032)]
	public override void ValidEntityTest(CadFileFormat format, ACadVersion version)
	{
	}
}