using ACadSharp.Entities;
using Xunit;

namespace ACadSharp.Tests.Entities;

public class TableEntityTests
{
	[Fact]
	public void RowCountShouldFollowRowsCollection()
	{
		TableEntity table = new TableEntity();

		Assert.Equal((ushort)1, table.RowCount);

		table.Rows.Add(new TableEntity.Row());
		table.Rows.Add(new TableEntity.Row());

		Assert.Equal((ushort)2, table.RowCount);
	}

	[Fact]
	public void ColumnCountShouldFollowColumnsCollection()
	{
		TableEntity table = new TableEntity();

		Assert.Equal((ushort)1, table.ColumnCount);

		table.Columns.Add(new TableEntity.Column());
		table.Columns.Add(new TableEntity.Column());
		table.Columns.Add(new TableEntity.Column());

		Assert.Equal((ushort)3, table.ColumnCount);
	}
}
