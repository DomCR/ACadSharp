using ACadSharp.Tables;
using ACadSharp.Types.Units;
using System.Globalization;
using Xunit;

namespace ACadSharp.Tests.Units;

public class UnitStyleFormatTests
{
	[Fact]
	public void ToScientificUsesDecimalSeparator()
	{
		UnitStyleFormat style = new();
		style.LinearDecimalPlaces = 2;
		style.LinearZeroHandling = ZeroHandling.SuppressDecimalLeadingZeroes;

		CultureInfo current = CultureInfo.CurrentCulture;
		try
		{
			CultureInfo.CurrentCulture = new CultureInfo("de-DE");
			Assert.Equal("1.03E+01", style.ToScientific(10.3));

			CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
			style.DecimalSeparator = ",";
			Assert.Equal("1,03E+01", style.ToScientific(10.3));
		}
		finally
		{
			CultureInfo.CurrentCulture = current;
		}
	}

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
