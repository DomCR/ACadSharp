using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using System;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class ArcTests : CommonEntityTests<Arc>
	{
		[Fact]
		public void CreateFromBulgeTest()
		{
			XY start = new XY(1, 0);
			XY end = new XY(0, 1);
			// 90 degree bulge
			double bulge = Math.Tan(Math.PI / (2 * 4));

			XY center = Arc.GetCenter(start, end, bulge, out double radius);

#if NETFRAMEWORK
			center = MathHelper.FixZero(center);
#endif

			Assert.Equal(XY.Zero, center);
			Assert.Equal(1, radius, TestVariables.DecimalPrecision);

			Arc arc = Arc.CreateFromBulge(start, end, bulge);

#if NETFRAMEWORK
			arc.Center = MathHelper.FixZero(arc.Center);
#endif

			Assert.Equal(XYZ.Zero, arc.Center);
			Assert.Equal(1, arc.Radius, TestVariables.DecimalPrecision);
			Assert.Equal(0, arc.StartAngle, TestVariables.DecimalPrecision);
			Assert.Equal(Math.PI / 2, arc.EndAngle, TestVariables.DecimalPrecision);
		}

		[Fact]
		public void GetBoundingBoxTest()
		{
			Arc arc = new Arc();
			arc.Radius = 5;
			arc.EndAngle = Math.PI / 2;

			BoundingBox boundingBox = arc.GetBoundingBox();

			arc.GetEndVertices(out XYZ s1, out XYZ e2);

			Assert.Equal(new XYZ(5, 0, 0), s1);
			Assert.Equal(new XYZ(0, 5, 0), e2);

			AssertUtils.Equals(new XYZ(0, 0, 0), boundingBox.Min);
			AssertUtils.Equals(new XYZ(5, 5, 0), boundingBox.Max);

			arc.Center = new XYZ(200.0, 200.0, 0.0);
			boundingBox = arc.GetBoundingBox();

			AssertUtils.Equals(new XYZ(200, 200, 0), boundingBox.Min);
			AssertUtils.Equals(new XYZ(205, 205, 0), boundingBox.Max);
		}

		[Fact]
		public void GetCenter()
		{
			XY start = new XY(1, 0);
			XY end = new XY(0, 1);
			// 90 degree bulge
			double bulge = Math.Tan(Math.PI / (2 * 4));

			XY center = Arc.GetCenter(start, end, bulge);

#if NETFRAMEWORK
			center = MathHelper.FixZero(center);
#endif

			Assert.Equal(XY.Zero, center);

			Arc arc = Arc.CreateFromBulge(start, end, bulge);

#if NETFRAMEWORK
			arc.Center = MathHelper.FixZero(arc.Center);
#endif

			Assert.Equal(XYZ.Zero, arc.Center);
			Assert.Equal(1, arc.Radius, TestVariables.DecimalPrecision);
			Assert.Equal(0, arc.StartAngle, TestVariables.DecimalPrecision);
			Assert.Equal(Math.PI / 2, arc.EndAngle, TestVariables.DecimalPrecision);
		}

		[Fact]
		public void GetEndVerticesTest()
		{
			var start = new XYZ(1, 0, 0);
			var end = new XYZ(0, 1, 0);
			// 90 degree bulge
			double bulge = Math.Tan(Math.PI / (2 * 4));

			Arc arc = Arc.CreateFromBulge(start.Convert<XY>(), end.Convert<XY>(), bulge);

			arc.GetEndVertices(out XYZ s1, out XYZ e2);

			AssertUtils.AreEqual<XYZ>(start, s1, "start point");
			AssertUtils.AreEqual<XYZ>(end, e2, "end point");

			arc = new Arc()
			{
				StartAngle = 0,
				EndAngle = Math.PI / 2,
				Normal = XYZ.AxisX
			};

			start = new XYZ(0, 1, 0);
			end = new XYZ(0, 0, 1);

			arc.GetEndVertices(out s1, out e2);

			AssertUtils.AreEqual<XYZ>(start, s1, "start point");
			AssertUtils.AreEqual<XYZ>(end, e2, "end point");

			arc = new Arc()
			{
				Center = new XYZ(100, 0, 0),
				Radius = 50,
				StartAngle = MathHelper.HalfPI,
				EndAngle = Math.PI,
			};

			start = new XYZ(100, 50, 0);
			end = new XYZ(50, 0, 0);

			arc.GetEndVertices(out s1, out e2);

			AssertUtils.AreEqual<XYZ>(start, s1, "start point");
			AssertUtils.AreEqual<XYZ>(end, e2, "end point");
		}

		[Fact]
		public void PolarCoordinateRelativeToCenterTest()
		{
			var mid = new XYZ(Math.Sqrt(2) / 2, Math.Sqrt(2) / 2, 0);
			Arc arc = new Arc()
			{
				StartAngle = 0,
				Radius = 1,
				EndAngle = Math.PI / 2,
			};

			var v = arc.PolarCoordinateRelativeToCenter(Math.PI / 4);

			AssertUtils.AreEqual<XYZ>(mid, v, "mid point");

			arc = new Arc()
			{
				StartAngle = 0,
				Radius = 1,
				Center = new XYZ(20, 20, 0),
				EndAngle = Math.PI / 2,
			};

			mid += arc.Center;

			v = arc.PolarCoordinateRelativeToCenter(Math.PI / 4);

			AssertUtils.AreEqual<XYZ>(mid, v, "mid point");
		}

		[Fact]
		public void PolygonalVertexesTest()
		{
			var start = new XYZ(1, 0, 0);
			var mid = new XYZ(Math.Sqrt(2) / 2, Math.Sqrt(2) / 2, 0);
			var end = new XYZ(0, 1, 0);
			Arc arc = new Arc()
			{
				StartAngle = 0,
				Radius = 1,
				EndAngle = Math.PI / 2,
			};

			var v = arc.PolygonalVertexes(3);

			AssertUtils.AreEqual<XYZ>(start, v[0], "start point");
			AssertUtils.AreEqual<XYZ>(mid, v[1], "mid point");
			AssertUtils.AreEqual<XYZ>(end, v[2], "end point");

			arc = new Arc()
			{
				StartAngle = 0,
				Radius = 1,
				Center = new XYZ(20, 20, 0),
				EndAngle = Math.PI / 2,
			};

			start += arc.Center;
			mid += arc.Center;
			end += arc.Center;

			v = arc.PolygonalVertexes(3);

			AssertUtils.AreEqual<XYZ>(start, v[0], "start point");
			AssertUtils.AreEqual<XYZ>(mid, v[1], "mid point");
			AssertUtils.AreEqual<XYZ>(end, v[2], "end point");

			arc = new Arc()
			{
				Center = new XYZ(100, 0, 0),
				Radius = 50,
				StartAngle = MathHelper.HalfPI,
				EndAngle = Math.PI,
			};

			start = new XYZ(100, 50, 0);
			end = new XYZ(50, 0, 0);

			v = arc.PolygonalVertexes(3);

			arc.GetEndVertices(out XYZ s, out XYZ e);

			AssertUtils.AreEqual<XYZ>(start, v[0], "start point");
			AssertUtils.AreEqual<XYZ>(end, v[2], "end point");
		}

		[Fact]
		public void RotationTest()
		{
			double radius = 5;
			XYZ center = new XYZ(1, 1, 0);
			Arc arc = new Arc
			{
				Radius = radius,
				Center = center
			};

			Transform transform = Transform.CreateRotation(XYZ.AxisX, MathHelper.DegToRad(90));
			arc.ApplyTransform(transform);

			AssertUtils.AreEqual(new XYZ(1, 0, 1), arc.Center);
			Assert.Equal(radius, arc.Radius);
			Assert.Equal(Math.PI, arc.StartAngle);
			Assert.Equal(0, arc.EndAngle);
			AssertUtils.AreEqual(XYZ.AxisY, arc.Normal);
		}

		[Fact]
		public void ScalingTest()
		{
			double radius = 5;
			XYZ center = new XYZ(1, 1, 0);
			Arc arc = new Arc
			{
				Radius = radius,
				Center = center
			};

			XYZ scale = new XYZ(2, 2, 1);
			Transform transform = Transform.CreateScaling(scale, center);
			arc.ApplyTransform(transform);

			AssertUtils.AreEqual(XYZ.AxisZ, arc.Normal);
			AssertUtils.AreEqual(center, arc.Center);
			Assert.Equal(10, arc.Radius);
			Assert.Equal(0, arc.StartAngle);
			Assert.Equal(Math.PI, arc.EndAngle);
		}

		[Fact]
		public void TranslationTest()
		{
			double radius = 5;
			XYZ center = new XYZ(1, 1, 0);
			Arc arc = new Arc
			{
				Radius = radius,
				Center = center,
			};

			XYZ move = new XYZ(5, 5, 0);
			Transform transform = Transform.CreateTranslation(move);
			arc.ApplyTransform(transform);

			AssertUtils.AreEqual(XYZ.AxisZ, arc.Normal);
			AssertUtils.AreEqual(center.Add(move), arc.Center);
			Assert.Equal(radius, arc.Radius);
			Assert.Equal(0, arc.StartAngle);
			Assert.Equal(Math.PI, arc.EndAngle);
		}
	}
}