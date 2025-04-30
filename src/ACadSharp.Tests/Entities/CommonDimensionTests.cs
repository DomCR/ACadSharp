using ACadSharp.Entities;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public abstract class CommonDimensionTests<T>
		where T : Dimension, new()
	{
		public abstract DimensionType Type { get; }

		[Fact]
		public void DimensionTypeTest()
		{
			T dim = new T();

			Assert.True(dim.Flags.HasFlag(this.Type));
			Assert.True(dim.Flags.HasFlag(DimensionType.BlockReference));
		}

		[Fact]
		public void IsTextUserDefinedLocationTest()
		{
			T dim = new T();

			Assert.False(dim.Flags.HasFlag(DimensionType.TextUserDefinedLocation));

			dim.IsTextUserDefinedLocation = true;

			Assert.True(dim.Flags.HasFlag(DimensionType.TextUserDefinedLocation));
		}

		[Fact]
		public virtual void UpdateBlockTests()
		{
			T dim = new T();

			Assert.Null(dim.Block);

			dim.UpdateBlock();

			Assert.NotNull(dim.Block);
			Assert.True(dim.Block.IsAnonymous);
		}
	}
}
