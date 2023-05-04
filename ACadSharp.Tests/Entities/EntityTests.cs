using ACadSharp.Entities;
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

		[Theory]
		[MemberData(nameof(EntityTypes))]
		public void CloneUnattachEvent(Type t)
		{
			Entity entity = EntityFactory.Create(t);
			entity.OnReferenceChanged += this.entity_OnReferenceChanged;

			Entity clone = (Entity)entity.Clone();

			CadObjectTestUtils.AssertEntityClone(entity, clone);
		}

		private void entity_OnReferenceChanged(object sender, ReferenceChangedEventArgs e)
		{
			//The clone must not have any attachment
			throw new InvalidOperationException();
		}
	}
}
