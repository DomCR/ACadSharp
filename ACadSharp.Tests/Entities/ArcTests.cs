using ACadSharp.Entities;
using CSMath;
using System;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class ArcTests
	{
		[Fact]
		public void CreateFromBulgeTest()
		{
			XY start = new XY(1, 0);
			XY end = new XY(0, 1);
			// 90 degree bulge
			double bulge = Math.Tan(Math.PI / (2 * 4));

			XY center = MathUtils.GetCenter(start, end, bulge, out double radius);

			Assert.Equal(XY.Zero, center);
			Assert.Equal(1, radius);

			Arc arc = Arc.CreateFromBulge(start, end, bulge);

			Assert.Equal(XYZ.Zero, arc.Center);
			Assert.Equal(1, arc.Radius);
			Assert.Equal(0, arc.StartAngle);
			Assert.Equal(Math.PI / 2, arc.EndAngle);
		}

		[Fact(Skip = "Needs a dedicated implementation for Arc")]
		public void GetBoundingBoxTest()
		{
			Arc arc = new Arc();
			arc.Radius = 5;
			arc.EndAngle = Math.PI / 2;

			BoundingBox boundingBox = arc.GetBoundingBox();

			Assert.Equal(new XYZ(0, 0, 0), boundingBox.Min);
			Assert.Equal(new XYZ(5, 5, 0), boundingBox.Max);
		}
	}
}
