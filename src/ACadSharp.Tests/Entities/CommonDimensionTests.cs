using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.Tests.Common;
using System;
using System.Globalization;
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
		public void GetMeasurementTextTests()
		{
			T dim = this.createDim();

			string text;

			if (dim.IsAngular)
			{
				text = dim.GetMeasurementText(new DimensionStyle
				{
					AngularUnit = Types.Units.AngularUnitFormat.DecimalDegrees
				});
			}
			else
			{
				text = dim.GetMeasurementText(new DimensionStyle
				{
					LinearUnitFormat = Types.Units.LinearUnitFormat.Scientific
				});

				Assert.True(double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double result));
				AssertUtils.AreEqual(dim.Measurement, result);
			}
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

		protected virtual T createDim()
		{
			return new T();
		}
	}
}