using ACadSharp.Entities;

namespace ACadSharp.Tests.Entities
{
	public class DimensionAlignedTests : CommonDimensionTests<DimensionAligned>
	{
		public override DimensionType Type => DimensionType.Aligned;
	}
}
