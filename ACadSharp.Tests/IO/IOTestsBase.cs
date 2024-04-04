using ACadSharp.IO;
using ACadSharp.Tests.Common;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public abstract class IOTestsBase
	{
		protected static string samplesFolder => TestVariables.SamplesFolder;

		protected static string samplesOutFolder => TestVariables.OutputSamplesFolder;

		protected static string singleCasesOutFolder => TestVariables.OutputSingleCasesFolder;

		public static TheoryData<string> DwgFilePaths { get; }

		public static TheoryData<string> DxfAsciiFiles { get; }

		public static TheoryData<string> DxfBinaryFiles { get; }

		public static TheoryData<ACadVersion> Versions { get; }

		protected readonly DwgReaderConfiguration _dwgConfiguration = new DwgReaderConfiguration
		{
			Failsafe = false
		};

		protected readonly DxfReaderConfiguration _dxfConfiguration = new DxfReaderConfiguration
		{
		};

		protected readonly ITestOutputHelper _output;

		protected readonly DocumentIntegrity _docIntegrity;

		static IOTestsBase()
		{
			DwgFilePaths = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(samplesFolder, $"*.dwg"))
			{
				DwgFilePaths.Add(file);
			}

			DxfAsciiFiles = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(samplesFolder, "*_ascii.dxf"))
			{
				DxfAsciiFiles.Add(file);
			}

			DxfBinaryFiles = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(samplesFolder, "*_binary.dxf"))
			{
				DxfBinaryFiles.Add(file);
			}

			Versions = new TheoryData<ACadVersion>
			{
				ACadVersion.AC1012,
				ACadVersion.AC1014,
				ACadVersion.AC1015,
				ACadVersion.AC1018,
				ACadVersion.AC1021,
				ACadVersion.AC1024,
				ACadVersion.AC1027,
				ACadVersion.AC1032
			};
		}

		public IOTestsBase(ITestOutputHelper output)
		{
			this._output = output;
			this._docIntegrity = new DocumentIntegrity(output);
		}

		protected void onNotification(object sender, NotificationEventArgs e)
		{
			if (e.NotificationType == NotificationType.Error)
			{
				throw e.Exception;
			}

			_output.WriteLine(e.Message);
			if (e.Exception != null)
			{
				_output.WriteLine(e.Exception.ToString());
			}
		}

		protected static void loadSamples(string folder, string ext, TheoryData<string> files)
		{
			string path = Path.Combine(samplesFolder, "local", folder);

			if (!Directory.Exists(path))
			{
				files.Add(string.Empty);
				return;
			}

			foreach (string file in Directory.GetFiles(path, $"*.{ext}"))
			{
				files.Add(file);
			}

			if (!files.Any())
			{
				files.Add(string.Empty);
			}
		}

		protected bool isSupportedVersion(ACadVersion version)
		{
			switch (version)
			{
				case ACadVersion.MC0_0:
				case ACadVersion.AC1_2:
				case ACadVersion.AC1_4:
				case ACadVersion.AC1_50:
				case ACadVersion.AC2_10:
				case ACadVersion.AC1002:
				case ACadVersion.AC1003:
				case ACadVersion.AC1004:
				case ACadVersion.AC1006:
				case ACadVersion.AC1009:
				case ACadVersion.AC1012:
					return false;
				case ACadVersion.AC1014:
				case ACadVersion.AC1015:
				case ACadVersion.AC1018:
					return true;
				case ACadVersion.AC1021:
					return false;
				case ACadVersion.AC1024:
					return true;
				case ACadVersion.AC1027:
				case ACadVersion.AC1032:
					return true;
				case ACadVersion.Unknown:
				default:
					return false;
			}
		}
	}
}
