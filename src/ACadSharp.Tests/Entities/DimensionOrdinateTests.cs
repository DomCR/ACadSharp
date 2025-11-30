using ACadSharp.Entities;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class DimensionOrdinateTests : CommonDimensionTests<DimensionOrdinate>
	{
		public override DimensionType Type => DimensionType.Ordinate;

		[Fact]
		public void IsOrdinateTypeXTest()
		{
			DimensionOrdinate aligned = new DimensionOrdinate();

			Assert.False(aligned.Flags.HasFlag(DimensionType.OrdinateTypeX));

			aligned.IsOrdinateTypeX = true;

			Assert.True(aligned.Flags.HasFlag(DimensionType.OrdinateTypeX));
		}
	}
}
