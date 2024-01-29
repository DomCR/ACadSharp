using ACadSharp.Entities;
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

			Polyline2D polyline = new Polyline2D();
			List<Vertex2D> vertices = new List<Vertex2D>
			{
				new Vertex2D(),
				new Vertex2D(new XY(1,1)),
				new Vertex2D(new XY(2,2))
			};
			polyline.Vertices.AddRange(vertices);

			doc.Entities.Add(polyline);

			polyline.Vertices.Clear();

			foreach (var item in vertices)
			{
				Assert.True(item.Handle == 0);
				Assert.False(doc.TryGetCadObject(item.Handle, out Vertex2D _));
			}
		}
	}
}
