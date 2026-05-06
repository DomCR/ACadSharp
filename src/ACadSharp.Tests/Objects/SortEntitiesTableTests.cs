using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Objects;

public class SortEntitiesTableTests
{
	[Fact]
	public void MoveToBottom_EntityAlreadyInTable_UpdatesHandleHigherThanMax()
	{
		BlockRecord record = new BlockRecord("test_block");
		Line l1 = new Line();
		Line l2 = new Line();

		record.Entities.Add(l1);
		record.Entities.Add(l2);

		record.CreateSortEntitiesTable();
		SortEntitiesTable table = record.SortEntitiesTable;

		ulong handle1 = 10UL;
		ulong handle2 = 20UL;

		table.Add(l1, handle1);
		table.Add(l2, handle2);

		table.MoveToBottom(l1);

		ulong l1SorterHandle = table.GetSorterHandle(l1);

		Assert.True(l1SorterHandle > handle1);
		Assert.True(l1SorterHandle > handle2);
	}

	[Fact]
	public void MoveToBottom_EntityIsLastInEnumeration()
	{
		BlockRecord record = new BlockRecord("test_block");
		Line l1 = new Line();
		Line l2 = new Line();
		Line l3 = new Line();

		record.Entities.Add(l1);
		record.Entities.Add(l2);
		record.Entities.Add(l3);

		record.CreateSortEntitiesTable();
		SortEntitiesTable table = record.SortEntitiesTable;

		table.Add(l1, 10UL);
		table.Add(l2, 20UL);
		table.Add(l3, 30UL);

		table.MoveToBottom(l1);

		var sorted = table.ToList();

		Assert.Equal(l1, sorted[sorted.Count - 1].Entity);
	}

	[Fact]
	public void MoveToBottom_EntityNotInTable_AddsWithHandleHigherThanMax()
	{
		BlockRecord record = new BlockRecord("test_block");
		Line l1 = new Line();
		Line l2 = new Line();
		Line l3 = new Line();

		record.Entities.Add(l1);
		record.Entities.Add(l2);
		record.Entities.Add(l3);

		record.CreateSortEntitiesTable();
		SortEntitiesTable table = record.SortEntitiesTable;

		ulong handle1 = 10UL;
		ulong handle2 = 20UL;

		table.Add(l1, handle1);
		table.Add(l2, handle2);

		table.MoveToBottom(l3);

		ulong l3SorterHandle = table.GetSorterHandle(l3);
		ulong maxExisting = Math.Max(handle1, handle2);

		Assert.True(l3SorterHandle > maxExisting);
		Assert.True(record.GetSortedEntities().Last() == l3);
	}

	[Fact]
	public void MoveToTop_EntityAlreadyInTable_UpdatesHandleLowerThanMin()
	{
		BlockRecord record = new BlockRecord("test_block");
		Line l1 = new Line();
		Line l2 = new Line();

		record.Entities.Add(l1);
		record.Entities.Add(l2);

		record.CreateSortEntitiesTable();
		SortEntitiesTable table = record.SortEntitiesTable;

		ulong handle1 = 10UL;
		ulong handle2 = 20UL;

		table.Add(l1, handle1);
		table.Add(l2, handle2);

		table.MoveToTop(l1);

		ulong l1SorterHandle = table.GetSorterHandle(l1);

		Assert.True(l1SorterHandle < handle1);
		Assert.True(l1SorterHandle < handle2);
	}

	[Fact]
	public void MoveToTop_EntityIsFirstInEnumeration()
	{
		BlockRecord record = new BlockRecord("test_block");
		Line l1 = new Line();
		Line l2 = new Line();
		Line l3 = new Line();

		record.Entities.Add(l1);
		record.Entities.Add(l2);
		record.Entities.Add(l3);

		record.CreateSortEntitiesTable();
		SortEntitiesTable table = record.SortEntitiesTable;

		table.Add(l1, 10UL);
		table.Add(l2, 20UL);
		table.Add(l3, 30UL);

		table.MoveToTop(l3);

		var sorted = table.ToList();

		Assert.Equal(l3, sorted[0].Entity);
	}

	[Fact]
	public void MoveToTop_EntityNotInTable_AddsWithHandleLowerThanMin()
	{
		BlockRecord record = new BlockRecord("test_block");
		Line l1 = new Line();
		Line l2 = new Line();
		Line l3 = new Line();

		record.Entities.Add(l1);
		record.Entities.Add(l2);
		record.Entities.Add(l3);

		record.CreateSortEntitiesTable();
		SortEntitiesTable table = record.SortEntitiesTable;

		ulong handle1 = 10UL;
		ulong handle2 = 20UL;

		table.Add(l1, handle1);
		table.Add(l2, handle2);

		table.MoveToTop(l3);

		ulong l3SorterHandle = table.GetSorterHandle(l3);
		ulong minExisting = Math.Min(handle1, handle2);

		Assert.True(l3SorterHandle < minExisting);
		Assert.True(record.GetSortedEntities().First() == l3);
	}

	[Fact]
	public void OneStepDown_CalledUntilBottom_EntityReachesBottom()
	{
		BlockRecord record = new BlockRecord("test_block");
		Line l1 = new Line();
		Line l2 = new Line();
		Line l3 = new Line();

		record.Entities.Add(l1);
		record.Entities.Add(l2);
		record.Entities.Add(l3);

		record.CreateSortEntitiesTable();
		SortEntitiesTable table = record.SortEntitiesTable;

		table.Add(l1, 10UL);
		table.Add(l2, 20UL);
		table.Add(l3, 30UL);

		// Move l1 down twice to reach the bottom
		table.OneStepDown(l1);
		table.OneStepDown(l1);

		Assert.Equal(l1, table.Last().Entity);
	}

	[Fact]
	public void OneStepDown_EntityAlreadyAtBottom_NoActionTaken()
	{
		BlockRecord record = new BlockRecord("test_block");
		Line l1 = new Line();
		Line l2 = new Line();
		Line l3 = new Line();

		record.Entities.Add(l1);
		record.Entities.Add(l2);
		record.Entities.Add(l3);

		record.CreateSortEntitiesTable();
		SortEntitiesTable table = record.SortEntitiesTable;

		table.Add(l1, 10UL);
		table.Add(l2, 20UL);
		table.Add(l3, 30UL);

		ulong bottomHandleBefore = table.Last().SortHandle;

		table.OneStepDown(l3);

		ulong bottomHandleAfter = table.Last().SortHandle;

		Assert.Equal(l3, table.Last().Entity);
		Assert.Equal(bottomHandleBefore, bottomHandleAfter);
	}

	[Fact]
	public void OneStepDown_EntityAtTop_MovesDownOneStep()
	{
		BlockRecord record = new BlockRecord("test_block");
		Line l1 = new Line();
		Line l2 = new Line();
		Line l3 = new Line();

		record.Entities.Add(l1);
		record.Entities.Add(l2);
		record.Entities.Add(l3);

		record.CreateSortEntitiesTable();
		SortEntitiesTable table = record.SortEntitiesTable;

		table.Add(l1, 10UL);
		table.Add(l2, 20UL);
		table.Add(l3, 30UL);

		// Before: l1(10) < l2(20) < l3(30)
		table.OneStepDown(l1);

		var sorted = table.ToList();

		// After: l1 swaps with l2 → l2 < l1 < l3
		Assert.Equal(l2, sorted[0].Entity);
		Assert.Equal(l1, sorted[1].Entity);
		Assert.Equal(l3, sorted[2].Entity);
	}

	[Fact]
	public void OneStepDown_EntityInMiddle_MovesDownOneStep()
	{
		BlockRecord record = new BlockRecord("test_block");
		Line l1 = new Line();
		Line l2 = new Line();
		Line l3 = new Line();

		record.Entities.Add(l1);
		record.Entities.Add(l2);
		record.Entities.Add(l3);

		record.CreateSortEntitiesTable();
		SortEntitiesTable table = record.SortEntitiesTable;

		table.Add(l1, 10UL);
		table.Add(l2, 20UL);
		table.Add(l3, 30UL);

		// Before: l1(10) < l2(20) < l3(30)
		table.OneStepDown(l2);

		var sorted = table.ToList();

		// After: l2 swaps with l3 → l1 < l3 < l2
		Assert.Equal(l1, sorted[0].Entity);
		Assert.Equal(l3, sorted[1].Entity);
		Assert.Equal(l2, sorted[2].Entity);
	}

	[Fact]
	public void OneStepDown_EntityNotInTable_NoActionTaken()
	{
		var blk = this.createBlock();
		SortEntitiesTable table = blk.SortEntitiesTable;

		var handlesBefore = table.Select(s => s.SortHandle).ToArray();

		var outsider = new Point();
		blk.Entities.Add(outsider);
		// outsider is NOT added to the sort table

		table.OneStepDown(outsider);

		var handlesAfter = table.Select(s => s.SortHandle).ToArray();

		Assert.Equal(handlesBefore, handlesAfter);
	}

	[Fact]
	public void OneStepUp_CalledUntilTop_EntityReachesTop()
	{
		BlockRecord record = new BlockRecord("test_block");
		Line l1 = new Line();
		Line l2 = new Line();
		Line l3 = new Line();

		record.Entities.Add(l1);
		record.Entities.Add(l2);
		record.Entities.Add(l3);

		record.CreateSortEntitiesTable();
		SortEntitiesTable table = record.SortEntitiesTable;

		table.Add(l1, 10UL);
		table.Add(l2, 20UL);
		table.Add(l3, 30UL);

		// Move l3 up twice to reach the top
		table.OneStepUp(l3);
		table.OneStepUp(l3);

		Assert.Equal(l3, table.First().Entity);
	}

	[Fact]
	public void OneStepUp_EntityAlreadyAtTop_NoActionTaken()
	{
		BlockRecord record = new BlockRecord("test_block");
		Line l1 = new Line();
		Line l2 = new Line();
		Line l3 = new Line();

		record.Entities.Add(l1);
		record.Entities.Add(l2);
		record.Entities.Add(l3);

		record.CreateSortEntitiesTable();
		SortEntitiesTable table = record.SortEntitiesTable;

		table.Add(l1, 10UL);
		table.Add(l2, 20UL);
		table.Add(l3, 30UL);

		ulong topHandleBefore = table.First().SortHandle;

		table.OneStepUp(l1);

		ulong topHandleAfter = table.First().SortHandle;

		Assert.Equal(l1, table.First().Entity);
		Assert.Equal(topHandleBefore, topHandleAfter);
	}

	[Fact]
	public void OneStepUp_EntityAtBottom_MovesUpOneStep()
	{
		BlockRecord record = new BlockRecord("test_block");
		Line l1 = new Line();
		Line l2 = new Line();
		Line l3 = new Line();

		record.Entities.Add(l1);
		record.Entities.Add(l2);
		record.Entities.Add(l3);

		record.CreateSortEntitiesTable();
		SortEntitiesTable table = record.SortEntitiesTable;

		table.Add(l1, 10UL);
		table.Add(l2, 20UL);
		table.Add(l3, 30UL);

		// Before: l1(10) < l2(20) < l3(30)
		table.OneStepUp(l3);

		var sorted = table.ToList();

		// After: l3 swaps with l2 → l1 < l3 < l2
		Assert.Equal(l1, sorted[0].Entity);
		Assert.Equal(l3, sorted[1].Entity);
		Assert.Equal(l2, sorted[2].Entity);
	}

	[Fact]
	public void OneStepUp_EntityInMiddle_MovesUpOneStep()
	{
		BlockRecord record = new BlockRecord("test_block");
		Line l1 = new Line();
		Line l2 = new Line();
		Line l3 = new Line();

		record.Entities.Add(l1);
		record.Entities.Add(l2);
		record.Entities.Add(l3);

		record.CreateSortEntitiesTable();
		SortEntitiesTable table = record.SortEntitiesTable;

		table.Add(l1, 10UL);
		table.Add(l2, 20UL);
		table.Add(l3, 30UL);

		// Before: l1(10) < l2(20) < l3(30)
		table.OneStepUp(l2);

		var sorted = table.ToList();

		// After: l2 swaps with l1 → l2 < l1 < l3
		Assert.Equal(l2, sorted[0].Entity);
		Assert.Equal(l1, sorted[1].Entity);
		Assert.Equal(l3, sorted[2].Entity);
	}

	[Fact]
	public void OneStepUp_EntityNotInTable_NoActionTaken()
	{
		var blk = this.createBlock();
		SortEntitiesTable table = blk.SortEntitiesTable;

		var handlesBefore = table.Select(s => s.SortHandle).ToArray();

		var outsider = new Point();
		blk.Entities.Add(outsider);
		// outsider is NOT added to the sort table

		table.OneStepUp(outsider);

		var handlesAfter = table.Select(s => s.SortHandle).ToArray();

		Assert.Equal(handlesBefore, handlesAfter);
	}

	[Fact]
	public void SortersOrderTest()
	{
		var blk = this.createBlock();

		ulong before = 0;
		foreach (var sorter in blk.SortEntitiesTable)
		{
			Assert.True(sorter.SortHandle > before);
			before = sorter.SortHandle;
		}
	}

	protected BlockRecord createBlock()
	{
		Random rnd = new Random();
		var blk = new BlockRecord("my_block");
		var sort = blk.CreateSortEntitiesTable();

		for (ulong i = 0; i < 5; i++)
		{
			var pt = new Point();
			blk.Entities.Add(pt);
			sort.Add(pt, (ulong)rnd.Next(1, int.MaxValue));
		}

		return blk;
	}
}