using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class DimensionAngular2LineTests : CommonDimensionTests<DimensionAngular2Line>
	{
		public override DimensionType Type => DimensionType.Angular;

		[Fact]
		public void CenterTest()
		{
			var dim = this.createDim();

			AssertUtils.AreEqual(XYZ.Zero, dim.Center);
		}

		[Fact]
		public void MeasurementTest()
		{
			var dim = this.createDim();

			Assert.Equal(MathHelper.HalfPI, dim.Measurement);
		}

		public override void UpdateBlockTests()
		{
		}

		protected override DimensionAngular2Line createDim()
		{
			DimensionAngular2Line angular = new DimensionAngular2Line();
			angular.FirstPoint = XYZ.Zero;
			angular.SecondPoint = XYZ.AxisX;

			angular.DefinitionPoint = XYZ.Zero;
			angular.AngleVertex = XYZ.AxisY;

			return angular;
		}
	}
}