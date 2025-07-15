using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class DimensionAlignedTests : CommonDimensionTests<DimensionAligned>
	{
		public override DimensionType Type => DimensionType.Aligned;

		[Fact]
		public void DefinitionPointRecalculation()
		{
			DimensionAligned aligned = new DimensionAligned
			{
				FirstPoint = new CSMath.XYZ(1, 0, 0),
				SecondPoint = new CSMath.XYZ(5, 0, 0),
			};

			aligned.Offset = 5;

			Assert.True((aligned.DefinitionPoint - aligned.SecondPoint).IsPerpendicular(aligned.SecondPoint));
			AssertUtils.AreEqual(new XYZ(5, 5, 0), aligned.DefinitionPoint);
		}

		protected override DimensionAligned createDim()
		{
			DimensionAligned dim = new DimensionAligned(XYZ.Zero, new XYZ(10, 0, 0));

			return dim;
		}
	}
}