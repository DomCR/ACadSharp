using ACadSharp.Entities;

namespace ACadSharp.Tests.Entities
{
	public class DimensionAngular3PtTests : CommonDimensionTests<DimensionAngular3Pt>
	{
		public override DimensionType Type => DimensionType.Angular3Point;

		public override void CloneTest()
		{
			throw new System.NotImplementedException();
		}

		public override void GetBoundingBoxTest()
		{
			throw new System.NotImplementedException();
		}

		public override void UpdateBlockTests()
		{
		}
	}
}
