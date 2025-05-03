using ACadSharp.Entities;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class DimensionDiameterTests : CommonDimensionTests<DimensionDiameter>
	{
		public override DimensionType Type => DimensionType.Diameter;

		[Fact]
		public void CalculateReferencePointsTest()
		{
			DimensionDiameter dim = new DimensionDiameter
			{
				AngleVertex = new XYZ(10, 10, 0),
			};

			Assert.Equal(new XYZ(0, 0, 0), dim.DefinitionPoint);
			Assert.Equal(new XYZ(5, 5, 0), dim.Center);

			dim.CalculateReferencePoints();

			Assert.Equal(new XYZ(5, 5, 0), dim.Center);

			dim = new DimensionDiameter
			{
				DefinitionPoint = new XYZ(0, 0, 0),
				AngleVertex = new XYZ(10, 0, 0),
			};

			dim.CalculateReferencePoints();

			Assert.Equal(new XYZ(5, 0, 0), dim.Center);
			Assert.Equal(new XYZ(0, 0, 0), dim.DefinitionPoint);
			Assert.Equal(new XYZ(10, 0, 0), dim.AngleVertex);
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
