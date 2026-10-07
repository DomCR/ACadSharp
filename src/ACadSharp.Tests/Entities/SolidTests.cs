using ACadSharp.Entities;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class SolidTests : CommonEntityTests<Solid>
	{
		[Fact]
		public override void GetBoundingBoxTest()
		{
			Solid solid = new Solid
			{
				FirstCorner = new XYZ(1, 1, 5),
				SecondCorner = new XYZ(3, 1, 5),
				ThirdCorner = new XYZ(1, 2, 5),
				FourthCorner = new XYZ(3, 2, 5),
			};

			BoundingBox box = solid.GetBoundingBox();

			Assert.Equal(new XYZ(1, 1, 5), box.Min);
			Assert.Equal(new XYZ(3, 2, 5), box.Max);
		}

		[Fact]
		public void MirroredSolidIsBoxedThroughItsNormal()
		{
			//The corners are in the solid's own object coordinate system, so a mirrored solid -
			//the (0,0,-1) normal AutoCAD writes for one - draws at the negated X of the stored
			//values, and at the negated Z.
			Solid solid = new Solid
			{
				FirstCorner = new XYZ(1, 1, 5),
				SecondCorner = new XYZ(3, 1, 5),
				ThirdCorner = new XYZ(1, 2, 5),
				FourthCorner = new XYZ(3, 2, 5),
				Normal = new XYZ(0, 0, -1),
			};

			BoundingBox box = solid.GetBoundingBox();

			Assert.Equal(-3.0, box.Min.X, 6);
			Assert.Equal(-1.0, box.Max.X, 6);
			Assert.Equal(-5.0, box.Max.Z, 6);
		}
	}
}
