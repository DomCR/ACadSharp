using CSMath;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ACadSharp.Tests.Common
{
	public static class AssertUtils
	{
		public static void AreEqual<T>(T expected, T actual, string varname)
		{
			switch (expected, actual)
			{
				case (double d1, double d2):
					Assert.AreEqual(d1, d2, TestVariables.Delta, $"Different {varname}");
					break;
				case (XY xy1, XY xy2):
					Assert.IsTrue(xy1.IsEqual(xy2, TestVariables.DecimalPrecision), $"Different {varname}");
					break;
				case (XYZ xyz1, XYZ xyz2):
					Assert.IsTrue(xyz1.IsEqual(xyz2, TestVariables.DecimalPrecision), $"Different {varname}");
					break;
				default:
					Assert.AreEqual(expected, actual, $"Different {varname}");
					break;
			}
		}
	}
}
