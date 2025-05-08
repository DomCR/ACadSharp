using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class SplineTests : CommonEntityTests<Spline>
	{
		[Fact]
		public override void BoundingBoxTest()
		{
			Spline spline = new Spline();

			spline.ControlPoints.Add(new XYZ(0, 0, 0));
			spline.ControlPoints.Add(new XYZ(10, 10, 0));
			spline.ControlPoints.Add(new XYZ(20, 10, 0));
			spline.ControlPoints.Add(new XYZ(50, 30, 0));

			spline.Degree = 3;

			spline.Knots.Add(0);
			spline.Knots.Add(0);
			spline.Knots.Add(0);
			spline.Knots.Add(0);

			spline.Knots.Add(1);
			spline.Knots.Add(1);
			spline.Knots.Add(1);
			spline.Knots.Add(1);

			var box = spline.GetBoundingBox();

			AssertUtils.AreEqual(new XYZ(0, 0, 0), box.Min);
			AssertUtils.AreEqual(new XYZ(50, 30, 0), box.Max);
		}

		[Fact]
		public void CheckIsCloseFlag()
		{
			Spline spline = new Spline();

			spline.IsClosed = true;

			Assert.True(spline.Flags.HasFlag(SplineFlags.Closed));
			Assert.True(spline.Flags1.HasFlag(SplineFlags1.Closed));
			Assert.True(spline.IsClosed);

			spline.IsClosed = false;

			Assert.False(spline.Flags.HasFlag(SplineFlags.Closed));
			Assert.False(spline.Flags1.HasFlag(SplineFlags1.Closed));
			Assert.False(spline.IsClosed);
		}
	}
}