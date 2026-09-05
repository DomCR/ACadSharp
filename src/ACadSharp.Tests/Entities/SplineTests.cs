using ACadSharp.Entities;
using ACadSharp.Extensions;
using ACadSharp.Tests.Common;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities;

public class SplineTests : CommonEntityTests<Spline>
{
	[Fact]
	public override void GetBoundingBoxTest()
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
	public void GetBoundingBoxOfAPeriodicSplineStaysOnTheCurve()
	{
		//A periodic spline is flagged closed as well, and the domain of its unclamped knot vector
		//starts at knots[degree] - not at knots[0], which lies outside it. Sampling below the
		//domain makes every basis function vanish, and the zero vector that comes back used to
		//drag the bounding box all the way to the origin.
		Spline spline = new Spline();
		spline.Degree = 3;

		foreach (XYZ pt in new[]
		{
			new XYZ(1000, 2000, 0),
			new XYZ(1010, 2000, 0),
			new XYZ(1015, 2009, 0),
			new XYZ(1010, 2017, 0),
			new XYZ(1000, 2017, 0),
			new XYZ(995, 2009, 0),
			new XYZ(1000, 2000, 0),
		})
		{
			spline.ControlPoints.Add(pt);
		}

		for (int i = 0; i < 11; i++)
		{
			spline.Knots.Add(i);
		}

		spline.IsClosed = true;
		spline.IsPeriodic = true;

		BoundingBox hull = BoundingBox.FromPoints(spline.ControlPoints);
		BoundingBox box = spline.GetBoundingBox();

		//A B-spline lies inside the convex hull of its control points, so the box cannot reach
		//further out than they do - and certainly not back to (0,0).
		Assert.True(box.Min.X >= hull.Min.X - 1, $"min X {box.Min.X} escaped the control hull {hull.Min.X}");
		Assert.True(box.Min.Y >= hull.Min.Y - 1, $"min Y {box.Min.Y} escaped the control hull {hull.Min.Y}");
		Assert.True(box.Max.X <= hull.Max.X + 1, $"max X {box.Max.X} escaped the control hull {hull.Max.X}");
		Assert.True(box.Max.Y <= hull.Max.Y + 1, $"max Y {box.Max.Y} escaped the control hull {hull.Max.Y}");
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

	[Fact]
	public void UpdateFromFitPoints_2FitPoints()
	{
		Spline spline = new Spline();
		spline.FitPoints.Add(new XYZ(0, -5, 0));
		spline.FitPoints.Add(new XYZ(5, 0, 0));

		spline.UpdateFromFitPoints();

		Assert.Equal(4, spline.ControlPoints.Count);
		AssertUtils.AreEqual(new XYZ(0, -5, 0), spline.ControlPoints[0]);
		AssertUtils.AreEqual(new XYZ(1.6666666667, -3.3333333333, 0), spline.ControlPoints[1]);
		AssertUtils.AreEqual(new XYZ(3.3333333333, -1.6666666667, 0), spline.ControlPoints[2]);
		AssertUtils.AreEqual(new XYZ(5, 0, 0), spline.ControlPoints[3]);
	}

	[Fact]
	public void UpdateFromFitPoints_3FitPoints()
	{
		Spline spline = new Spline();
		spline.FitPoints.Add(new XYZ(0, -5, 0));
		spline.FitPoints.Add(new XYZ(5, 0, 0));
		spline.FitPoints.Add(new XYZ(10, -5, 0));

		spline.UpdateFromFitPoints();

		Assert.Equal(5, spline.ControlPoints.Count);
		AssertUtils.AreEqual(new XYZ(0, -5, 0), spline.ControlPoints[0]);
		AssertUtils.AreEqual(new XYZ(1.6666666667, -2.5, 0), spline.ControlPoints[1]);
		AssertUtils.AreEqual(new XYZ(5, 2.5, 0), spline.ControlPoints[2]);
		AssertUtils.AreEqual(new XYZ(8.3333333333, -2.5, 0), spline.ControlPoints[3]);
		AssertUtils.AreEqual(new XYZ(10, -5, 0), spline.ControlPoints[4]);
	}

	[Fact]
	public void UpdateFromFitPoints_5FitPoints()
	{
		Spline spline = new Spline();
		spline.FitPoints.Add(new XYZ(0, -5, 0));
		spline.FitPoints.Add(new XYZ(5, 0, 0));
		spline.FitPoints.Add(new XYZ(10, -5, 0));
		spline.FitPoints.Add(new XYZ(15, 0, 0));
		spline.FitPoints.Add(new XYZ(20, -5, 0));

		spline.UpdateFromFitPoints();

		Assert.Equal(7, spline.ControlPoints.Count);
		AssertUtils.AreEqual(new XYZ(0, -5, 0), spline.ControlPoints[0]);
		AssertUtils.AreEqual(new XYZ(1.6667, -2.1428, 0), spline.ControlPoints[1].Round(4));
		AssertUtils.AreEqual(new XYZ(5, 3.5714, 0), spline.ControlPoints[2].Round(4));
		AssertUtils.AreEqual(new XYZ(10, -9.2857, 0), spline.ControlPoints[3].Round(4));
		AssertUtils.AreEqual(new XYZ(15, 3.5714, 0), spline.ControlPoints[4].Round(4));
		AssertUtils.AreEqual(new XYZ(18.3333, -2.1428, 0), spline.ControlPoints[5].Round(4));
		AssertUtils.AreEqual(new XYZ(20, -5, 0), spline.ControlPoints[6].Round(4));
	}

	public override void CloneTest()
	{
		Spline spline = new Spline();

		spline.ControlPoints.Add(new XYZ(0, 0, 0));
		spline.ControlPoints.Add(new XYZ(10, 10, 0));
		spline.ControlPoints.Add(new XYZ(20, 10, 0));
		spline.ControlPoints.Add(new XYZ(50, 30, 0));

		spline.FitPoints.Add(new XYZ(1, 0, 2));
		spline.FitPoints.Add(new XYZ(20, 0, 0));
		spline.FitPoints.Add(new XYZ(20, 30, 0));
		spline.FitPoints.Add(new XYZ(50, 50, 0));

		spline.Degree = 3;

		spline.Knots.Add(0);
		spline.Knots.Add(0);
		spline.Knots.Add(0);
		spline.Knots.Add(0);

		spline.Knots.Add(1);
		spline.Knots.Add(1);
		spline.Knots.Add(1);
		spline.Knots.Add(1);

		spline.Weights.Add(1);
		spline.Weights.Add(1);
		spline.Weights.Add(1);
		spline.Weights.Add(1);

		Spline clone = spline.CloneTyped();

		CadObjectTestUtils.AssertEntityClone(spline, clone);

		Assert.NotEmpty(clone.ControlPoints);
		Assert.NotEmpty(clone.Knots);
		Assert.NotEmpty(clone.FitPoints);
		Assert.NotEmpty(clone.Weights);

		Assert.Equal(spline.ControlPoints, clone.ControlPoints);
		Assert.Equal(spline.Knots, clone.Knots);
		Assert.Equal(spline.FitPoints, clone.FitPoints);
		Assert.Equal(spline.Weights, clone.Weights);
	}
}