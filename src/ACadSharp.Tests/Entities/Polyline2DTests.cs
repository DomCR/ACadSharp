using ACadSharp.Entities;
using Xunit;

namespace ACadSharp.Tests.Entities;

public class Polyline2DTests : CommonPolylineTests<Polyline2D, Vertex2D>
{
	[Fact]
	public void ContainsTypeFlagTest()
	{
		Polyline2D polyline = new Polyline2D();

		Assert.False(polyline.Flags.HasFlag(PolylineFlags.Polyline3D));
		Assert.False(polyline.Flags.HasFlag(PolylineFlags.PolygonMesh));
		Assert.False(polyline.Flags.HasFlag(PolylineFlags.PolyfaceMesh));
	}

	[Fact]
	public void GetBoundingBoxIncludesTheWidthTheVertexesCarry()
	{
		//AutoCAD's PLINE with a width of 150 measures 75 to each side of the centre line, and its
		//extents say so; the old-style POLYLINE carries the width on the vertexes rather than in a
		//single field, so both routes have to add it.
		Polyline2D polyline = new Polyline2D();
		polyline.Vertices.Add(new Vertex2D(new CSMath.XY(0, 0)) { StartWidth = 150, EndWidth = 150 });
		polyline.Vertices.Add(new Vertex2D(new CSMath.XY(1000, 0)) { StartWidth = 150, EndWidth = 150 });
		polyline.Vertices.Add(new Vertex2D(new CSMath.XY(1000, 1000)) { StartWidth = 150, EndWidth = 150 });

		CSMath.BoundingBox box = polyline.GetBoundingBox();

		Assert.Equal(new CSMath.XYZ(0, -75, 0), box.Min);
		Assert.Equal(new CSMath.XYZ(1075, 1000, 0), box.Max);
	}

}