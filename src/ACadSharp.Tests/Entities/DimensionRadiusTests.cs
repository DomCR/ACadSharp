using ACadSharp.Entities;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities;

public class DimensionRadiusTests : CommonDimensionTests<DimensionRadius>
{
	public override DimensionType Type => DimensionType.Radius;

	public override void GetBoundingBoxTest()
	{
		//Laid out far from the origin, the way a real drawing is. AngleVertex is a point on the
		//measured curve; treating it as a half-size used to return a box that straddled (0,0) and was
		//as wide as the drawing is far from it, which is enough to wreck a whole drawing's extents.
		DimensionRadius dim = new DimensionRadius
		{
			DefinitionPoint = new XYZ(1000, 2000, 0),
			AngleVertex = new XYZ(1010, 2000, 0),
			TextMiddlePoint = new XYZ(1005, 2005, 0),
		};

		BoundingBox box = dim.GetBoundingBox();

		Assert.Equal(new XYZ(1000, 2000, 0), box.Min);
		Assert.Equal(new XYZ(1010, 2005, 0), box.Max);
	}

	protected override DimensionRadius createDim()
	{
		var dim = new DimensionRadius()
		{
			AngleVertex = new CSMath.XYZ(5, 5, 0)
		};

		return dim;
	}
}
