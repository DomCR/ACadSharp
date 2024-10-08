using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class ViewportTests
	{
		[Fact]
		public void GetBoundingBoxTest()
		{
			Viewport viewport = new Viewport();
			viewport.Width = 100;
			viewport.Height = 50;
			viewport.Center = new XYZ(10, 10, 0);

			BoundingBox boundingBox = viewport.GetBoundingBox();

			AssertUtils.AreEqual(viewport.Center, boundingBox.Center);
			AssertUtils.AreEqual(new XYZ(-40, -15, 0), boundingBox.Min);
			AssertUtils.AreEqual(new XYZ(60, 35, 0), boundingBox.Max);
		}

		[Fact]
		public void GetModelBoundingBox()
		{
			Viewport viewport = new Viewport();
			viewport.Width = 100;
			viewport.Height = 50;
			viewport.ViewHeight = 50;
			viewport.ViewCenter = new XY(10, 10);

			BoundingBox boundingBox = viewport.GetModelBoundingBox();

			Assert.Equal(100, viewport.ViewWidth);
			AssertUtils.AreEqual(viewport.ViewCenter, ((XY)boundingBox.Center));
			AssertUtils.AreEqual(new XYZ(-40, -15, 0), boundingBox.Min);
			AssertUtils.AreEqual(new XYZ(60, 35, 0), boundingBox.Max);
		}
	}
}
