﻿using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using System;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class EntityTests
	{
		public static readonly TheoryData<Type> EntityTypes = new TheoryData<Type>();

		static EntityTests()
		{
			foreach (var item in DataFactory.GetTypes<Entity>())
			{
				if(item == typeof(UnknownEntity))
				{
					continue;
				}

				EntityTypes.Add(item);
			}
		}

		[Theory]
		[MemberData(nameof(EntityTypes))]
		public void BoundingBoxTest(Type entityType)
		{
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
