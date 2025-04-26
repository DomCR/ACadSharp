using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using System;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class CircleTests : CommonEntityTests<Circle>
	{
		private CSMathRandom _random = new CSMathRandom();

		[Fact]
		public void GetBoundingBoxTest()
		{
			Circle circle = new Circle();
			circle.Radius = 5;

			BoundingBox boundingBox = circle.GetBoundingBox();

			Assert.Equal(new XYZ(-5, -5, 0), boundingBox.Min);
			Assert.Equal(new XYZ(5, 5, 0), boundingBox.Max);
		}

		[Fact]
		public void PolygonalVertexesTest()
		{
			var start = new XYZ(1, 0, 0);
			var mid = new XYZ(Math.Sqrt(2) / 2, Math.Sqrt(2) / 2, 0);
			var end = new XYZ(0, 1, 0);
			Circle circle = new Circle()
			{
				Radius = 1,
			};

			var v = circle.PolygonalVertexes(9);

			AssertUtils.AreEqual<XYZ>(start, v[0], "start point");
			AssertUtils.AreEqual<XYZ>(mid, v[1], "mid point");
			AssertUtils.AreEqual<XYZ>(end, v[2], "end point");

			circle = new Circle()
			{
				Radius = 1,
				Center = new XYZ(20, 20, 0),
			};		


			start += circle.Center;
			mid += circle.Center;
			end += circle.Center;

			v = circle.PolygonalVertexes(9);

			AssertUtils.AreEqual<XYZ>(start, v[0], "start point");
			AssertUtils.AreEqual<XYZ>(mid, v[1], "mid point");
			AssertUtils.AreEqual<XYZ>(end, v[2], "end point");
		}

		[Fact]
		public void RotationTest()
		{
			double radius = 5;
			XYZ center = new XYZ(1, 1, 0);
			Circle circle = new Circle
			{
				Radius = radius,
				Center = center
			};

			Transform transform = Transform.CreateRotation(XYZ.AxisX, MathHelper.DegToRad(90));
			circle.ApplyTransform(transform);

			AssertUtils.AreEqual(new XYZ(1, 0, 1), circle.Center);
			Assert.Equal(radius, circle.Radius);
			AssertUtils.AreEqual(XYZ.AxisY, circle.Normal);
		}

		[Fact]
		public void ScalingTest()
		{
			double radius = 5;
			XYZ center = new XYZ(1, 1, 0);
			Circle circle = new Circle
			{
				Radius = radius,
				Center = center
			};

			XYZ scale = new XYZ(2, 2, 1);
			Transform transform = Transform.CreateScaling(scale, center);
			circle.ApplyTransform(transform);

			AssertUtils.AreEqual(XYZ.AxisZ, circle.Normal);
			AssertUtils.AreEqual(center, circle.Center);
			Assert.Equal(10, circle.Radius);
		}

		[Fact]
		public void TranslationTest()
		{
			double radius = 5;
			XYZ center = new XYZ(1, 1, 0);
			Circle circle = new Circle
			{
				Radius = radius,
				Center = center
			};

			XYZ move = new XYZ(5, 5, 0);
			Transform transform = Transform.CreateTranslation(move);
			circle.ApplyTransform(transform);

			AssertUtils.AreEqual(XYZ.AxisZ, circle.Normal);
			AssertUtils.AreEqual(center.Add(move), circle.Center);
			Assert.Equal(radius, circle.Radius);
		}
	}
}