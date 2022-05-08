using ACadSharp.IO;
using ACadSharp.Tests.Common;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public abstract class IOTestsBase
	{
		private const string _samplesFolder = "../../../../samples/";

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
	}
}
