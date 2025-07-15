using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class DimensionDimensionLinearTests : CommonDimensionTests<DimensionLinear>
	{
		public override DimensionType Type => DimensionType.Linear;

		[Fact]
		public void DefinitionPointRecalculation()
		{
			DimensionLinear aligned = new DimensionLinear
			{
				FirstPoint = XYZ.Zero,
				SecondPoint = new XYZ(10, 10, 0),
			};

			aligned.Offset = 5;

			AssertUtils.AreEqual(new XYZ(10, 15, 0), aligned.DefinitionPoint);

			aligned = new DimensionLinear
			{
				FirstPoint = XYZ.Zero,
				SecondPoint = new XYZ(10, 10, 0),
				Rotation = MathHelper.DegToRad(45),
			};

			aligned.Offset = 5;

			AssertUtils.AreEqual(new XYZ(6.464466, 13.535533, 0), aligned.DefinitionPoint);
		}

		protected override DimensionLinear createDim()
		{
			DimensionLinear dim = new DimensionLinear
			{
				FirstPoint = XYZ.Zero,
				SecondPoint = new XYZ(10, 10, 0),
				DefinitionPoint = new XYZ(10, 15, 0),
			};

			return dim;
		}
	}
}