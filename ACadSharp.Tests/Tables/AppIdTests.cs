using ACadSharp.Tables;
using Xunit;

namespace ACadSharp.Tests.Tables
{
	public class AppIdTests
	{
		[Fact()]
		public void CloneTest()
		{
			AppId app = new AppId("clone_test");

			AppId clone = (AppId)app.Clone();

			Assert.NotEqual(app, clone);
		}
	}
}
