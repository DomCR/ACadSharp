using ACadSharp.Entities;
using ACadSharp.Extensions;
using ACadSharp.Tables;
using ACadSharp.Tests.Common;
using CSMath;
using System;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Entities;

public class InsertTest
{
	private readonly string _blockName = "mock_record";

	[Fact]
	public void AddInsertToDocument()
	{
		BlockRecord record = new BlockRecord(this._blockName);
		Insert insert = new Insert(record);

		CadDocument document = new CadDocument();

		document.Entities.Add(insert);

		Assert.Equal(document, insert.Document);
		Assert.Equal(document, insert.Block.Document);
		Assert.Equal(record.Name, insert.Block.Name);
		Assert.Equal(record, insert.Block);
		Assert.True(insert.Handle != 0);
		Assert.True(document.BlockRecords.Contains(this._blockName));
	}

	[Fact]
	public void AddInsertToDocumentWithExistingBlock()
	{
		CadDocument document = new CadDocument();
		BlockRecord record = new BlockRecord(this._blockName);

		document.BlockRecords.Add(record);

		Insert insert = new Insert(record);

		document.Entities.Add(insert);

		Assert.Equal(document, insert.Document);
		Assert.Equal(document, insert.Block.Document);
		Assert.True(document.BlockRecords.Contains(this._blockName));
	}

	[Fact]
	public void CloneTest()
	{
		BlockRecord record = new BlockRecord(this._blockName);
		Insert insert = new Insert(record);
		insert.InsertPoint = new CSMath.XYZ(10, 10, 0);

		var att1 = new AttributeEntity
		{
			Value = "This is an attribute",
			InsertPoint = new CSMath.XYZ(5, 5, 0)
		};
		insert.Attributes.Add(att1);

		Assert.Equal(new XYZ(15, 15, 0), att1.InsertPoint);

		Insert clone = insert.CloneTyped();

		CadObjectTestUtils.AssertEntityClone(insert, clone);
		CadObjectTestUtils.AssertEntityCollection(insert.Attributes, clone.Attributes);

		var att2 = new AttributeEntity
		{
			Value = "This is an attribute",
			InsertPoint = new CSMath.XYZ(5, 5, 0)
		};
		clone.Attributes.Add(att2);
		Assert.Equal(new XYZ(15, 15, 0), att2.InsertPoint);
	}

	[Fact]
	public void CreateInsert()
	{
		BlockRecord record = new BlockRecord(this._blockName);
		Insert insert = new Insert(record);

		Assert.NotNull(insert);
		Assert.NotNull(insert.Block);
		Assert.Empty(insert.Attributes);
	}

	[Fact]
	public void CreateUnlinkedInsert()
	{
		CadDocument document = new CadDocument();
		BlockRecord record = new BlockRecord(this._blockName);

		document.BlockRecords.Add(record);

		Insert insert = new Insert(record);

		Assert.Null(insert.Document);
		Assert.Null(insert.Block.Document);
		Assert.NotEqual(record, insert.Block);
	}

	[Fact]
	public void LinkedAttributes()
	{
		BlockRecord record = new BlockRecord(this._blockName);

		var attdef = new AttributeDefinition
		{
			Tag = "TEST",
			InsertPoint = new CSMath.XYZ(5, 5, 0)
		};
		record.Entities.Add(attdef);
		record.Entities.Add(new AttributeDefinition());
		record.Entities.Add(new AttributeDefinition());

		Insert insert = new Insert(record);
		AttributeEntity att = insert.Attributes.First(a => a.Tag == attdef.Tag);

		Assert.True(record.AttributeDefinitions.Count() == insert.Attributes.Count);
		Assert.NotNull(att);
		Assert.Equal(attdef.InsertPoint, att.InsertPoint);
	}

	[Fact]
	public void ScaleRange()
	{
		BlockRecord record = new BlockRecord(this._blockName);
		Insert insert = new Insert(record);

		insert.XScale = 5;
		insert.YScale = 5;
		insert.ZScale = 5;

		//Negative
		insert.XScale = -1;
		insert.YScale = -1;
		insert.ZScale = -1;

		//Zero
		Assert.Throws<ArgumentOutOfRangeException>(() => insert.XScale = 0);
		Assert.Throws<ArgumentOutOfRangeException>(() => insert.YScale = 0);
		Assert.Throws<ArgumentOutOfRangeException>(() => insert.ZScale = 0);
	}
}