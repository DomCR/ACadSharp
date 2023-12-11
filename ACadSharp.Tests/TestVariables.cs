using CSUtilities;

namespace ACadSharp.Tests
{
	public static class TestVariables
	{
		public static bool LocalEnv { get { return EnvironmentVars.Get<bool>("LOCAL_ENV"); } }

		public static double Delta { get { return EnvironmentVars.Get<double>("DELTA"); } }

		public static int DecimalPrecision { get { return EnvironmentVars.Get<int>("DECIMAL_PRECISION"); } }

		public static bool DxfAutocadConsoleCheck { get { return EnvironmentVars.Get<bool>("DXF_CONSOLE_CHECK"); } }

		public static bool DwgAutocadConsoleCheck { get { return EnvironmentVars.Get<bool>("DWG_CONSOLE_CHECK"); } }

		public static bool RunDwgWriterSingleCases { get { return EnvironmentVars.Get<bool>("RUN_DWG_WRITER_SINGLE_CASES_TEST"); } }

		static TestVariables()
		{
			EnvironmentVars.SetIfNull("LOCAL_ENV", "true");
			EnvironmentVars.SetIfNull("DELTA", "0.00001");
			EnvironmentVars.SetIfNull("DECIMAL_PRECISION", "5");
			EnvironmentVars.SetIfNull("DXF_CONSOLE_CHECK", "true");
			EnvironmentVars.SetIfNull("DWG_CONSOLE_CHECK", "true");
			EnvironmentVars.SetIfNull("RUN_DWG_WRITER_SINGLE_CASES_TEST", "true");
		}
	}
}
