using ACadSharp.Entities;
using System;
using Xunit;

namespace ACadSharp.Tests.Entities;

public class TableEntityTests
{
	[Fact]
	public void RowCountShouldFollowRowsCollection()
	{
		TableEntity table = new TableEntity();

		Assert.Equal((ushort)0, table.RowCount);

		table.Rows.Add(new TableEntity.Row());
		table.Rows.Add(new TableEntity.Row());

		Assert.Equal((ushort)2, table.RowCount);
	}

	[Fact]
	public void ColumnCountShouldFollowColumnsCollection()
	{
		TableEntity table = new TableEntity();

		Assert.Equal((ushort)0, table.ColumnCount);

		table.Columns.Add(new TableEntity.Column());
		table.Columns.Add(new TableEntity.Column());
		table.Columns.Add(new TableEntity.Column());

		Assert.Equal((ushort)3, table.ColumnCount);
	}

	[Fact]
	public void SetCellTextShouldFillCellValue()
	{
		TableEntity table = new TableEntity();

		table.Rows.Add(new TableEntity.Row());
		table.Rows.Add(new TableEntity.Row());

		table.Columns.Add(new TableEntity.Column());
		table.Columns.Add(new TableEntity.Column());
		table.Columns.Add(new TableEntity.Column());

		table.SetCellText(0, 0, "A1");
		table.SetCellText(1, 2, "C2");

		Assert.Equal("A1", table.GetCell(0, 0).Content.CadValue.Value);
		Assert.Equal("C2", table.GetCell(1, 2).Content.CadValue.Value);

		Assert.Equal(CadValueType.String, table.GetCell(0, 0).Content.CadValue.ValueType);
	}

	[Fact]
	public void GetOrCreateCellShouldFillMissingCells()
	{
		TableEntity table = new TableEntity();

		table.Rows.Add(new TableEntity.Row());
		table.Columns.Add(new TableEntity.Column());
		table.Columns.Add(new TableEntity.Column());

		TableEntity.Cell cell = table.GetOrCreateCell(0, 1);

		Assert.NotNull(cell);
		Assert.Equal(2, table.Rows[0].Cells.Count);
	}

	[Fact]
	public void GetOrCreateCellShouldThrowOutsideTheGrid()
	{
		TableEntity table = new TableEntity();

		table.Rows.Add(new TableEntity.Row());
		table.Columns.Add(new TableEntity.Column());

		Assert.Throws<ArgumentOutOfRangeException>(() => table.GetOrCreateCell(1, 0));
		Assert.Throws<ArgumentOutOfRangeException>(() => table.GetOrCreateCell(0, 1));
		Assert.Throws<ArgumentOutOfRangeException>(() => table.GetOrCreateCell(-1, 0));
	}
}
