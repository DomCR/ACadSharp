using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.Tests.Common;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public abstract class CommonEntityTests<T>
		where T : Entity, new()
	{
		[Fact]
		public virtual void BoundingBoxTest()
		{
			Entity entity = EntityFactory.Create(typeof(T));

			entity.GetBoundingBox();
		}

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
		public void GetActiveColorTest()
		{
			BlockRecord owner = new BlockRecord("owner");
			owner.BlockEntity.Color = new Color(20);

			Entity entity = EntityFactory.Create(typeof(T), false);
			entity.Layer.Color = new Color(30);

			owner.Entities.Add(entity);

			Assert.Equal(entity.Layer.Color, entity.GetActiveColor());

			entity.Color = Color.ByBlock;
			Assert.Equal(owner.BlockEntity.Color, entity.GetActiveColor());

			entity.Color = new Color(40);
			Assert.Equal(entity.Color, entity.GetActiveColor());
		}

		[Fact]
		public void GetActiveLineTypeTest()
		{
			BlockRecord owner = new BlockRecord("owner");
			owner.BlockEntity.LineType = new LineType("block_linetype");

			Entity entity = EntityFactory.Create(typeof(T), false);
			entity.Layer.LineType = new LineType("layer_linetype");

			owner.Entities.Add(entity);

			Assert.Equal(entity.Layer.LineType, entity.GetActiveLineType());

			entity.LineType = LineType.ByBlock;
			Assert.Equal(owner.BlockEntity.LineType, entity.GetActiveLineType());

			entity.LineType = new LineType("entity_linetype");
			Assert.Equal(entity.LineType, entity.GetActiveLineType());
		}

		[Fact]
		public void GetActiveLineWeightTypeTest()
		{
			BlockRecord owner = new BlockRecord("owner");
			owner.BlockEntity.LineWeight = LineWeightType.W20;

			Entity entity = EntityFactory.Create(typeof(T), false);
			entity.Layer.LineWeight = LineWeightType.W50;

			owner.Entities.Add(entity);

			Assert.Equal(entity.Layer.LineWeight, entity.GetActiveLineWeightType());

			entity.LineWeight = LineWeightType.ByBlock;
			Assert.Equal(owner.BlockEntity.LineWeight, entity.GetActiveLineWeightType());

			entity.LineWeight = LineWeightType.W70;
			Assert.Equal(entity.LineWeight, entity.GetActiveLineWeightType());
		}
	}
}