using ACadSharp.Entities;
using ACadSharp.Tables;
using System;
using Xunit;

namespace ACadSharp.Tests.Objects
{
	public class SortEntitiesTableTests
	{
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
}
