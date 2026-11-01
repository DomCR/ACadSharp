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

	[Fact]
	public void ABulgeOverAZeroLengthSegmentDoesNotThrow()
	{
		//A repeated vertex that still carries a bulge is an arc over a chord of no length.
		//There is no radius to recover, and Arc refuses the zero its own maths produces - so
		//one such vertex, which real exports are full of, used to discard the whole polyline.
		LwPolyline lwPolyline = new LwPolyline();
		lwPolyline.Vertices.Add(new LwPolyline.Vertex(new XY(0, 0)));
		lwPolyline.Vertices.Add(new LwPolyline.Vertex(new XY(10, 0)) { Bulge = 0.5 });
		lwPolyline.Vertices.Add(new LwPolyline.Vertex(new XY(10, 0)));
		lwPolyline.Vertices.Add(new LwPolyline.Vertex(new XY(10, 8)));

		BoundingBox box = lwPolyline.GetBoundingBox();

		Assert.Equal(0.0, box.Min.X, 6);
		Assert.Equal(0.0, box.Min.Y, 6);
		Assert.Equal(10.0, box.Max.X, 6);
		Assert.Equal(8.0, box.Max.Y, 6);
	}

	public override void GetBoundingBoxTest()
	{
	}
}
