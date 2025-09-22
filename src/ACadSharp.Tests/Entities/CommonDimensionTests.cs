using ACadSharp.Entities;
using System;
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
		public void DimStyleNotNull()
		{
			T dim = new T();

			Assert.NotNull(dim.Style);
			Assert.Throws<ArgumentNullException>(() => dim.Style = null);
		}

		[Fact]
		public void IsAngularTest()
		{
			T dim = this.createDim();

			Assert.Equal(dim.Flags.HasFlag(DimensionType.Angular) || dim.Flags.HasFlag(DimensionType.Angular3Point), dim.IsAngular);
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
			T dim = this.createDim();

			Assert.Null(dim.Block);

			dim.UpdateBlock();

			Assert.NotNull(dim.Block);
			Assert.True(dim.Block.IsAnonymous);
		}

		protected virtual T createDim()
		{
			return new T();
		}
	}
}