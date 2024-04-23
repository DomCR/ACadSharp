using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
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

		public static void NotNull<T>(T o, string info = null)
		{
			Assert.IsNotNull(o != null, $"Object of type {typeof(T)} should not be null:  {info}");
		}

		public static void EntryNotNull<T>(Table<T> table, string entry)
			where T : TableEntry
		{
			var record = table[entry];
			Assert.IsTrue(record != null, $"Entry with name {entry} is null for table {table}");
			Assert.IsNotNull(record.Document);
		}
	}
}
