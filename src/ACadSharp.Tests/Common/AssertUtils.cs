using CSMath;
using Xunit;

namespace ACadSharp.Tests.Common
{
	public static class AssertUtils
	{
		public static void AreEqual<T>(T expected, T actual, string varname = null)
		{
			AreEqual(expected, actual, TestVariables.DecimalPrecision, varname);
		}

		public static void AreEqual<T>(T expected, T actual, int precision, string varname = null)
		{
			switch (expected, actual)
			{
				case (double d1, double d2):
					Assert.Equal(d1, d2, TestVariables.Delta);
					break;
				case (XY xy1, XY xy2):
					Assert.True(xy1.IsEqual(xy2, precision),
						$"Different {varname}\n" +
						$"expected: {expected}\n" +
						$"actual:   {actual}");
					break;
				case (XYZ xyz1, XYZ xyz2):
					Assert.True(xyz1.IsEqual(xyz2, precision),
						$"Different {varname}\n" +
						$"expected: {expected}\n" +
						$"actual:   {actual}");
					break;
				default:
					Assert.Equal(expected, actual);
					break;
			}
		}
	}
}
