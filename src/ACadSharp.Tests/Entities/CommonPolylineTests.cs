using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public abstract class CommonPolylineTests<T, R> : CommonEntityTests<T>
		where T : Polyline<R>, new()
		where R : Vertex, new()
	{
		[Fact]
		public void ClearVertrticesTest()
		{
			CadDocument doc = new CadDocument();

			IEnumerable<R> vertices = this.createVertices();
			T polyline = this.createPolyline(vertices);

			doc.Entities.Add(polyline);

			polyline.Vertices.Clear();
			foreach (var item in vertices)
			{
				Assert.True(item.Handle == 0);
				Assert.False(doc.TryGetCadObject(item.Handle, out R _));
			}
		}

		[Fact]
		public override void CloneTest()
		{
			CadDocument doc = new CadDocument();

			IEnumerable<R> vertices = this.createVertices();
			T polyline = this.createPolyline(vertices);

			doc.Entities.Add(polyline);

			T clone = polyline.Clone() as T;

			CadObjectTestUtils.AssertEntityClone(polyline, clone);
			CadObjectTestUtils.AssertEntityCollection(polyline.Vertices, clone.Vertices);
		}

		public override void GetBoundingBoxTest()
		{
			IEnumerable<R> vertices = this.createVertices();
			T polyline = this.createPolyline(vertices);

			var box = polyline.GetBoundingBox();

			var box1 = BoundingBox.FromPoints(vertices.Select(v => v.Location));

			var minx = vertices.Select(v => v.Location.X).Min();
			var miny = vertices.Select(v => v.Location.Y).Min();
			var minz = vertices.Select(v => v.Location.Z).Min();

			var maxx = vertices.Select(v => v.Location.X).Max();
			var maxy = vertices.Select(v => v.Location.Y).Max();
			var maxz = vertices.Select(v => v.Location.Z).Max();

			Assert.Equal(new XYZ(minx, miny, minz), box.Min);
			Assert.Equal(new XYZ(maxx, maxy, maxz), box.Max);
		}

		[Fact]
		public void IsClosedTest()
		{
			IEnumerable<R> vertices = this.createVertices();
			T polyline = this.createPolyline(vertices);

			polyline.IsClosed = true;

			Assert.True(polyline.IsClosed);
			Assert.True(polyline.Flags.HasFlag(PolylineFlags.ClosedPolylineOrClosedPolygonMeshInM));
			Assert.True(polyline.Flags.HasFlag(PolylineFlags.ClosedPolygonMeshInN));
		}

		protected IEnumerable<R> createVertices(int amount = 5)
		{
			List<R> vertices = new List<R>();
			for (int i = 0; i < amount; i++)
			{
				R v = new R();
				v.Location = this._random.NextXYZ();
				vertices.Add(v);
			}
			return vertices;
		}

		protected T createPolyline(IEnumerable<R> vertices)
		{
			T polyline = new T();
			polyline.Vertices.AddRange(vertices);
			return polyline;
		}
	}
}