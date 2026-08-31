using ACadSharp.Entities;

namespace ACadSharp.Tests.Entities;

public class DimensionArcTests : CommonDimensionTests<DimensionArc>
{
	public override DimensionType Type => DimensionType.Angular3Point;

	public override void GetBoundingBoxTest()
	{
	}

	public override void UpdateBlockTests()
	{
	}
}
