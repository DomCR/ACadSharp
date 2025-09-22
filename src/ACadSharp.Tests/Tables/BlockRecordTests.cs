using ACadSharp.Entities;
using ACadSharp.Extensions;
using ACadSharp.Objects;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using ACadSharp.Tests.Common;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Tables
{
	public class BlockRecordTests : TableEntryCommonTests<BlockRecord>
	{
		[Fact()]
		public void AddEntityTest()
		{
			string name = "my_block";
			BlockRecord record = new BlockRecord(name);

			Line l1 = new Line();
			Line l2 = new Line();
			Line l3 = new Line();
			Line l4 = new Line();

			record.Entities.Add(l1);
			record.Entities.Add(l2);
			record.Entities.Add(l3);
			record.Entities.Add(l4);

			foreach (Entity e in record.Entities)
			{
				Assert.Equal(record, e.Owner);
			}
		}

		[Fact()]
		public void BlockRecordTest()
		{
			string name = "my_block";
			BlockRecord record = new BlockRecord(name);

			Assert.Equal(name, record.Name);

			Assert.NotNull(record.BlockEntity);
			Assert.Equal(record.Name, record.BlockEntity.Name);

			Assert.NotNull(record.BlockEnd);
		}

		[Fact()]
		public void CloneDetachDocumentTest()
		{
			string name = "my_block";
			BlockRecord record = new BlockRecord(name);
			CadDocument doc = new CadDocument();

			doc.BlockRecords.Add(record);

			BlockRecord clone = (BlockRecord)record.Clone();

			Assert.Null(clone.Document);
			Assert.Null(clone.BlockEntity.Document);
			Assert.Null(clone.BlockEnd.Document);

			Assert.NotNull(record.Document);
			Assert.NotNull(record.BlockEntity.Document);
			Assert.NotNull(record.BlockEnd.Document);
		}

		[Fact()]
		public void CloneInDocumentTest()
		{
			string name = "my_block";
			BlockRecord record = new BlockRecord(name);
			CadDocument doc = new CadDocument();

			doc.BlockRecords.Add(record);

			Assert.NotNull(record.Document);
			Assert.NotNull(record.BlockEntity.Document);
			Assert.NotNull(record.BlockEnd.Document);
		}

		[Fact()]
		public void ClonePaperSpaceTest()
		{
			CadDocument doc = new CadDocument();

			BlockRecord record = doc.PaperSpace.CloneTyped();

			Assert.NotNull(record);
			Assert.Null(record.Layout);

			//Test the layout keeps the block
			Layout paper = doc.Layouts["Layout1"];
			Layout layout = paper.CloneTyped();

			Assert.NotNull(layout);
			Assert.NotNull(layout.AssociatedBlock);
		}

		[Fact()]
		public void CloneSortensTableTest()
		{
			string name = "my_block";
			BlockRecord record = new BlockRecord(name);

			Line l1 = new Line();
			Line l2 = new Line();
			Line l3 = new Line();
			Line l4 = new Line();
			Line l5 = new Line();

			record.Entities.Add(l1);
			record.Entities.Add(l2);
			record.Entities.Add(l3);
			record.Entities.Add(l4);
			record.Entities.Add(l5);

			record.CreateSortEntitiesTable();

			record.SortEntitiesTable.Add(l1, 1);
			record.SortEntitiesTable.Add(l3, 3);
			record.SortEntitiesTable.Add(l4, 4);

			BlockRecord clone = record.CloneTyped();

			Assert.NotNull(clone.SortEntitiesTable);
			Assert.NotEmpty(clone.SortEntitiesTable);
			Assert.Equal(3, clone.SortEntitiesTable.Count());
		}

		[Fact()]
		public void CloneTest()
		{
			string name = "my_block";
			BlockRecord record = new BlockRecord(name);

			Line l1 = new Line();
			Line l2 = new Line();
			Line l3 = new Line();
			Line l4 = new Line();

			record.Entities.Add(l1);
			record.Entities.Add(l2);
			record.Entities.Add(l3);
			record.Entities.Add(l4);

			BlockRecord clone = record.Clone() as BlockRecord;

			CadObjectTestUtils.AssertTableEntryClone(record, clone);

			Assert.NotEqual(clone.BlockEntity.Owner, record);

			CadObjectTestUtils.AssertEntityCollection(record.Entities, clone.Entities);
		}

		[Fact()]
		public void CreateSortensTableTest()
		{
			string name = "my_block";
			BlockRecord record = new BlockRecord(name);

			record.Entities.Add(new Line());
			record.Entities.Add(new Line());
			record.Entities.Add(new Line());
			record.Entities.Add(new Line());

			record.CreateSortEntitiesTable();

			Assert.NotNull(record.SortEntitiesTable);
			Assert.Empty(record.SortEntitiesTable);
		}

		[Fact()]
		public void CreateXRef()
		{
			string name = "my_block";
			string path = Path.Combine(TestVariables.SamplesFolder, "sample_AC1032.dwg");
			BlockRecord record = new BlockRecord(name, path);

			Assert.Equal(name, record.Name);
			Assert.Equal(path, record.BlockEntity.XRefPath);
			Assert.True(record.Flags.HasFlag(Blocks.BlockTypeFlags.XRef));
			Assert.True(record.Flags.HasFlag(Blocks.BlockTypeFlags.XRefResolved));
			Assert.False(record.Flags.HasFlag(Blocks.BlockTypeFlags.XRefOverlay));

			BlockRecord overlay = new BlockRecord(name, path, true);

			Assert.Equal(name, overlay.Name);
			Assert.Equal(path, overlay.BlockEntity.XRefPath);
			Assert.True(overlay.Flags.HasFlag(Blocks.BlockTypeFlags.XRef));
			Assert.True(overlay.Flags.HasFlag(Blocks.BlockTypeFlags.XRefResolved));
			Assert.True(overlay.Flags.HasFlag(Blocks.BlockTypeFlags.XRefOverlay));
		}

		[Fact()]
		public void NotAllowDuplicatesTest()
		{
			string name = "my_block";
			BlockRecord record = new BlockRecord(name);

			Line l1 = new Line();

			record.Entities.Add(l1);
			Assert.Throws<ArgumentException>(() => record.Entities.Add(l1));
		}

		protected override Table<BlockRecord> getTable(CadDocument document)
		{
			return document.BlockRecords;
		}
	}
}