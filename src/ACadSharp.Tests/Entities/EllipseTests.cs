using ACadSharp.Entities;
using CSMath;
using System;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class EllipseTests
	{
		[Fact]
		public void GetBoundingBoxTest()
		{
			//Ellipse size: x = 1, y = 0.5
			Ellipse ellipse = new();
			ellipse.RadiusRatio = 0.5d;

			BoundingBox boundingBox = ellipse.GetBoundingBox();

			Assert.Equal(new XYZ(-1, -0.5, 0), boundingBox.Min);
			Assert.Equal(new XYZ(1, 0.5, 0), boundingBox.Max);
		}
	}
}
