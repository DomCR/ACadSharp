using ACadSharp.Entities;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class DimensionDiameterTests : CommonDimensionTests<DimensionDiameter>
	{
		public override DimensionType Type => DimensionType.Diameter;

		public override void CloneTest()
		{
			throw new System.NotImplementedException();
		}

		public override void GetBoundingBoxTest()
		{
			throw new System.NotImplementedException();
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
