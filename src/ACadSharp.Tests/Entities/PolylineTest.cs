using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using System.Collections.Generic;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class PolylineTests
	{
		[Fact]
		public void ClearVertrticesTest()
		{
			CadDocument doc = new CadDocument();

			List<Vertex2D> vertices = this.createVertices2DMock();
			Polyline2D polyline = this.createPolyline2DMock(vertices);

			doc.Entities.Add(polyline);

			polyline.Vertices.Clear();
			foreach (var item in vertices)
			{
				Assert.True(item.Handle == 0);
				Assert.False(doc.TryGetCadObject(item.Handle, out Vertex2D _));
			}
		}

		[Fact]
		public void CloneTest()
		{
			CadDocument doc = new CadDocument();

			List<Vertex2D> vertices = this.createVertices2DMock();
			Polyline2D polyline = this.createPolyline2DMock(vertices);

			doc.Entities.Add(polyline);

			Polyline2D clone = polyline.Clone() as Polyline2D;

			CadObjectTestUtils.AssertEntityClone(polyline, clone);
			CadObjectTestUtils.AssertEntityCollection(polyline.Vertices, clone.Vertices);
		}

		[Fact]
		public void IsClosedTest()
		{
			List<Vertex2D> vertices = this.createVertices2DMock();
			var polyline = this.createPolyline2DMock(vertices);

			polyline.IsClosed = true;

			Assert.True(polyline.IsClosed);
			Assert.True(polyline.Flags.HasFlag(PolylineFlags.ClosedPolylineOrClosedPolygonMeshInM));
			Assert.True(polyline.Flags.HasFlag(PolylineFlags.ClosedPolygonMeshInN));
		}

		private Polyline2D createPolyline2DMock(List<Vertex2D> vertices)
		{
			Polyline2D polyline = new Polyline2D();
			if (vertices == null)
			{
				vertices = this.createVertices2DMock();
			}

			polyline.Vertices.AddRange(vertices);

			return polyline;
		}

		private List<Vertex2D> createVertices2DMock()
		{
			return new List<Vertex2D>
			{
				new Vertex2D(),
				new Vertex2D(new XY(1,1)),
				new Vertex2D(new XY(2,2))
			};
		}
	}
}