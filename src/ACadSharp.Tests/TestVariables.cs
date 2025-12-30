using CSUtilities;
using System;
using System.IO;

namespace ACadSharp.Tests
{
	public static class TestVariables
	{
		public static int DecimalPrecision { get { return EnvironmentVars.Get<int>("DECIMAL_PRECISION"); } }

		public static double Delta { get { return EnvironmentVars.Get<double>("DELTA"); } }

		public static string DesktopFolder { get { return Environment.GetFolderPath(Environment.SpecialFolder.Desktop); } }

		public static bool LocalEnv { get { return EnvironmentVars.Get<bool>("LOCAL_ENV"); } }

		public static string OutputSamplesFolder { get { return EnvironmentVars.Get<string>("OUTPUT_SAMPLES_FOLDER"); } }

		public static string OutputSingleCasesFolder { get { return EnvironmentVars.Get<string>("OUTPUT_SINGLE_CASES_FOLDER"); } }

		public static string OutputSvgFolder { get { return EnvironmentVars.Get<string>("OUTPUT_SVG"); } }

		public static bool SaveOutputInStream { get { return EnvironmentVars.Get<bool>("SAVE_OUTPUT_IN_STREAM"); } }

		public static string SamplesFolder { get { return EnvironmentVars.Get<string>("SAMPLES_FOLDER"); } }

		public static bool SavePreview { get { return EnvironmentVars.Get<bool>("SAVE_PREVIEW"); } }

		public static bool SelfCheckOutput { get { return EnvironmentVars.Get<bool>("SELF_CHECK_OUTPUT"); } }

		static TestVariables()
		{
			EnvironmentVars.SetIfNull("SAMPLES_FOLDER", "../../../../../samples/");
			EnvironmentVars.SetIfNull("OUTPUT_SAMPLES_FOLDER", "../../../../../samples/out");
			EnvironmentVars.SetIfNull("OUTPUT_SINGLE_CASES_FOLDER", "../../../../../samples/out/single_cases");
			EnvironmentVars.SetIfNull("OUTPUT_SVG", "../../../../../samples/out/svg");
			EnvironmentVars.SetIfNull("LOCAL_ENV", "true");
			EnvironmentVars.SetIfNull("DELTA", "0.00001");
			EnvironmentVars.SetIfNull("DECIMAL_PRECISION", "5");
			EnvironmentVars.SetIfNull("SAVE_OUTPUT_IN_STREAM", "false");
			EnvironmentVars.SetIfNull("SAVE_PREVIEW", "true");
			EnvironmentVars.SetIfNull("SELF_CHECK_OUTPUT", "true");
		}

		public static void CreateOutputFolders()
		{
			string outputSamplesFolder = OutputSamplesFolder;
			string outputSingleCasesFolder = OutputSingleCasesFolder;
			string outputSvgFolder = OutputSvgFolder;

#if NETFRAMEWORK
			string curr = AppDomain.CurrentDomain.BaseDirectory;
			outputSamplesFolder = Path.GetFullPath(Path.Combine(curr, OutputSamplesFolder));
			outputSingleCasesFolder = Path.GetFullPath(Path.Combine(curr, OutputSingleCasesFolder));
			outputSvgFolder = Path.GetFullPath(Path.Combine(curr, OutputSvgFolder));
#endif

			craateFolderIfDoesNotExist(outputSamplesFolder);
			craateFolderIfDoesNotExist(outputSingleCasesFolder);
			craateFolderIfDoesNotExist(outputSvgFolder);
		}

		private static void craateFolderIfDoesNotExist(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}
	}
}