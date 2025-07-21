using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using System;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	[Obsolete]
	public class EntityTests
	{
		public static readonly TheoryData<Type> EntityTypes = new TheoryData<Type>();

		static EntityTests()
		{
			foreach (var item in DataFactory.GetTypes<Entity>())
			{
				if (item == typeof(UnknownEntity)
					|| item == typeof(PdfUnderlay))
				{
					continue;
				}

				EntityTypes.Add(item);
			}
		}

		[Theory]
		[MemberData(nameof(EntityTypes))]
		public void DefaultConstructor(Type t)
		{
			CadObject cadObject = Factory.CreateObject(t, false);

			Assert.NotNull(cadObject);
			Assert.True(0 == cadObject.Handle);

			Assert.NotEqual(ObjectType.UNDEFINED, cadObject.ObjectType);

			Assert.False(string.IsNullOrEmpty(cadObject.ObjectName));
			Assert.False(string.IsNullOrEmpty(cadObject.SubclassMarker));

			Assert.Null(cadObject.XDictionary);
		}

		[Theory]
		[MemberData(nameof(EntityTypes))]
		public void BoundingBoxTest(Type entityType)
		{
			if (entityType == typeof(Spline))
			{
				return;
			}

			Entity entity = EntityFactory.Create(entityType);

			entity.GetBoundingBox();
		}

		[Theory]
		[MemberData(nameof(EntityTypes))]
		public void Clone(Type entityType)
		{
			Entity entity = EntityFactory.Create(entityType);
			Entity clone = (Entity)entity.Clone();

			CadObjectTestUtils.AssertEntityClone(entity, clone);
		}
	}
}
