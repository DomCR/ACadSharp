using ACadSharp.Entities;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class HatchTests
	{
		[Fact]
		public void CreateHatch()
		{
			Hatch hatch = new Hatch();
			hatch.IsSolid = true;

			hatch.SeedPoints.Add(new XY());

			List<Hatch.BoundaryPath.Line> edges = new List<Hatch.BoundaryPath.Line>();

			//edges
			Hatch.BoundaryPath.Line edge1 = new Hatch.BoundaryPath.Line
			{
				Start = new CSMath.XY(0, 0),
				End = new CSMath.XY(1, 0)
			};
			edges.Add(edge1);

			Hatch.BoundaryPath.Line edge2 = new Hatch.BoundaryPath.Line
			{
				Start = new CSMath.XY(1, 0),
				End = new CSMath.XY(1, 1)
			};
			edges.Add(edge2);

			Hatch.BoundaryPath.Line edge3 = new Hatch.BoundaryPath.Line
			{
				Start = new CSMath.XY(1, 1),
				End = new CSMath.XY(0, 1)
			};
			edges.Add(edge3);

			Hatch.BoundaryPath.Line edge4 = new Hatch.BoundaryPath.Line
			{
				Start = new CSMath.XY(0, 1),
				End = new CSMath.XY(0, 0)
			};
			edges.Add(edge4);

			Hatch.BoundaryPath path = new Hatch.BoundaryPath();
			foreach (var item in edges)
			{
				path.Edges.Add(item);
			}

			hatch.Paths.Add(path);

			Assert.NotEmpty(hatch.Paths);
			Assert.NotEmpty(path.Edges);
			Assert.False(path.IsPolyline);
		}

		[Fact]
		public void CreatePolylineHatch()
		{
			Hatch hatch = new Hatch();
			hatch.IsSolid = true;

			Hatch.BoundaryPath path = new Hatch.BoundaryPath();

			Hatch.BoundaryPath.Polyline pline = new Hatch.BoundaryPath.Polyline();
			pline.Vertices.Add(new XYZ(0, 0, 0));
			pline.Vertices.Add(new XYZ(1, 0, 0));
			pline.Vertices.Add(new XYZ(1, 1, 0));
			pline.Vertices.Add(new XYZ(0, 1, 0));
			pline.Vertices.Add(new XYZ(0, 0, 0));

			path.Edges.Add(pline);
			hatch.Paths.Add(path);

			Assert.True(path.IsPolyline);
		}

		[Fact]
		public void ExplodeTest()
		{
			Hatch hatch = new Hatch();
			hatch.IsSolid = true;

			Hatch.BoundaryPath path = new Hatch.BoundaryPath();

			Hatch.BoundaryPath.Polyline pline = new Hatch.BoundaryPath.Polyline();
			pline.Vertices.Add(new XYZ(0, 0, 0));
			pline.Vertices.Add(new XYZ(1, 0, 0));
			pline.Vertices.Add(new XYZ(1, 1, 0));
			pline.Vertices.Add(new XYZ(0, 1, 0));
			pline.Vertices.Add(new XYZ(0, 0, 0));

			path.Edges.Add(pline);
			hatch.Paths.Add(path);

			var entities = hatch.Explode();

			Assert.NotEmpty(entities);
		}

		[Fact]
		public void GetBoundingBoxTest()
		{
			Hatch hatch = new Hatch();
			hatch.IsSolid = true;

			Hatch.BoundaryPath path = new Hatch.BoundaryPath();

			Hatch.BoundaryPath.Polyline pline = new Hatch.BoundaryPath.Polyline();
			pline.Vertices.Add(new XYZ(0, 0, 0));
			pline.Vertices.Add(new XYZ(1, 0, 0));
			pline.Vertices.Add(new XYZ(1, 1, 0));
			pline.Vertices.Add(new XYZ(0, 1, 0));
			pline.Vertices.Add(new XYZ(0, 0, 0));

			path.Edges.Add(pline);

			hatch.Paths.Add(path);

			var box = hatch.GetBoundingBox();

			Assert.Equal(new XYZ(0, 0, 0), box.Min);
			Assert.Equal(new XYZ(1, 1, 0), box.Max);
		}

		[Fact]
		public void PolylineHatchNotAllowMoreEdges()
		{
			Hatch hatch = new Hatch();
			hatch.IsSolid = true;

			Hatch.BoundaryPath path = new Hatch.BoundaryPath();

			Hatch.BoundaryPath.Polyline pline = new Hatch.BoundaryPath.Polyline();
			pline.Vertices.Add(new XYZ(0, 0, 0));
			pline.Vertices.Add(new XYZ(1, 0, 0));
			pline.Vertices.Add(new XYZ(1, 1, 0));
			pline.Vertices.Add(new XYZ(0, 1, 0));
			pline.Vertices.Add(new XYZ(0, 0, 0));

			path.Edges.Add(pline);

			Assert.Throws<InvalidOperationException>(() =>
			{
				path.Edges.Add(new Hatch.BoundaryPath.Line());
			}
			);

			Assert.Throws<InvalidOperationException>(() =>
			{
				path.Edges.Add(new Hatch.BoundaryPath.Polyline());
			}
			);
		}

		[Fact]
		public void UpdatePattern()
		{
			Hatch hatch = new Hatch();
			var pattern = new HatchPattern("custom");
			hatch.Pattern = pattern;

			pattern.Lines.Add(new HatchPattern.Line
			{
				Angle = 0,
				Offset = new XY(-1, 1),
				DashLengths = { 0.5, 0.5 }
			});

			hatch.PatternScale = 2;

			var line = pattern.Lines.First();
			Assert.Equal(new XY(-2, 2), line.Offset);
			foreach (var item in line.DashLengths)
			{
				Assert.Equal(1, item);
			}

			hatch.PatternAngle = MathHelper.HalfPI;
			Assert.Equal(MathHelper.HalfPI, line.Angle);
		}
	}
}