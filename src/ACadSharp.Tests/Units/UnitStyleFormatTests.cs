using ACadSharp.Tables;
using ACadSharp.Types.Units;
using Xunit;

namespace ACadSharp.Tests.Units
{
	public class UnitStyleFormatTests
	{
		[Fact]
		public void ZeroHandlingFormatTest()
		{
			UnitStyleFormat style = new();

			//Test linear
			style.LinearDecimalPlaces = 2;
			style.LinearZeroHandling = ZeroHandling.SuppressZeroFeetAndInches;
			Assert.Equal("0.00", style.GetZeroHandlingFormat());

			style.LinearDecimalPlaces = 5;
			style.LinearZeroHandling = ZeroHandling.SuppressDecimalTrailingZeroes;
			Assert.Equal("0.#####", style.GetZeroHandlingFormat());

			style.LinearDecimalPlaces = 1;
			style.LinearZeroHandling = ZeroHandling.SuppressDecimalLeadingAndTrailingZeroes;
			Assert.Equal("#.#", style.GetZeroHandlingFormat());

			style.LinearDecimalPlaces = 1;
			style.LinearZeroHandling = ZeroHandling.SuppressDecimalLeadingZeroes;
			Assert.Equal("#.0", style.GetZeroHandlingFormat());
		}
	}
}
