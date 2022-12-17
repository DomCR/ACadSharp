using ACadSharp.IO;
using ACadSharp.IO.DWG;
using Microsoft.VisualStudio.TestPlatform.Utilities.Helpers;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharpInternal.Tests
{
	public class DwgFileHeaderExploration
	{
		protected const string _samplesFolder = "../../../../samples/";

		public static TheoryData<string> DwgFilePaths { get; }

		private ITestOutputHelper _output;

		static DwgFileHeaderExploration()
		{
			DwgFilePaths = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(_samplesFolder, $"*.dwg"))
			{
				DwgFilePaths.Add(file);
			}
		}

		public DwgFileHeaderExploration(ITestOutputHelper output) : base()
		{
			this._output = output;
		}

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		public void PrintFileHeaderInfo(string test)
		{
			DwgFileHeader fh;
			using (DwgReader reader = new DwgReader(test))
			{
				fh = reader.readFileHeader();
			}

			printHeader(fh);
		}

		private void printHeader(DwgFileHeader fh)
		{
			printVar(nameof(fh.AcadVersion), fh.AcadVersion);
			printVar(nameof(fh.AcadMaintenanceVersion), fh.AcadMaintenanceVersion);
			printVar(nameof(fh.AcadMaintenanceVersion), fh.AcadMaintenanceVersion);

			if (fh is DwgFileHeader15 fh15)
			{
				printVar(nameof(fh15.DrawingCodePage), fh15.DrawingCodePage);
				this._output.WriteLine("--- Records:");
				foreach (DwgSectionLocatorRecord record in fh15.Records.Values)
				{
					printVar(nameof(record.Number), record.Number);
					printVar(nameof(record.Seeker), record.Seeker);
					printVar(nameof(record.Size), record.Size);
				}
				this._output.WriteLine("--- end records");
			}
		}

		private void printVar(string name, object value)
		{
			this._output.WriteLine($"{name}:{value}");
		}
	}
}
