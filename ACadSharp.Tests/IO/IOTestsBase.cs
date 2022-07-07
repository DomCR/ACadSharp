using ACadSharp.IO;
using ACadSharp.Tests.Common;
using System;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public abstract class IOTestsBase
	{
		protected const string _samplesFolder = "../../../../samples/";

		protected const string _samplesOutFolder = "../../../../samples/out";

		public static TheoryData<string> DwgFilePaths { get; }

		public static TheoryData<string> DxfAsciiFiles { get; }

		public static TheoryData<string> DxfBinaryFiles { get; }

		protected readonly ITestOutputHelper _output;

		protected readonly DocumentIntegrity _docIntegrity;

		static IOTestsBase()
		{
			DwgFilePaths = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(_samplesFolder, $"*.dwg"))
			{
				DwgFilePaths.Add(file);
			}

			DxfAsciiFiles = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(_samplesFolder, "*_ascii.dxf"))
			{
				DxfAsciiFiles.Add(file);
			}

			DxfBinaryFiles = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(_samplesFolder, "*_binary.dxf"))
			{
				DxfBinaryFiles.Add(file);
			}

			//Create folder, necessary in workflow
			if (!Directory.Exists(_samplesOutFolder))
			{
				Directory.CreateDirectory(_samplesOutFolder);
			}
		}

		public IOTestsBase(ITestOutputHelper output)
		{
			this._output = output;
			this._docIntegrity = new DocumentIntegrity(output);
		}

		protected void onNotification(object sender, NotificationEventArgs e)
		{
			_output.WriteLine(e.Message);
		}

		protected void checkDocumentInAutocad(string path)
		{
			if (Environment.GetEnvironmentVariable("GITHUB_WORKFLOW") != null || 
				!File.Exists("\"D:\\Programs\\Autodesk\\AutoCAD 2023\\accoreconsole.exe\""))
				return;

			System.Diagnostics.Process process = new System.Diagnostics.Process();

			try
			{
				process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
				process.StartInfo.FileName = "\"D:\\Programs\\Autodesk\\AutoCAD 2023\\accoreconsole.exe\"";
				process.StartInfo.Arguments = $"/i \"{ Path.Combine(_samplesFolder, "sample_base/empty.dwg")}\" /l en - US";
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardInput = true;
				process.StartInfo.StandardOutputEncoding = Encoding.ASCII;

				Assert.True(process.Start());

				process.StandardInput.WriteLine($"_DXFIN");
				process.StandardInput.WriteLine($"{path}");

				string l = process.StandardOutput.ReadLine();
				bool testPassed = true;
				while (!process.StandardOutput.EndOfStream)
				{
					string li = l.Replace("\0", "");
					if (!string.IsNullOrEmpty(li))
					{
						if (li.Contains("Invalid or incomplete DXF input -- drawing discarded."))
						{
							testPassed = false;
						}

						_output.WriteLine(li);
					}

					var t = process.StandardOutput.ReadLineAsync();

					//The last line gets into an infinite loop
					if (t.Wait(1000))
					{
						l = t.Result;
					}
					else
					{
						break;
					}
				}

				if (!testPassed)
					throw new Exception("File loading with accoreconsole failed");
			}
			finally
			{
				process.Kill();
			}
		}
	}
}
