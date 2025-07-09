using ACadSharp.Tables;
using System;
using Xunit;

namespace ACadSharp.Tests.Tables.Collections
{
	public class BlockRecordsTableTests
	{
		[Fact]
		public void AddAnonymousBlocks()
		{
			CadDocument doc = new CadDocument();

			BlockRecord record = new BlockRecord("anonymous") { IsAnonymous = true };
			BlockRecord record1 = new BlockRecord("anonymous") { IsAnonymous = true };
			BlockRecord record2 = new BlockRecord("anonymous") { IsAnonymous = true };

			doc.BlockRecords.Add(record);

			Assert.Contains(record, doc.BlockRecords);

			doc.BlockRecords.Add(record1);

			Assert.Contains(record1, doc.BlockRecords);
			Assert.Equal("*A0", record1.Name);

			doc.BlockRecords.Add(record2);

			Assert.Contains(record2, doc.BlockRecords);
			Assert.Equal("*A1", record2.Name);

			Assert.ThrowsAny<ArgumentException>(() => doc.BlockRecords.Add(record1));
		}
	}
}