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

		public override void GetBoundingBoxTest()
		{
			var dim = createDim();
			BoundingBox b = dim.GetBoundingBox();

			Assert.Equal(new BoundingBox(XYZ.Zero, XYZ.AxisX), b);
		}

		[Fact]
		public void MeasurementTest()
		{
			var dim = this.createDim();

			Assert.Equal(MathHelper.HalfPI, dim.Measurement);
		}

		[Fact]
		public void MeasurementTest_DimensionArc_BetweenVectors()
		{
			var dim = this.createDim();
			dim.DefinitionPoint = XYZ.AxisX; // 0°
			dim.SecondPoint = new XYZ(1, 1, 0).Normalize(); // 45°
			dim.DimensionArc = new XYZ(1, 0.5f, 0); // somewhere between

			Assert.Equal(MathHelper.HalfPI * 0.5f, dim.Measurement);
		}

		[Fact]
		public void MeasurementTest_DimensionArc_After2ndVector()
		{
			var dim = this.createDim();
			dim.DefinitionPoint = XYZ.AxisX; // 0°
			dim.SecondPoint = new XYZ(1, 1, 0).Normalize(); // 45°
			dim.DimensionArc = new XYZ(0, 1, 0).Normalize(); //After 2nd vector

			Assert.Equal(MathHelper.HalfPI * 1.5f, dim.Measurement);
		}

		[Fact]
		public void MeasurementTest_DimensionArc_BetweenMirroredVector()
		{
			var dim = this.createDim();
			dim.DefinitionPoint = XYZ.AxisX; // 0°
			dim.SecondPoint = new XYZ(1, 1, 0).Normalize(); // 45°
			dim.DimensionArc = new XYZ(-1, -0.5f, 0); // somewhere between

			Assert.Equal(MathHelper.HalfPI * 0.5f, dim.Measurement);
		}

		[Fact]
		public void MeasurementTest_DimensionArc_BeforeFirstVector()
		{
			var dim = this.createDim();
			dim.DefinitionPoint = XYZ.AxisX; // 0°
			dim.SecondPoint = new XYZ(1, 1, 0).Normalize(); // 45°
			dim.DimensionArc = new XYZ(0, -1, 0).Normalize(); //After 2nd vector

			Assert.Equal(MathHelper.HalfPI * 1.5f, dim.Measurement);
		}

		public override void UpdateBlockTests()
		{
		}

		protected override DimensionAngular2Line createDim()
		{
			DimensionAngular2Line angular = new DimensionAngular2Line();
			angular.FirstPoint = XYZ.Zero;
			angular.SecondPoint = XYZ.AxisX;

			angular.AngleVertex = XYZ.Zero;
			angular.DefinitionPoint = XYZ.AxisY;

			return angular;
		}
	}
}