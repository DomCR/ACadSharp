using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class EllipseTests : CommonEntityTests<Ellipse>
	{
		[Fact]
		public void GetBoundingBoxTest()
		{
			//Ellipse size: x = 1, y = 0.5
			Ellipse ellipse = new();
			ellipse.RadiusRatio = 0.5d;

			BoundingBox boundingBox = ellipse.GetBoundingBox();

			//The point may not be exactly at the min max values, the tolerance needs to decrease
			AssertUtils.AreEqual(new XYZ(-1, -0.5, 0), boundingBox.Min, 2);
			AssertUtils.AreEqual(new XYZ(1, 0.5, 0), boundingBox.Max, 2);
		}

		[Fact]
		public void GetEndVerticesTest()
		{
			Ellipse ellipse = new Ellipse();

			ellipse.GetEndVertices(out XYZ start, out XYZ end);

			AssertUtils.AreEqual(XYZ.AxisX, start);
			AssertUtils.AreEqual(XYZ.AxisX, end);

			ellipse.StartParameter = MathHelper.HalfPI;
			ellipse.EndParameter = MathHelper.PI;

			ellipse.GetEndVertices(out start, out end);
			AssertUtils.AreEqual(XYZ.AxisY, start.RoundZero());
			AssertUtils.AreEqual(-XYZ.AxisX, end.RoundZero());
		}
	}
}