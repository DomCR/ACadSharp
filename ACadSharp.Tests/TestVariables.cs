using CSUtilities;
using System.IO;

namespace ACadSharp.Tests
{
	public static class TestVariables
	{
		public static string SamplesFolder { get { return EnvironmentVars.Get<string>("SAMPLES_FOLDER"); } }
	
		public static string OutputSamplesFolder { get { return EnvironmentVars.Get<string>("OUTPUT_SAMPLES_FOLDER"); } }

		public static string OutputSingleCasesFolder { get { return EnvironmentVars.Get<string>("OUTPUT_SINGLE_CASES_FOLDER"); } }

		public static bool LocalEnv { get { return EnvironmentVars.Get<bool>("LOCAL_ENV"); } }

		public static double Delta { get { return EnvironmentVars.Get<double>("DELTA"); } }

		public static int DecimalPrecision { get { return EnvironmentVars.Get<int>("DECIMAL_PRECISION"); } }

		public static bool RunDwgWriterSingleCases { get { return EnvironmentVars.Get<bool>("RUN_DWG_WRITER_SINGLE_CASES_TEST"); } }

		static TestVariables()
		{
			EnvironmentVars.SetIfNull("SAMPLES_FOLDER", "../../../../samples/");
			EnvironmentVars.SetIfNull("OUTPUT_SAMPLES_FOLDER", "../../../../samples/out");
			EnvironmentVars.SetIfNull("OUTPUT_SINGLE_CASES_FOLDER", "../../../../samples/out/single_cases");
			EnvironmentVars.SetIfNull("LOCAL_ENV", "true");
			EnvironmentVars.SetIfNull("DELTA", "0.00001");
			EnvironmentVars.SetIfNull("DECIMAL_PRECISION", "5");
			EnvironmentVars.SetIfNull("RUN_DWG_WRITER_SINGLE_CASES_TEST", "true");
		}

		public static void CreateOutputFolders()
		{
			if (!Directory.Exists(OutputSamplesFolder))
			{
				Directory.CreateDirectory(OutputSamplesFolder);
			}

			if (!Directory.Exists(OutputSingleCasesFolder))
			{
				Directory.CreateDirectory(OutputSingleCasesFolder);
			}
		}
	}
}
