namespace ACadSharp.Tests.Entities;

using ACadSharp.Entities;
using CSMath;
using System.Linq;
using Xunit;

/// <summary>
/// The control points of a hatch's spline boundary keep the WEIGHT in Z - the class says so on the
/// property itself. Measuring a bounding box from them as if they were points gave the hatch a
/// height made of weights, and every block and INSERT above it inherited that height.
/// </summary>
public class HatchSplineBoundingBoxTests
{
	[Fact]
	public void AWeightIsNotAHeight()
	{
		Hatch.BoundaryPath.Spline edge = this.edge(weight: 1.0);

		BoundingBox box = edge.GetBoundingBox();

		Assert.Equal(0, box.Min.Z);
		Assert.Equal(0, box.Max.Z);
	}

	[Theory]
	[InlineData(1.0)]
	[InlineData(0.5)]
	[InlineData(7.25)]
	public void TheBoxIsTheSameWhateverTheWeightsAre(double weight)
	{
		//The point of the fix: two hatches over the same outline occupy the same space, however
		//their control points are weighted.
		BoundingBox reference = this.edge(weight: 1.0).GetBoundingBox();

		BoundingBox box = this.edge(weight).GetBoundingBox();

		Assert.Equal(reference.Min, box.Min);
		Assert.Equal(reference.Max, box.Max);
	}

	[Fact]
	public void TheOutlineItselfStillMeasures()
	{
		Hatch.BoundaryPath.Spline edge = this.edge(weight: 1.0);

		BoundingBox box = edge.GetBoundingBox();

		Assert.Equal(0, box.Min.X);
		Assert.Equal(0, box.Min.Y);
		Assert.Equal(10, box.Max.X);
		Assert.Equal(4, box.Max.Y);
	}

	private Hatch.BoundaryPath.Spline edge(double weight)
	{
		Hatch.BoundaryPath.Spline edge = new();
		edge.Degree = 3;
		edge.ControlPoints.Add(new XYZ(0, 0, weight));
		edge.ControlPoints.Add(new XYZ(3, 4, weight));
		edge.ControlPoints.Add(new XYZ(7, 1, weight));
		edge.ControlPoints.Add(new XYZ(10, 2, weight));
		return edge;
	}
}
