using ACadSharp.Entities;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class SplineTests : CommonEntityTests<Spline>
	{
		[Fact]
		public void BoundingBoxTest()
		{
		}

		[Fact]
		public void CheckIsCloseFlag()
		{
			Spline spline = new Spline();

			spline.IsClosed = true;

			Assert.True(spline.Flags.HasFlag(SplineFlags.Closed));
			Assert.True(spline.Flags1.HasFlag(SplineFlags1.Closed));

			spline.IsClosed = false;

			Assert.False(spline.Flags.HasFlag(SplineFlags.Closed));
			Assert.False(spline.Flags1.HasFlag(SplineFlags1.Closed));
		}
	}
}