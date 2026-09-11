using ACadSharp.Entities;
using ACadSharp.Extensions;
using CSMath;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Entities;

public class LwPolylineTests : CommonEntityTests<LwPolyline>
{
	private XYZ[] _points = new XYZ[]
		{
			new XYZ(0,0,0),
			new XYZ(0,1,0),
			new XYZ(1,1,0),
			new XYZ(1,0,0),
		};

	private List<Line> _lines;

	private Arc _arc = new Arc
	{
		Radius = 0.5,
		Center = new XYZ(1, 0.5, 0)
	};

	public LwPolylineTests()
	{
		//Square
		this._lines = new List<Line>
		{
			new Line{StartPoint = this._points[0], EndPoint = this._points[1]},
			new Line{StartPoint = this._points[1], EndPoint = this._points[2]},
			new Line{StartPoint = this._points[2], EndPoint = this._points[3]},
			new Line{StartPoint = this._points[3], EndPoint = this._points[0]},
		};
	}

	[Fact]
	public void ExplodeInLines()
	{
		LwPolyline lwPolyline = new LwPolyline();
		for (int i = 0; i < this._points.Length; i++)
		{
			lwPolyline.Vertices.Add(new LwPolyline.Vertex((XY)this._points[i]));
		}

		foreach (Entity item in lwPolyline.Explode())
		{
			Assert.IsType<Line>(item);

			Line l = item as Line;
			var result = this._lines.FirstOrDefault(o =>
			o.StartPoint == l.StartPoint &&
			o.EndPoint == l.EndPoint);

			Assert.NotNull(result);
		}
	}

	[Fact]
	public void ExplodeInLinesAndArcs()
	{
		LwPolyline lwPolyline = new LwPolyline();
		for (int i = 0; i < this._points.Length; i++)
		{
			lwPolyline.Vertices.Add(new LwPolyline.Vertex((XY)this._points[i]));
		}

		//Curve the last arc
		lwPolyline.Vertices[lwPolyline.Vertices.Count - 2].Bulge = 1.0;

		foreach (Entity item in lwPolyline.Explode())
		{
			Entity result = null;
			if (item is Line l)
			{
				result = this._lines.FirstOrDefault(o =>
					o.StartPoint == l.StartPoint &&
					o.EndPoint == l.EndPoint);
			}
			else if (item is Arc a)
			{
				Assert.Equal(this._arc.Center, a.Center);
				Assert.Equal(this._arc.Radius, a.Radius);
				continue;
			}

			Assert.NotNull(result);
		}
	}

	[Fact]
	public void ExplodeClosedInLines()
	{
		LwPolyline lwPolyline = new LwPolyline();
		lwPolyline.Flags |= LwPolylineFlags.Closed;

		for (int i = 0; i < this._points.Length; i++)
		{
			lwPolyline.Vertices.Add(new LwPolyline.Vertex((XY)this._points[i]));
		}

		foreach (Entity item in lwPolyline.Explode())
		{
			Assert.IsType<Line>(item);

			Line l = item as Line;
			var result = this._lines.FirstOrDefault(o =>
			o.StartPoint == l.StartPoint &&
			o.EndPoint == l.EndPoint);

			Assert.NotNull(result);
		}
	}

	public override void GetBoundingBoxTest()
	{
		LwPolyline lwPolyline = new LwPolyline();
		foreach (XYZ p in this._points)
		{
			lwPolyline.Vertices.Add(new LwPolyline.Vertex((XY)p));
		}

		BoundingBox box = lwPolyline.GetBoundingBox();

		Assert.Equal(new XYZ(0, 0, 0), box.Min);
		Assert.Equal(new XYZ(1, 1, 0), box.Max);

		//The vertices are stored in the polyline's own object coordinate system. AutoCAD writes a
		//(0,0,-1) normal whenever geometry is mirrored, and the world position is then the negated X -
		//not the stored one, which would place the polyline on the wrong side of the drawing.
		lwPolyline.Normal = -XYZ.AxisZ;

		box = lwPolyline.GetBoundingBox();

		Assert.Equal(new XYZ(-1, 0, 0), box.Min);
		Assert.Equal(new XYZ(0, 1, 0), box.Max);
	}

	[Fact]
	public void GetBoundingBoxIncludesAConstantWidth()
	{
		//A width is drawn half on each side of the centre line, and AutoCAD's extents include it:
		//asked for this square of width 150, AutoCAD reports (-75,-75)..(1075,1075). Measuring the
		//centre line alone left one real drawing's extents short by exactly 75 on all four sides.
		LwPolyline polyline = new LwPolyline { ConstantWidth = 150, IsClosed = true };
		polyline.Vertices.Add(new LwPolyline.Vertex(new XY(0, 0)));
		polyline.Vertices.Add(new LwPolyline.Vertex(new XY(1000, 0)));
		polyline.Vertices.Add(new LwPolyline.Vertex(new XY(1000, 1000)));
		polyline.Vertices.Add(new LwPolyline.Vertex(new XY(0, 1000)));

		BoundingBox box = polyline.GetBoundingBox();

		Assert.Equal(new XYZ(-75, -75, 0), box.Min);
		Assert.Equal(new XYZ(1075, 1075, 0), box.Max);
	}

	[Fact]
	public void GetBoundingBoxCarriesAWidthAcrossTheSegmentAndNotBeyondItsEnds()
	{
		//The width goes perpendicular to the segment; the ends are flat caps. AutoCAD agrees: for
		//these two segments tapering 0-200-400 it reports (0,-108.9)..(1200,1000), so the open ends
		//stay at their vertexes and only the joint fill reaches slightly further than measured here.
		LwPolyline polyline = new LwPolyline();
		polyline.Vertices.Add(new LwPolyline.Vertex(new XY(0, 0)) { StartWidth = 0, EndWidth = 200 });
		polyline.Vertices.Add(new LwPolyline.Vertex(new XY(1000, 0)) { StartWidth = 200, EndWidth = 400 });
		polyline.Vertices.Add(new LwPolyline.Vertex(new XY(1000, 1000)) { StartWidth = 400, EndWidth = 0 });

		BoundingBox box = polyline.GetBoundingBox();

		Assert.Equal(new XYZ(0, -100, 0), box.Min);
		Assert.Equal(new XYZ(1200, 1000, 0), box.Max);
	}

	[Fact]
	public void GetBoundingBoxKeepsTheCentreLineWithoutAWidth()
	{
		LwPolyline polyline = new LwPolyline();
		polyline.Vertices.Add(new LwPolyline.Vertex(new XY(0, 0)));
		polyline.Vertices.Add(new LwPolyline.Vertex(new XY(1000, 500)));

		BoundingBox box = polyline.GetBoundingBox();

		Assert.Equal(new XYZ(0, 0, 0), box.Min);
		Assert.Equal(new XYZ(1000, 500, 0), box.Max);
	}

}
