using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class DimensionAlignedTests
	{
		[Fact]
		public void DefinitionPointRecalculation()
		{
			DimensionAligned aligned = new DimensionAligned
			{
				FirstPoint = new CSMath.XYZ(1, 0, 0),
				SecondPoint = new CSMath.XYZ(5, 0, 0),
			};

			aligned.Offset = 5;

			Assert.True((aligned.DefinitionPoint - aligned.FirstPoint).IsPerpendicular(aligned.FirstPoint));
			AssertUtils.AreEqual(new XYZ(1, 5, 0), aligned.DefinitionPoint);
		}
	}

	public class DimensionTests
	{
		[Fact]
		public void DimensionTypeTest()
		{
			DimensionAligned aligned = new DimensionAligned();

			Assert.True(aligned.Flags.HasFlag(DimensionType.Aligned));
			Assert.True(aligned.Flags.HasFlag(DimensionType.BlockReference));
		}

		[Fact]
		public void IsTextUserDefinedLocationTest()
		{
			DimensionAligned aligned = new DimensionAligned();

			Assert.False(aligned.Flags.HasFlag(DimensionType.TextUserDefinedLocation));

			aligned.IsTextUserDefinedLocation = true;

			Assert.True(aligned.Flags.HasFlag(DimensionType.TextUserDefinedLocation));
		}
	}
}
