using CSUtilities.Text;
using System.Globalization;
using Xunit;

namespace ACadSharp.Tests.Internal;

public class CadUtilsTests
{
	[Theory]
	[InlineData("en-US")]
	[InlineData("tr-TR")]
	[InlineData("az-Latn-AZ")]
	public void GetCodePageIgnoresCurrentCulture(string cultureName)
	{
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		try
		{
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

			Assert.Equal(CodePage.Windows1254, CadUtils.GetCodePage("ANSI_1254"));
			Assert.Equal(CodePage.Windows1254, CadUtils.GetCodePage("ansi_1254"));
			Assert.Equal(CodePage.Windows1254, CadUtils.GetCodePage("AnSi_1254"));
		}
		finally
		{
			CultureInfo.CurrentCulture = originalCulture;
		}
	}
}
