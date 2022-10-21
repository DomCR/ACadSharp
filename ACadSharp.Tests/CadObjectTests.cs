using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			Entity entity = (Entity)Activator.CreateInstance(entityType);
			Entity copy = (Entity)Activator.CreateInstance(entityType);

			CadObjectTestUtils.AssertEntityClone(entity, copy);
		}
	}
}
