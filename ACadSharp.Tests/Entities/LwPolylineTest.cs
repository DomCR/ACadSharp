using ACadSharp.Entities;
using CSMath;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class LwPolylineTest
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

		public LwPolylineTest()
		{
			//Square
			_lines = new List<Line>
			{
				new Line{StartPoint = _points[0], EndPoint = _points[1]},
				new Line{StartPoint = _points[1], EndPoint = _points[2]},
				new Line{StartPoint = _points[2], EndPoint = _points[3]},
				new Line{StartPoint = _points[3], EndPoint = _points[0]},
			};
		}

		[Fact]
		public void ExplodeInLines()
		{
			LwPolyline lwPolyline = new LwPolyline();
			for (int i = 0; i < _points.Length; i++)
			{
				lwPolyline.Vertices.Add(new LwPolyline.Vertex((XY)_points[i]));
			}

			foreach (Entity item in lwPolyline.Explode())
			{
				Assert.IsType<Line>(item);

				Line l = item as Line;
				var result = _lines.FirstOrDefault(o =>
				o.StartPoint == l.StartPoint &&
				o.EndPoint == l.EndPoint);

				Assert.NotNull(result);
			}
		}


		[Fact]
		public void ExplodeInLinesAndArcs()
		{
			LwPolyline lwPolyline = new LwPolyline();
			for (int i = 0; i < _points.Length; i++)
			{
				lwPolyline.Vertices.Add(new LwPolyline.Vertex((XY)_points[i]));
			}

			//Curve the last arc
			lwPolyline.Vertices[lwPolyline.Vertices.Count - 2].Bulge = 1.0;

			foreach (Entity item in lwPolyline.Explode())
			{
				Entity result = null;
				if (item is Line l)
				{
					result = _lines.FirstOrDefault(o =>
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

			for (int i = 0; i < _points.Length; i++)
			{
				lwPolyline.Vertices.Add(new LwPolyline.Vertex((XY)_points[i]));
			}

			foreach (Entity item in lwPolyline.Explode())
			{
				Assert.IsType<Line>(item);

				Line l = item as Line;
				var result = _lines.FirstOrDefault(o =>
				o.StartPoint == l.StartPoint &&
				o.EndPoint == l.EndPoint);

				Assert.NotNull(result);
			}
		}
	}
}
