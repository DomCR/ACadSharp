using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public abstract class CommonEntityTests<T>
		where T : Entity, new()
	{
		[Fact]
		public void DefaultConstructor()
		{
			T entity = new T();

			Assert.NotNull(entity);
			Assert.True(0 == entity.Handle);

			Assert.NotEqual(ObjectType.UNDEFINED, entity.ObjectType);

			Assert.False(string.IsNullOrEmpty(entity.ObjectName));
			Assert.False(string.IsNullOrEmpty(entity.SubclassMarker));

			Assert.Null(entity.XDictionary);
		}

		[Fact]
		public virtual void BoundingBoxTest()
		{
			Entity entity = EntityFactory.Create(typeof(T));

			entity.GetBoundingBox();
		}
	}
}
