using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class PointTests : CommonEntityTests<Point>
	{
		private CSMathRandom _random = new CSMathRandom();

		[Fact]
		public void TranslateTest()
		{
			XYZ init = _random.Next<XYZ>();
			XYZ translation = _random.Next<XYZ>();
			XYZ result = init + translation;

			Point point = new Point(init);

			point.Translate(translation);

			AssertUtils.AreEqual(result, point.Location, "Point Location");
		}
	}
}
