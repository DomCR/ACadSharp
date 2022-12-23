using ACadSharp;
using ACadSharp.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharpInternal.Tests
{
	public abstract class DwgSectionWriterTestBase
	{
		public static TheoryData<ACadVersion> DwgVersions { get; }

		protected readonly ITestOutputHelper _output;

		static DwgSectionWriterTestBase()
		{
			DwgVersions = new TheoryData<ACadVersion>
			{
				ACadVersion.AC1012,
				ACadVersion.AC1014,
				ACadVersion.AC1015,
				ACadVersion.AC1018,
				ACadVersion.AC1021,
				ACadVersion.AC1024,
				ACadVersion.AC1027,
				ACadVersion.AC1032,
			};
		}

		public DwgSectionWriterTestBase(ITestOutputHelper output)
		{
			this._output = output;
		}

		protected void onNotification(object sender, NotificationEventArgs e)
		{
			_output.WriteLine(e.Message);
		}
	}
}
