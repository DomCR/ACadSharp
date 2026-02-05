using ACadSharp.Entities;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class DimensionDiameterTests : CommonDimensionTests<DimensionDiameter>
	{
		public override DimensionType Type => DimensionType.Diameter;

		public override void GetBoundingBoxTest()
		{
		}

		protected override DimensionDiameter createDim()
		{
			return new DimensionDiameter
			{
				AngleVertex = new XYZ(10, 10, 0),
			};
		}
	}
}
