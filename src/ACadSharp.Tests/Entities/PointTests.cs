using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class PointTests : CommonEntityTests<Point>
	{
		private CSMathRandom _random = new CSMathRandom();

		[Fact]
		public void TranslateTest()
		{
			XYZ init = _random.Next<XYZ>();
			XYZ transform = _random.Next<XYZ>();
			XYZ result = init + transform;

			Point point = new Point(init);

			point.ApplyTranslation(transform);

			AssertUtils.AreEqual(result, point.Location, "Point Location");
			AssertUtils.AreEqual(XYZ.AxisZ, point.Normal);
		}


		[Fact]
		public void RotationTest()
		{
			XYZ init = new(5, 5, 0);

			Point point = new Point(init);

			Transform translation = Transform.CreateRotation(new XYZ(1, 0, 0), MathHelper.DegToRad(90));
			point.ApplyTransform(translation);

			//Rotation around origin
			AssertUtils.AreEqual(new XYZ(5, 0, 5), point.Location, "Point Location");
			AssertUtils.AreEqual(XYZ.AxisY, point.Normal);
		}
	}
}
