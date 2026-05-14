using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class AttributeDefinitionTests : CommonEntityTests<AttributeDefinition>
	{
		[Fact]
		public void AttConstructor()
		{
			AttributeEntity att = EntityFactory.CreateDefault<AttributeEntity>();

			AttributeDefinition attributeDefinition = new AttributeDefinition(att);

			EntityComparator.IsEqual(att, attributeDefinition);
		}

		public override void GetBoundingBoxTest()
		{
			AttributeDefinition att = new AttributeDefinition();
			att.InsertPoint = new CSMath.XYZ(5, 5, 5);

			BoundingBox box = att.GetBoundingBox();

			Assert.Equal(new BoundingBox(new CSMath.XYZ(5, 5, 5)), box);
		}
	}
}
