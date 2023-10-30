using System;

namespace ACadSharp.Tests
{
	public static class TestVariables
	{
		public const double Delta = 0.00001d;

		public const int DecimalPrecision = 5;

		public const bool AutocadConsoleCheck = true;

		static TestVariables()
		{
			EnvironmentVars.SetIfNull("", "");
		}
	}

	public static class EnvironmentVars
	{
		public static void Set(string name, string value)
		{
			Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
		}

		public static void SetIfNull(string name, string value)
		{
			if (Get(name) == null)
			{
				Set(name, value);
			}
		}

		public static string Get(string name)
		{
			return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
		}

		public static void Delete(string name)
		{
			Environment.SetEnvironmentVariable(name, null, EnvironmentVariableTarget.Process);
		}
	}
}
