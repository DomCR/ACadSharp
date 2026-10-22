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
		style.ToleranceZeroHandling = ZeroHandling.ShowZeroFeetSuppressZeroInches;

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

		// [PATCH] DimensionType keeps the raw group-code-70 type values (0-6) in the low
		// three bits, so HasFlag(DimensionType.Angular) is also true for Diameter(3) and
		// Ordinate(6) because of the shared bit. IsAngular now compares the masked type code,
		// so it is true only for genuine angular dimensions. Assert against the real type.
		bool expected = this.Type == DimensionType.Angular || this.Type == DimensionType.Angular3Point;
		Assert.Equal(expected, dim.IsAngular);
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