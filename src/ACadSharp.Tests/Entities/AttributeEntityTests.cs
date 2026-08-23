using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities;

public class AttributeEntityTests : CommonEntityTests<AttributeEntity>
{
	[Fact]
	public void AttributeDefinitionWithMText()
	{
		BlockRecord record = new("TestRecord");

		AttributeDefinition testAtt = new();
		testAtt.MText = new();

		record.Entities.Add(testAtt);
		Insert recordInsert = new(record);

		Assert.True(recordInsert.HasAttributes);
		Assert.True(recordInsert.Attributes.Count == 1);
		Assert.Contains(recordInsert.Attributes, attributeEntity => attributeEntity.MText is not null);
	}

	public override void GetBoundingBoxTest()
	{
		AttributeEntity att = new AttributeEntity();
		att.InsertPoint = new CSMath.XYZ(5, 5, 5);

		BoundingBox box = att.GetBoundingBox();

		Assert.Equal(new BoundingBox(new CSMath.XYZ(5, 5, 5)), box);
	}
}
