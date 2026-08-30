using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities;

public class EllipseTests : CommonEntityTests<Ellipse>
{
	[Fact]
	public override void GetBoundingBoxTest()
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
	public void MirroredEllipseCentreIsAlreadyInWorldCoordinates()
	{
		//An ellipse's centre and major axis are WCS in both DWG and DXF - unlike a circle's,
		//which is OCS. A (0,0,-1) normal must NOT relocate it. This test exists to stop anyone
		//"fixing" Ellipse the way Circle had to be fixed.
		Ellipse ellipse = new Ellipse
		{
			Center = new XYZ(10, 0, 0),
			MajorAxisEndPoint = new XYZ(5, 0, 0),
			RadiusRatio = 0.5,
			Normal = new XYZ(0, 0, -1),
		};

		BoundingBox box = ellipse.GetBoundingBox();

		Assert.Equal(5.0, box.Min.X, 6);
		Assert.Equal(15.0, box.Max.X, 6);
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