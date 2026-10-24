using ACadSharp.Entities;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities;

public class ImageBoundingBoxTests
{
	[Fact]
	public void ARotatedImageIsMeasuredThroughItsOwnAxes()
	{
		//Taken from a client drawing and checked against AutoCAD, which reports
		//(664987.657, -42554.232)..(675578.740, -31963.149) for this image. The boundary is in the
		//image's own pixel space; adding those pixel coordinates to the insertion point instead -
		//which is what this used to do - claims a 500 x 500 square sitting at the insertion point,
		//while the image is a 10,591-unit diamond hanging below it.
		RasterImage image = new()
		{
			InsertPoint = new XYZ(670274.87890043925, -31963.148565297, 0),
			Size = new XY(500, 500),
			UVector = new XYZ(-10.574442749672151, -10.607724733996102, 0),
			VVector = new XYZ(10.607724733996102, -10.574442749672151, 0),
		};
		image.ClipBoundaryVertices.Add(new XY(-0.5, -0.5));
		image.ClipBoundaryVertices.Add(new XY(499.5, 499.5));

		BoundingBox box = image.GetBoundingBox();

		//Two decimals: AutoCAD prints its own extents to six significant figures, so that is as
		//close as its number can be read.
		Assert.Equal(664987.657, box.Min.X, 2);
		Assert.Equal(-42554.232, box.Min.Y, 2);
		Assert.Equal(675578.740, box.Max.X, 2);
		Assert.Equal(-31963.149, box.Max.Y, 2);
	}

	[Fact]
	public void AWipeoutHangsBelowItsInsertionPointBecauseTheBoundaryCountsFromTheTop()
	{
		//The case that tells the two directions apart. A wipeout is a 1 x 1 image whose vectors span
		//the whole shape, and this boundary sits at the TOP of that unit square: AutoCAD draws the
		//900 x 400 rectangle at the insertion point, not a whole V - here 4.5 million units - below
		//it. Measured: AutoCAD reports (749966.684, -59623.691)..(750866.684, -59223.690).
		Wipeout wipeout = new()
		{
			InsertPoint = new XYZ(749966.68417709228, -59223.690436399324, 0),
			Size = new XY(1, 1),
			UVector = new XYZ(4571087.5147777516, 0, 0),
			VVector = new XYZ(0, -4571087.5147777516, 0),
		};
		wipeout.ClipBoundaryVertices.Add(new XY(-0.5, 0.5));
		wipeout.ClipBoundaryVertices.Add(new XY(-0.5, 0.49991249347147548));
		wipeout.ClipBoundaryVertices.Add(new XY(-0.49980311031082003, 0.49991249347147548));
		wipeout.ClipBoundaryVertices.Add(new XY(-0.49980311031082003, 0.5));

		BoundingBox box = wipeout.GetBoundingBox();

		Assert.Equal(749966.684, box.Min.X, 2);
		Assert.Equal(-59623.691, box.Min.Y, 2);
		Assert.Equal(750866.684, box.Max.X, 2);
		Assert.Equal(-59223.690, box.Max.Y, 2);
	}

	[Fact]
	public void ARectangularBoundaryIsSquaredOffBeforeItIsMapped()
	{
		//Two vertexes are opposite corners, and the other two have to be built before the mapping:
		//map only the diagonal and a rotated image is measured along it instead of around its edges.
		RasterImage diagonal = new()
		{
			InsertPoint = XYZ.Zero,
			Size = new XY(10, 10),
			UVector = new XYZ(1, 1, 0),
			VVector = new XYZ(-1, 1, 0),
		};
		diagonal.ClipBoundaryVertices.Add(new XY(-0.5, -0.5));
		diagonal.ClipBoundaryVertices.Add(new XY(9.5, 9.5));

		BoundingBox box = diagonal.GetBoundingBox();

		Assert.Equal(-10, box.Min.X, 9);
		Assert.Equal(0, box.Min.Y, 9);
		Assert.Equal(10, box.Max.X, 9);
		Assert.Equal(20, box.Max.Y, 9);
	}

	[Fact]
	public void AnImageWithNoStoredBoundaryStillCoversItsOwnPixels()
	{
		//An unclipped image may carry no boundary at all in memory; the whole-image rectangle is
		//what AutoCAD writes for it, and it is what the image covers.
		RasterImage image = new()
		{
			InsertPoint = new XYZ(100, 200, 0),
			Size = new XY(4, 3),
			UVector = new XYZ(2, 0, 0),
			VVector = new XYZ(0, 2, 0),
		};

		BoundingBox box = image.GetBoundingBox();

		Assert.Equal(new XYZ(100, 200, 0), box.Min);
		Assert.Equal(new XYZ(108, 206, 0), box.Max);
	}
}
