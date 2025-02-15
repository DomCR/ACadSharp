using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class LineTests
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
			Transform translation = Transform.CreateTranslation(move);
			line.ApplyTransform(translation);

			AssertUtils.AreEqual(start.Add(move), line.StartPoint);
			AssertUtils.AreEqual(end.Add(move), line.EndPoint);
		}

		[Fact]
		public void EscalationTest()
		{
			var start = new XYZ(-1, -1, 0);
			var end = new XYZ(1, 1, 0);
			Line line = new Line
			{
				StartPoint = start,
				EndPoint = end,
			};

			XYZ scale = new XYZ(2, 2, 1);
			Transform translation = Transform.CreateEscalation(scale);
			line.ApplyTransform(translation);

			AssertUtils.AreEqual(start.Multiply(scale), line.StartPoint);
			AssertUtils.AreEqual(end.Multiply(scale), line.EndPoint);
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
