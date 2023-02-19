using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ACadSharp.Tests.Common
{
	public static class AssertUtils
	{
		public static void AreEqual<T>(T expected, T actual, string varname)
		{
			Assert.AreEqual(expected, actual, $"Different {varname}");
		}

		public static void AreEqual(double expected, double actual, double delta, string varname)
		{
			Assert.AreEqual(expected, actual, delta, $"Different {varname}");
		}
	}
}
