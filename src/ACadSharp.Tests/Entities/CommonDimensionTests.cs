using ACadSharp.Entities;
using ACadSharp.Tables;
using System;
using Xunit;

namespace ACadSharp.Tests.Entities;

public abstract class CommonDimensionTests<T> : CommonEntityTests<T>
	where T : Dimension, new()
{
	public abstract DimensionType Type { get; }

	[Fact]
	public void DimensionStyleOverride()
	{
		T dim = new T();
		DimensionStyle style = new DimensionStyle();
		style.ScaleFactor = 2.0;

		dim.SetDimensionOverride(style);

		var current = dim.GetActiveDimensionStyle();

		Assert.NotNull(current);
		Assert.Equal("override", current.Name);
		Assert.Equal(style.ScaleFactor, current.ScaleFactor);
	}

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