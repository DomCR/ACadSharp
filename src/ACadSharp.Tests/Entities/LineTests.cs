using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class LineTests : CommonEntityTests<Line>
	{
		private CSMathRandom _random = new CSMathRandom();

		[Fact]
		public void TranslationTest()
		{
			var start = XYZ.Zero;
			var end = new XYZ(1, 1, 0);
			Line line = new Line
			{
				StartPoint = start,
				EndPoint = end,
			};

			XYZ move = new XYZ(5, 5, 0);
			Transform transform = Transform.CreateTranslation(move);
			line.ApplyTransform(transform);

			AssertUtils.AreEqual(start.Add(move), line.StartPoint);
			AssertUtils.AreEqual(end.Add(move), line.EndPoint);
			AssertUtils.AreEqual(XYZ.AxisZ, line.Normal);
		}

		[Fact]
		public void RotationTest()
		{
			var start = XYZ.Zero;
			var end = new XYZ(1, 1, 0);
			Line line = new Line
			{
				StartPoint = start,
				EndPoint = end,
			};

			Transform translation = Transform.CreateRotation(XYZ.AxisX, MathHelper.DegToRad(90));
			line.ApplyTransform(translation);

			AssertUtils.AreEqual(start, line.StartPoint);
			AssertUtils.AreEqual(new XYZ(1, 0, 1), line.EndPoint);
			AssertUtils.AreEqual(XYZ.AxisY, line.Normal);
		}

		[Fact]
		public void ScalingTest()
		{
			var start = new XYZ(-1, -1, 0);
			var end = new XYZ(1, 1, 0);
			Line line = new Line
			{
				StartPoint = start,
				EndPoint = end,
			};

			XYZ scale = new XYZ(2, 2, 1);
			Transform transform = Transform.CreateScaling(scale);
			line.ApplyTransform(transform);

			AssertUtils.AreEqual(start.Multiply(scale), line.StartPoint);
			AssertUtils.AreEqual(end.Multiply(scale), line.EndPoint);
			AssertUtils.AreEqual(XYZ.AxisZ, line.Normal);
		}

		[Fact]
		public void RandomTranslationTest()
		{
			XYZ start = this._random.Next<XYZ>();
			XYZ end = this._random.Next<XYZ>();
			Line line = new Line
			{
				StartPoint = start,
				EndPoint = end,
			};

			XYZ move = this._random.Next<XYZ>();
			Transform translation = Transform.CreateTranslation(move);
			line.ApplyTransform(translation);

			AssertUtils.AreEqual(start.Add(move), line.StartPoint);
			AssertUtils.AreEqual(end.Add(move), line.EndPoint);
		}


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
