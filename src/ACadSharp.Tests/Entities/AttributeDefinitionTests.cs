using ACadSharp.Entities;
using ACadSharp.Tests.Common;
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

		public override void CloneTest()
		{
			throw new System.NotImplementedException();
		}

		public override void GetBoundingBoxTest()
		{
			throw new System.NotImplementedException();
		}
	}
}
