using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestFramework("ACadSharp.Tests.TestSetup", "ACadSharp.Tests")]

namespace ACadSharp.Tests
{
	public sealed class TestSetup : XunitTestFramework
	{
		public TestSetup(IMessageSink messageSink)
		  : base(messageSink)
		{
			this.init();
		}

		private void init()
		{
			TestVariables.CreateOutputFolders();
		}
	}
}