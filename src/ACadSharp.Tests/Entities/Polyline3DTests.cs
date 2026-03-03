using ACadSharp.Entities;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class Polyline3DTests : CommonPolylineTests<Polyline3D, Vertex3D>
	{
		[Fact]
		public void ContainsTypeFlagTest()
		{
			Polyline3D polyline = new Polyline3D();

			Assert.True(polyline.Flags.HasFlag(PolylineFlags.Polyline3D));
			Assert.False(polyline.Flags.HasFlag(PolylineFlags.PolygonMesh));
			Assert.False(polyline.Flags.HasFlag(PolylineFlags.PolyfaceMesh));
		}
	}
}