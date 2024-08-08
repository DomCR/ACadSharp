using Xunit;
using Xunit.Abstractions;

namespace CSMath.Tests
{
	public class XYTest : VectorTests<XY>
	{
		public XYTest(ITestOutputHelper output) : base(output)
		{
		}

		public override void ConvertTest()
		{
			XY xy = new XY(1, 1);
			XYZ xyz = xy.Convert<XYZ>();

			Assert.Equal(new XYZ(1, 1, 0), xyz);
		}
	}
}
