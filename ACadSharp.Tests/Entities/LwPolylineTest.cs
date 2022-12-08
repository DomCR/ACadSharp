using ACadSharp.Entities;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class LwPolylineTest
	{
		[Fact]
		public void ExplodeInLines()
		{
			XYZ[] points = new XYZ[]
			{
				new XYZ(0,0,0),
				new XYZ(0,1,0),
				new XYZ(1,1,0),
				new XYZ(1,0,0),
			};

			//Square
			List<Line> lines = new List<Line>
			{
				new Line{StartPoint = points[0], EndPoint = points[1]},
				new Line{StartPoint = points[1], EndPoint = points[2]},
				new Line{StartPoint = points[2], EndPoint = points[3]},
				new Line{StartPoint = points[3], EndPoint = points[0]},
			};

			LwPolyline lwPolyline = new LwPolyline();
			for (int i = 0; i < points.Length; i++)
			{
				lwPolyline.Vertices.Add(new LwPolyline.Vertex((XY)points[i]));
			}

			foreach (Entity item in lwPolyline.Explode())
			{
				Assert.IsType<Line>(item);

				Line l = item as Line;
				var result = lines.FirstOrDefault(o =>
				o.StartPoint == l.StartPoint &&
				o.EndPoint == l.EndPoint);

				Assert.NotNull(result);
			}
		}
	}
}
