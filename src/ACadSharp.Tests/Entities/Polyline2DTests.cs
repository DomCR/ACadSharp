using ACadSharp.Entities;
using Xunit;

namespace ACadSharp.Tests.Entities
{
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
	}
}