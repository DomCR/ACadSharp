using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using System;
using Xunit;

namespace ACadSharp.Tests
{
	public class CadObjectTests
	{
		public static readonly TheoryData<Type> EntityTypes;

		static CadObjectTests()
		{
			EntityTypes = new TheoryData<Type>();

			foreach (var item in DataFactory.GetTypes<Entity>())
			{
				EntityTypes.Add(item);
			}
		}

		[Theory]
		[MemberData(nameof(EntityTypes))]
		public void Clone(Type entityType)
		{
			Entity entity = EntityFactory.Create(entityType);
			Entity clone = (Entity)entity.Clone();

			CadObjectTestUtils.AssertEntityClone(entity, clone);
		}

		[Fact]
		public void CloneArc()
		{
			Arc arc = new Arc();
			Arc a = arc.CloneT();


		}
	}
}
