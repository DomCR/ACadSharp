using CSMath.Geometry;
using Xunit;

namespace CSMath.Tests.Geometry
{
	public class Line2DTests
	{
		[Fact]
		public void CreateLineTest()
		{
			//Line2D line = LineExtensions.CreateFromPoints<Line2D, XY>(new XY(), new XY(1, 1));
		}

		[Fact]
		public void IsPointOnLineTest()
		{
			Line2D line = new Line2D();

			line.IsPointOnLine(new XY());
		}
	}
}
