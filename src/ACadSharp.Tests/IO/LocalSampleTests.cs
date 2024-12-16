using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tables;
using ACadSharp.Tests.TestModels;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class LocalSampleTests : IOTestsBase
	{
		public static TheoryData<FileModel> UserDwgFiles { get; } = new();

		public static TheoryData<FileModel> UserDxfFiles { get; } = new();

		public static TheoryData<FileModel> StressFiles { get; } = new();

		static LocalSampleTests()
		{
			loadLocalSamples("user_files", "dwg", UserDwgFiles);
			loadLocalSamples("user_files", "dxf", UserDxfFiles);
			loadLocalSamples("stress", "*", StressFiles);
		}

		public LocalSampleTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(UserDwgFiles))]
		public void ReadUserDwg(FileModel test)
		{
			if (string.IsNullOrEmpty(test.Path))
				return;

			CadDocument doc = DwgReader.Read(test.Path, this._dwgConfiguration, this.onNotification);

			if (test.Path.Contains("out"))
			{
				return;
			}

			CadDocument a = new CadDocument();

			var b = doc.GetCadObject(0x94);
			var b1 = doc.GetCadObject(14);
			var b2 = doc.GetCadObject(25);

			var vport = doc.VPorts[VPort.DefaultName];
			//vport.ExtendedData.Clear();

			//doc.VPorts.Remove(VPort.DefaultName);
			//doc.VPorts.Add(VPort.Default);

			//doc.RootDictionary.Remove("AcDbVariableDictionary", out _);

			//doc.Header = new ACadSharp.Header.CadHeader();
			//doc.Header.Version = ACadVersion.AC1032;

			doc.RootDictionary.Remove("AcadPlotStyleName", out _);

			string path = Path.ChangeExtension(test.Path, ".out.dwg");
			DwgWriter.Write(path, doc, notification: this.onNotification);
		}

		[Theory]
		[MemberData(nameof(UserDxfFiles))]
		public void ReadUserDxf(FileModel test)
		{
			if (string.IsNullOrEmpty(test.Path))
				return;

			CadDocument doc = DxfReader.Read(test.Path, this.onNotification);
		}

		[Theory]
		[MemberData(nameof(StressFiles))]
		public void ReadStressFiles(FileModel test)
		{
			if (string.IsNullOrEmpty(test.Path))
				return;

			CadDocument doc = null;
			string extension = Path.GetExtension(test.Path);

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			if (extension == ".dxf")
			{
				doc = DxfReader.Read(test.Path, this.onNotification);
			}
			else if (extension.Equals(".dwg", System.StringComparison.OrdinalIgnoreCase))
			{
				doc = DwgReader.Read(test.Path, this.onNotification);
			}

			stopwatch.Stop();
			this._output.WriteLine(stopwatch.Elapsed.TotalSeconds.ToString());

			//Files tested have a size of ~100MB
			//Cannot exceed 10 seconds
			Assert.True(stopwatch.Elapsed.TotalSeconds < 10);
		}
	}
}
