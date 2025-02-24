using CSMath;
using Xunit;

namespace ACadSharp.Tests.Common
{
	public static class AssertUtils
	{
		public static void AreEqual<T>(T expected, T actual, string varname = null)
		{
			switch (expected, actual)
			{
				case (double d1, double d2):
					Assert.Equal(d1, d2, TestVariables.Delta);
					break;
				case (XY xy1, XY xy2):
					Assert.True(xy1.IsEqual(xy2, TestVariables.DecimalPrecision), $"Different {varname}");
					break;
				case (XYZ xyz1, XYZ xyz2):
					Assert.True(xyz1.IsEqual(xyz2, TestVariables.DecimalPrecision), $"Different {varname}");
					break;
				default:
					Assert.Equal(expected, actual);
					break;
			}
		}
	}
}
