using ACadSharp.Entities;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class CircleTests
	{
		[Fact]
		public void GetBoundingBoxTest()
		{
			Circle circle = new Circle();
			circle.Radius = 5;

			BoundingBox boundingBox = circle.GetBoundingBox();

			Assert.Equal(new XYZ(-5, -5, 0), boundingBox.Min);
			Assert.Equal(new XYZ(5, 5, 0), boundingBox.Max);
		}
	}
}
