using ACadSharp.Entities;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class DimensionTests
	{
		[Fact]
		public void DimensionTypeTest()
		{
			DimensionAligned aligned = new DimensionAligned();

			Assert.True(aligned.Flags.HasFlag(DimensionType.Aligned));
			Assert.True(aligned.Flags.HasFlag(DimensionType.BlockReference));
		}

		[Fact]
		public void IsOrdinateTypeXTest()
		{
			DimensionOrdinate aligned = new DimensionOrdinate();
			
			Assert.False(aligned.Flags.HasFlag(DimensionType.OrdinateTypeX));
			
			aligned.IsOrdinateTypeX = true;

			Assert.True(aligned.Flags.HasFlag(DimensionType.OrdinateTypeX));
		}

		[Fact]
		public void IsTextUserDefinedLocationTest()
		{
			DimensionAligned aligned = new DimensionAligned();

			Assert.False(aligned.Flags.HasFlag(DimensionType.TextUserDefinedLocation));

			aligned.IsTextUserDefinedLocation = true;

			Assert.True(aligned.Flags.HasFlag(DimensionType.TextUserDefinedLocation));
		}
	}
}
