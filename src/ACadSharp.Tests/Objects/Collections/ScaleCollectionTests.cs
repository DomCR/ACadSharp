using Xunit;

namespace ACadSharp.Tests.Objects.Collections
{
	public class ScaleCollectionTests
	{
		[Fact]
		public void InitScaleCollection()
		{
			CadDocument doc = new CadDocument();

			Assert.NotNull(doc.Scales);
		}
	}
}
