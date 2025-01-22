using ACadSharp.Entities;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class LineTests
	{
		[Fact]
		public void GetBoundingBoxTest()
		{
			Line line = new Line();
			line.EndPoint = new XYZ(10, 10, 0);

			BoundingBox boundingBox = line.GetBoundingBox();

			Assert.Equal(new XYZ(0, 0, 0), boundingBox.Min);
			Assert.Equal(new XYZ(10, 10, 0), boundingBox.Max);
		}
	}
}
