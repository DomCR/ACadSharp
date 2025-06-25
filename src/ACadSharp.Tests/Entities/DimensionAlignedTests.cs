using ACadSharp.Entities;
using CSMath;

namespace ACadSharp.Tests.Entities
{
	public class DimensionAlignedTests : CommonDimensionTests<DimensionAligned>
	{
		public override DimensionType Type => DimensionType.Aligned;

		protected override DimensionAligned createDim()
		{
			DimensionAligned dim = new DimensionAligned(XYZ.Zero, new XYZ(10, 0, 0));

			return dim;
		}
	}
}
