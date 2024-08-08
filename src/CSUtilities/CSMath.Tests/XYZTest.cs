using Xunit;
using Xunit.Abstractions;

namespace CSMath.Tests
{
	public class XYZTest : VectorTests<XYZ>
	{
		public XYZTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public void CrossTest()
		{
			var v = new XYZ(1, 2, 3);
			var u = new XYZ(1, 5, 7);
			var result = XYZ.Cross(v, u);

			Assert.Equal(-1, result.X);
			Assert.Equal(-4, result.Y);
			Assert.Equal(3, result.Z);
		}
	}
}
