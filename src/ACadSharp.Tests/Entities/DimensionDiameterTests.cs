using ACadSharp.Entities;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities;

public class DimensionDiameterTests : CommonDimensionTests<DimensionDiameter>
{
	public override DimensionType Type => DimensionType.Diameter;

	public override void GetBoundingBoxTest()
	{
		//Laid out far from the origin, the way a real drawing is. AngleVertex is a point on the
		//measured curve; treating it as a half-size used to return a box that straddled (0,0) and was
		//as wide as the drawing is far from it, which is enough to wreck a whole drawing's extents.
		DimensionDiameter dim = new DimensionDiameter
		{
			DefinitionPoint = new XYZ(1000, 2000, 0),
			AngleVertex = new XYZ(1010, 2000, 0),
			TextMiddlePoint = new XYZ(1005, 2005, 0),
		};

		BoundingBox box = dim.GetBoundingBox();

		Assert.Equal(new XYZ(1000, 2000, 0), box.Min);
		Assert.Equal(new XYZ(1010, 2005, 0), box.Max);
	}

	protected override DimensionDiameter createDim()
	{
		return new DimensionDiameter
		{
			AngleVertex = new XYZ(10, 10, 0),
		};
	}
}
