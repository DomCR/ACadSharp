using Xunit;

namespace ACadSharp.Tests.Objects.Collections
{
	public class ScaleCollectionTests
	{
		[Fact(Skip = "test needed for root dictionary")]
		public void InitScaleCollection()
		{
			CadDocument doc = new CadDocument();

			Assert.NotNull(doc.Scales);
		}
	}
}
